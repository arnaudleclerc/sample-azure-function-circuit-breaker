using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Functions.Extensions.CircuitBreaker.Internal
{
	internal class CircuitBreakerStateService : ICircuitBreakerStateService
	{
		private const string TABLE_STATE = "state";
		private readonly CloudStorageAccount _cloudStorageAccount;

		public CircuitBreakerStateService(string storageAccountConnectionString)
		{
			if (!CloudStorageAccount.TryParse(storageAccountConnectionString, out var account))
			{
				throw new ArgumentException(nameof(storageAccountConnectionString));
			}

			_cloudStorageAccount = account;
		}

		public async Task<CircuitBreakerState> GetStateAsync(string serviceName, string functionName)
		{
			var table = _cloudStorageAccount.CreateCloudTableClient().GetTableReference(TABLE_STATE);

			CircuitBreakerFunctionState functionState = (await table.ExecuteAsync(TableOperation.Retrieve<CircuitBreakerFunctionState>(serviceName, functionName))).Result as CircuitBreakerFunctionState;
			return functionState == null ? null : CircuitBreakerState.FromState(functionState.State);
		}

		public async Task CloseCircuitAsync(string serviceName, string functionName)
		{
			var table = _cloudStorageAccount.CreateCloudTableClient().GetTableReference(TABLE_STATE);

			await table.ExecuteAsync(TableOperation.InsertOrMerge(new CircuitBreakerFunctionState
			{
				PartitionKey = serviceName,
				RowKey = functionName,
				State = CircuitBreakerState.Closed.ToString()
			}));
		}

		public async Task OpenCircuitAsync(string serviceName, string functionName)
		{
			var table = _cloudStorageAccount.CreateCloudTableClient().GetTableReference(TABLE_STATE);

			await table.ExecuteAsync(TableOperation.Replace(new CircuitBreakerFunctionState
			{
				PartitionKey = serviceName,
				RowKey = functionName,
				State = CircuitBreakerState.Open.ToString(),
				ETag = "*"
			}));
		}

		public async Task HalfOpenCircuitAsync(string serviceName, string functionName)
		{
			var table = _cloudStorageAccount.CreateCloudTableClient().GetTableReference(TABLE_STATE);

			await table.ExecuteAsync(TableOperation.Replace(new CircuitBreakerFunctionState
			{
				PartitionKey = serviceName,
				RowKey = functionName,
				State = CircuitBreakerState.HalfOpen.ToString(),
				ETag = "*"
			}));
		}

		public async Task<IEnumerable<CircuitBreakerFunctionState>> GetOpenCircuits()
		{
			var table = _cloudStorageAccount.CreateCloudTableClient().GetTableReference(TABLE_STATE);
			return (await table.ExecuteQuerySegmentedAsync(
				new TableQuery<CircuitBreakerFunctionState>().Where(TableQuery.GenerateFilterCondition("State", QueryComparisons.Equal, CircuitBreakerState.Open.ToString())),
				null)).Results;
		}
	}
}
