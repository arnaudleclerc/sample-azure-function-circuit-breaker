using Functions.Extensions.CircuitBreaker;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

[assembly: FunctionsStartup(typeof(Functions.CircuitBreaker.Startup))]
namespace Functions.CircuitBreaker
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			var config = new ConfigurationBuilder()
					.SetBasePath(Environment.CurrentDirectory)
					.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
					.AddEnvironmentVariables()
					.Build();

			builder.Services.AddCircuitBreaker(config["CircuitBreakerStorageAccountConnectionString"]);
		}
	}
}
