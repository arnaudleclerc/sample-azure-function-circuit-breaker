using Functions.Extensions.CircuitBreaker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Threading.Tasks;

namespace Functions.Sample.API
{
	public class FailOnOddMinutesAPITrigger
	{
		[FunctionName("FailOnOddMinutesAPITrigger")]
		[CircuitBreaker("Samples", 3, 60000, 30000, 5)]
		public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/failonoddminutes")] HttpRequest req)
		{
			if(DateTime.Now.Minute % 2 == 1)
			{
				throw new Exception("That's odd");
			}

			return new NoContentResult();
		}
	}
}
