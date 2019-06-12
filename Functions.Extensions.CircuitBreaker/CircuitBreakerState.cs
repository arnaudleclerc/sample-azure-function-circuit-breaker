using System;

namespace Functions.Extensions.CircuitBreaker
{
	public class CircuitBreakerState
	{
		public static CircuitBreakerState Closed = new CircuitBreakerState("closed");
		public static CircuitBreakerState Open = new CircuitBreakerState("open");
		public static CircuitBreakerState HalfOpen = new CircuitBreakerState("halfopen");

		private readonly string _name;

		public DateTimeOffset Timestamp
		{
			get; private set;
		}

		public bool IsClosed => _name == "closed";

		public bool IsOpen => _name == "open";

		public bool IsHalfOpen => _name == "halfopen";

		private CircuitBreakerState(string name)
		{
			_name = name;
		}

		internal static CircuitBreakerState FromState(CircuitBreakerFunctionState functionState)
		{
			CircuitBreakerState result = null;
			if (functionState != null)
			{
				if (functionState.State == Closed.ToString())
				{
					result = Closed;
				}

				if (functionState.State == Open.ToString())
				{
					result = Open;
				}

				if (functionState.State == HalfOpen.ToString())
				{
					result = HalfOpen;
				}

				result.Timestamp = functionState.Timestamp;
			}
			
			return result;
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
