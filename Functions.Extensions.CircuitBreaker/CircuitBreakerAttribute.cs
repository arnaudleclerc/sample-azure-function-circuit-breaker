using Functions.Extensions.CircuitBreaker.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Functions.Extensions.CircuitBreaker
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CircuitBreakerAttribute : FunctionInvocationFilterAttribute
	{
		private readonly string _serviceName;
		private readonly int _failureRequestThreshold;
		private readonly int _failureRequestThresholdMilliseconds;
		private readonly int _openTimeoutExpireMilliseconds;
		private readonly int _halfOpenSuccessThreshold;

		private CircuitBreakerState _state;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="serviceName">Name of the service this function belongs to</param>
		/// <param name="failureRequestThreshold">Number of failed request which will trigger the open state</param>
		/// <param name="failureRequestThresholdMilliseconds">TimeSpan in milliseconds to evaluate the number of failed requests while being in a closed state</param>
		/// <param name="openTimeoutExpireMilliseconds">TimeSpan in milliseconds during which the open state will stay activated</param>
		/// <param name="halfOpenSuccessThreshold">While being in a half open state, number of succeeded requests necessary to close the circuit</param>
		public CircuitBreakerAttribute(string serviceName, int failureRequestThreshold, int failureRequestThresholdMilliseconds, int openTimeoutExpireMilliseconds, int halfOpenSuccessThreshold)
		{
			_serviceName = serviceName;
			_failureRequestThreshold = failureRequestThreshold;
			_failureRequestThresholdMilliseconds = failureRequestThresholdMilliseconds;
			_openTimeoutExpireMilliseconds = openTimeoutExpireMilliseconds;
			_halfOpenSuccessThreshold = halfOpenSuccessThreshold;
		}

		public override Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
		{
			var request = executingContext.Arguments.Values.FirstOrDefault(argument => argument is HttpRequest) as HttpRequest;
			var stateReaderService = request.HttpContext.RequestServices.GetRequiredService<ICircuitBreakerStateService>();

			_state = stateReaderService.GetStateAsync(_serviceName, executingContext.FunctionName).Result;

			if (_state != null && _state.IsOpen)
			{
				request.HttpContext.Response.StatusCode = 403;
				request.HttpContext.Response.Body.Flush();
				request.HttpContext.Response.Body.Close();
				throw new OpenCircuitException(_serviceName, executingContext.FunctionName);
			}

			return base.OnExecutingAsync(executingContext, cancellationToken);
		}

		public override Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
		{
			if (_state == null || !_state.IsOpen)
			{
				var request = executedContext.Arguments.Values.FirstOrDefault(argument => argument is HttpRequest) as HttpRequest;
				var queueService = request.HttpContext.RequestServices.GetRequiredService<CircuitBreakerQueueService>();

				queueService.AddMessageAsync(new CircuitBreakerMessage
				{
					ServiceName = _serviceName,
					FailureRequestThreshold = _failureRequestThreshold,
					FailureRequestThresholdMilliseconds = _failureRequestThresholdMilliseconds,
					FunctionName = executedContext.FunctionName,
					HalfOpenSuccessThreshold = _halfOpenSuccessThreshold,
					IsSuccess = executedContext.FunctionResult.Succeeded,
					OpenTimeoutExpireMilliseconds = _openTimeoutExpireMilliseconds,
					LoggedTime = DateTime.Now
				}).Wait();
			}

			return base.OnExecutedAsync(executedContext, cancellationToken);
		}
	}
}
