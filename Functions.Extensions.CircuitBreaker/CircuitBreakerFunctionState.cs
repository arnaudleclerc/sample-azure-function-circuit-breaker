using Microsoft.Azure.Cosmos.Table;

namespace Functions.Extensions.CircuitBreaker
{
	public class CircuitBreakerFunctionState : TableEntity
	{
		public string State { get; set; }
	}
}
