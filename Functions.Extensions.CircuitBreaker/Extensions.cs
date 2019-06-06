using Functions.Extensions.CircuitBreaker.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Functions.Extensions.CircuitBreaker
{
	public static class Extensions
	{
		public static IServiceCollection AddCircuitBreaker(this IServiceCollection services, string circuitBreakerStorageAccountConnectionString)
		{
			return services.AddTransient(sp => new CircuitBreakerQueueService(circuitBreakerStorageAccountConnectionString));
		}
	}
}
