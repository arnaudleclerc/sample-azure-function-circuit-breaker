using Microsoft.Azure.Cosmos.Table;
using System;
using System.Threading.Tasks;

namespace Functions.Extensions.CircuitBreaker.Internal
{
	internal class CircuitBreakerStateReaderService
	{
		private readonly CloudStorageAccount _cloudStorageAccount;

		protected readonly string tableName = "circuitbreakerstate";

		internal CircuitBreakerStateReaderService(string storageAccountConnectionString)
		{
			if(!CloudStorageAccount.TryParse(storageAccountConnectionString, out var account))
			{
				throw new ArgumentException(nameof(storageAccountConnectionString));
			}

			_cloudStorageAccount = account;
		}

		public async Task<CircuitBreakerState> GetStateAsync(string serviceName, string functionName)
		{
			var tableClient = _cloudStorageAccount.CreateCloudTableClient();
			var table = tableClient.GetTableReference(tableName);

			CircuitBreakerFunctionState functionState = (await table.ExecuteAsync(TableOperation.Retrieve<CircuitBreakerFunctionState>(serviceName, functionName))).Result as CircuitBreakerFunctionState;
			return functionState == null ? null : CircuitBreakerState.FromState(functionState.State);
		}
	}
}
