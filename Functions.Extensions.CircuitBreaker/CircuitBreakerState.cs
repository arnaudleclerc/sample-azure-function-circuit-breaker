namespace Functions.Extensions.CircuitBreaker
{
	public class CircuitBreakerState
	{
		public static CircuitBreakerState Closed = new CircuitBreakerState("closed");
		public static CircuitBreakerState Open = new CircuitBreakerState("open");
		public static CircuitBreakerState HalfOpen = new CircuitBreakerState("halfopen");

		private readonly string _name;

		public bool IsClosed => _name == "closed";

		public bool IsOpen => _name == "open";

		public bool IsHalfOpen => _name == "halfopen";

		private CircuitBreakerState(string name)
		{
			_name = name;
		}

		internal static CircuitBreakerState FromState(string state)
		{
			if(state == Closed.ToString())
			{
				return Closed;
			}

			if(state == Open.ToString())
			{
				return Open;
			}

			if(state == HalfOpen.ToString())
			{
				return HalfOpen;
			}

			return null;
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
