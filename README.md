A simple circuit breaker implementation for .NET.

+ Forked and updated from Tatham Oddie's Reliability Patterns solution on GitHub: https://github.com/tathamoddie/reliability-patterns

Taking advantage of the library is as simple as wrapping your outgoing service call with circuitBreaker.Execute:

```cs
// Note: you'll need to keep this instance around
var breaker = new CircuitBreaker();
 
var client = new SmtpClient();
var message = new MailMessage();
breaker.Execute(() => client.SendEmail(message));
```

You can also take advantage of built-in retry logic:

```cs
breaker.ExecuteWithRetries(() => client.SendEmail(message), 10, TimeSpan.FromSeconds(20));
```