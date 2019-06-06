using Microsoft.Azure.Cosmos.Table;

namespace Functions.CircuitBreaker.Models
{
	public class ConfigurationEntity : TableEntity
	{
		public int FailureRequestThreshold { get; set; }
		public int FailureRequestThresholdMilliseconds { get; set; }
		public double OpenTimeoutExpireMilliseconds { get; set; }
		public int HalfOpenSuccessThreshold { get; set; }
	}
}
