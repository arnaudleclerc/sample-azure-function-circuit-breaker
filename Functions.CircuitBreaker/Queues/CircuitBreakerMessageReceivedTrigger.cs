using Functions.CircuitBreaker.Services;
using Functions.Extensions.CircuitBreaker;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;

namespace Functions.CircuitBreaker.Queues
{
	public class CircuitBreakerMessageReceivedTrigger
	{
		private readonly CircuitBreakerRepository _circuitBreakerRepository;

		public CircuitBreakerMessageReceivedTrigger(CircuitBreakerRepository circuitBreakerRepository)
		{
			_circuitBreakerRepository = circuitBreakerRepository;
		}

		public async Task Run([QueueTrigger("circuitbreaker", Connection = "CircuitBreakerStorageAccountConnectionString")]CircuitBreakerMessage message)
		{
			await _circuitBreakerRepository.UpdateConfigurationAsync(message);
			await _circuitBreakerRepository.AddOperationAsync(message);
			await _circuitBreakerRepository.UpdateStateAsync(message.ServiceName, message.FunctionName);
		}
	}
}
