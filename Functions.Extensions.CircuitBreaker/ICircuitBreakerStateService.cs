using Functions.Extensions.CircuitBreaker.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Functions.Extensions.CircuitBreaker
{
	public interface ICircuitBreakerStateService
	{
		Task<CircuitBreakerState> GetStateAsync(string serviceName, string functionName);
		Task CloseCircuitAsync(string serviceName, string functionName);
		Task OpenCircuitAsync(string serviceName, string functionName);
		Task<IEnumerable<CircuitBreakerFunctionState>> GetOpenCircuits();
		Task HalfOpenCircuitAsync(string serviceName, string functionName);
	}
}
