# Sample implementation of a Circuit Breaker pattern with Azure Functions

## Circuit breaker

Implementing a circuit breaker prevent calling an operation which is likely to fail. More information on this pattern [here](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker).

## Implementation

This repositories contains two Azure functions and one extensions library.

- The `Functions.Extensions.CircuitBreaker` library contains the attribute to append to the azure function implementing the circuit breaker and some services to interact with the state of a circuit. 
The `CircuitBreakerAttribute` checks if the circuit is opened before executing the function and returns a 403 in this case. When the function has been executed, it pushes a message to a queue to notify of the result of the function. This message will be handled by the `Functions.Circuit` breaker function.

- The `Functions.CircuitBreaker` project is an Azure function making the state of the circuit evolve. It contains one queue trigger which is executed when a message is added to a queue by the `CircuitBreakerAttribute` and a timer trigger to handle the transition to the half-open state.

- The `Functions.Sample` project is an Azure function exposing one API failing on odd minutes. This API is marked with a `CircuitBreakerAttribute` which defines the configuration to use for the circuit of this function.
