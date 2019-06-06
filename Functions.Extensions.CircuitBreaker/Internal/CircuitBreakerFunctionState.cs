using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Functions.Extensions.CircuitBreaker.Internal
{
	internal class CircuitBreakerFunctionState : TableEntity
	{
		public string State { get; set; }
	}
}
