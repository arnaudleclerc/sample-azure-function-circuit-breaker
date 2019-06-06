using Functions.CircuitBreaker.Models;
using Functions.Extensions.CircuitBreaker;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Functions.CircuitBreaker.Services
{
	public class CircuitBreakerRepository
	{
		private const string TABLE_CONFIGURATION = "configuration";
		private const string TABLE_OPERATION = "operation";

		private readonly ICircuitBreakerStateService _circuitBreakerStateService;
		private readonly CloudStorageAccount _storageAccount;

		public CircuitBreakerRepository(ICircuitBreakerStateService circuitBreakerStateService,
			string circuitBreakerStorageAccountConnectionString)
		{
			if (!CloudStorageAccount.TryParse(circuitBreakerStorageAccountConnectionString, out var account))
			{
				throw new ArgumentException(nameof(circuitBreakerStorageAccountConnectionString));
			}

			_circuitBreakerStateService = circuitBreakerStateService;
			_storageAccount = account;
		}

		public async Task UpdateConfigurationAsync(CircuitBreakerMessage message)
		{
			var tableReference = _storageAccount.CreateCloudTableClient().GetTableReference(TABLE_CONFIGURATION);

			var configurationEntity = new ConfigurationEntity
			{
				PartitionKey = message.ServiceName,
				RowKey = message.FunctionName,
				FailureRequestThreshold = message.FailureRequestThreshold,
				FailureRequestThresholdMilliseconds = message.FailureRequestThresholdMilliseconds,
				HalfOpenSuccessThreshold = message.HalfOpenSuccessThreshold,
				OpenTimeoutExpireMilliseconds = message.OpenTimeoutExpireMilliseconds
			};

			await tableReference.ExecuteAsync(TableOperation.InsertOrMerge(configurationEntity));
		}

		public async Task<ConfigurationEntity> GetCircuitConfigurationAsync(string serviceName, string functionName)
		{
			var tableReference = _storageAccount.CreateCloudTableClient().GetTableReference(TABLE_CONFIGURATION);
			return (await tableReference.ExecuteAsync(TableOperation.Retrieve<ConfigurationEntity>(serviceName, functionName))).Result as ConfigurationEntity;
		}

		public async Task AddOperationAsync(CircuitBreakerMessage message)
		{
			var tableReference = _storageAccount.CreateCloudTableClient().GetTableReference(TABLE_OPERATION);

			var operationEntity = new OperationEntity
			{
				IsSuccess = message.IsSuccess,
				PartitionKey = $"{message.ServiceName}_{message.FunctionName}",
				RowKey = message.LoggedTime.ToString()
			};

			await tableReference.ExecuteAsync(TableOperation.InsertOrMerge(operationEntity));
		}

		public async Task UpdateStateAsync(string serviceName, string functionName)
		{
			var currentState = await _circuitBreakerStateService.GetStateAsync(serviceName, functionName);

			if (currentState == null)
			{
				//We start by default with a closed state
				await _circuitBreakerStateService.CloseCircuitAsync(serviceName, functionName);
				currentState = CircuitBreakerState.Closed;
			}

			if (currentState.IsOpen)
			{
				//We do nothing here, the state should be updated via a timer
				return;
			}

			var tableClient = _storageAccount.CreateCloudTableClient();
			var configurationTableReference = tableClient.GetTableReference(TABLE_CONFIGURATION);
			var configuration = (await configurationTableReference.ExecuteAsync(TableOperation.Retrieve<ConfigurationEntity>(serviceName, functionName))).Result as ConfigurationEntity;

			var operationTableReference = tableClient.GetTableReference(TABLE_OPERATION);

			if (currentState.IsClosed)
			{
				//Do we need to open the circuit ?
				var operations = (await operationTableReference.ExecuteQuerySegmentedAsync(
					new TableQuery<OperationEntity>().Where(
						TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{serviceName}_{functionName}"),
						TableOperators.And,
						TableQuery.GenerateFilterConditionForDate("RowKey", QueryComparisons.GreaterThan, DateTime.Now.AddMilliseconds(-configuration.FailureRequestThresholdMilliseconds)
					))), null)).Results;

				if (operations.Count(o => !o.IsSuccess) >= configuration.FailureRequestThreshold)
				{
					//Yes we do
					await _circuitBreakerStateService.OpenCircuitAsync(serviceName, functionName);
				}
			}
			else
			{
				//Here, the circuit is half open. Do we need to close it ?
				var failures = (await operationTableReference.ExecuteQuerySegmentedAsync(
					new TableQuery<OperationEntity>().Where(
						TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{serviceName}_{functionName}"),
						TableOperators.And,
						TableQuery.GenerateFilterConditionForBool("IsSuccess", QueryComparisons.Equal, false
					))), null)).Results;

				//Since the latest failure, did we reach the threshold to close the circuit ?
				var latestFailure = failures.OrderByDescending(o => DateTime.Parse(o.RowKey)).FirstOrDefault();

				var successSinceLatestFailure = (await operationTableReference.ExecuteQuerySegmentedAsync(
					new TableQuery<OperationEntity>().Where(
						TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{serviceName}_{functionName}"),
						TableOperators.And,
						TableQuery.GenerateFilterConditionForDate("RowKey", QueryComparisons.GreaterThan, DateTime.Parse(latestFailure.RowKey)
					))), null)).Results;

				if(successSinceLatestFailure != null && successSinceLatestFailure.Count() >= configuration.HalfOpenSuccessThreshold)
				{
					//There we close it
					await _circuitBreakerStateService.CloseCircuitAsync(serviceName, functionName);
				}
			}
		}
	}
}
