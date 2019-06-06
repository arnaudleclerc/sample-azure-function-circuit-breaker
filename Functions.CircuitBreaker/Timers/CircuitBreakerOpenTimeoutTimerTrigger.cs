using Functions.CircuitBreaker.Services;
using Functions.Extensions.CircuitBreaker;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading.Tasks;

namespace Functions.CircuitBreaker.Timers
{
	public class CircuitBreakerOpenTimeoutTimerTrigger
	{
		private readonly ICircuitBreakerStateService _circuitBreakerStateService;
		private readonly CircuitBreakerRepository _circuitBreakerRepository;

		public CircuitBreakerOpenTimeoutTimerTrigger(ICircuitBreakerStateService circuitBreakerStateService, CircuitBreakerRepository circuitBreakerRepository)
		{
			_circuitBreakerStateService = circuitBreakerStateService;
			_circuitBreakerRepository = circuitBreakerRepository;
		}

		[FunctionName("CircuitBreakerOpenTimeoutTimerTrigger")]
		public async Task Run([TimerTrigger("*/30 * * * * *")]TimerInfo timer)
		{
			var openCircuits = await _circuitBreakerStateService.GetOpenCircuits();

			if (openCircuits != null)
			{
				foreach (var openCircuit in openCircuits)
				{
					var configuration = await _circuitBreakerRepository.GetCircuitConfigurationAsync(openCircuit.PartitionKey, openCircuit.RowKey);

					if(openCircuit.Timestamp.AddMilliseconds(configuration.OpenTimeoutExpireMilliseconds).Ticks < DateTime.Now.Ticks)
					{
						await _circuitBreakerStateService.HalfOpenCircuitAsync(openCircuit.PartitionKey, openCircuit.RowKey);
					}
				}
			}
		}
	}
}
