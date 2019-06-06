using Microsoft.Azure.Cosmos.Table;

namespace Functions.CircuitBreaker.Models
{
	public class OperationEntity : TableEntity
	{
		public bool IsSuccess { get; set; }
	}
}
