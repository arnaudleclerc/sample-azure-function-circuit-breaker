using System;

namespace Functions.Extensions.CircuitBreaker
{
	public class CircuitBreakerMessage
	{
		public string ServiceName { get; set; }
		public string FunctionName { get; set; }
		public bool IsSuccess { get; set; }
		public int FailureRequestThreshold { get; set; }
		public int FailureRequestThresholdMilliseconds { get; set; }
		public int OpenTimeoutExpireMilliseconds { get; set; }
		public int HalfOpenSuccessThreshold { get; set; }
		public DateTime LoggedTime { get; set; }
	}
}
