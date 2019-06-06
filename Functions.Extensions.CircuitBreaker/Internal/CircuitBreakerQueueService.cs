using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Functions.Extensions.CircuitBreaker.Internal
{
	internal class CircuitBreakerQueueService
	{
		private const string CIRCUIT_BREAKER_QUEUE_NAME = "circuitbreaker";

		private readonly string _storageAccountConnectionString;

		internal CircuitBreakerQueueService(string storageAccountConnectionString) => _storageAccountConnectionString = storageAccountConnectionString;

		public async Task AddMessageAsync(CircuitBreakerMessage message)
		{
			if(CloudStorageAccount.TryParse(_storageAccountConnectionString, out var account))
			{
				var queueClient = account.CreateCloudQueueClient();
				var queueReference = queueClient.GetQueueReference(CIRCUIT_BREAKER_QUEUE_NAME);
				await queueReference.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
			}
		} 
	}
}
