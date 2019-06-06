using System;

namespace Functions.Extensions.CircuitBreaker.Internal
{
	internal sealed class OpenCircuitException : Exception
	{
		public OpenCircuitException(string serviceName, string functionName): base($"The circuit of {serviceName} - {functionName} is currently open") { }
	}
}
