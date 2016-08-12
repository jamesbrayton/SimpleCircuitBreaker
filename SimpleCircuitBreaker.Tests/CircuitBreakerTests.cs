// -----------------------------------------------------------------------------
// <copyright file="CircuitBreakerTests.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the CircuitBreakerTests class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker.Tests
{
    using System;
    using System.Threading;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    ///     The circuit breaker tests.
    /// </summary>
    [TestFixture]
    public class CircuitBreakerTests
    {
        /// <summary>
        ///     The execute method should execute action once test.
        /// </summary>
        [Test]
        public void ExecuteShouldExecuteActionOnce()
        {
            var circuitBreaker = new CircuitBreaker();
            var actionCallCount = 0;

            circuitBreaker.Execute(() => { actionCallCount++; });

            Assert.AreEqual(1, actionCallCount);
        }

        /// <summary>
        ///     The execute with result should return result test.
        /// </summary>
        [Test]
        public void ExecuteWithResultShouldReturnResult()
        {
            var circuitBreaker = new CircuitBreaker();
            var expectedResult = new object();

            var result = circuitBreaker.Execute(() => expectedResult);

            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        ///     The service level tests.
        /// </summary>
        /// <param name="callPattern">
        ///     The call pattern.
        /// </param>
        /// <returns>
        ///     The <see cref="double"/>.
        /// </returns>
        /// <exception cref="Exception">
        ///     Operation failed exception.
        /// </exception>
        [Test]
        [TestCase("", ExpectedResult = 100d)]
        [TestCase("bad", ExpectedResult = 80d)]
        [TestCase("bad good", ExpectedResult = 100d)]
        [TestCase("bad bad", ExpectedResult = 60d)]
        [TestCase("bad bad good", ExpectedResult = 80d)]
        [TestCase("bad bad good good", ExpectedResult = 100d)]
        [TestCase("bad good bad good", ExpectedResult = 100d)]
        public double ServiceLevel(string callPattern)
        {
            var circuitBreaker = new CircuitBreaker();

            foreach (var call in callPattern.Split(
                new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (call)
                {
                    case "good":
                        circuitBreaker.Execute(() => { });
                        break;

                    case "bad":
                        try
                        {
                            circuitBreaker.Execute(() => { throw new Exception(); });
                        }
                        catch (OperationFailedException) { }
                        break;

                    default:
                        Assert.Fail("Unknown call sequence");
                        break;
                }
            }

            return circuitBreaker.ServiceLevel;
        }

        /// <summary>
        ///     Tests that the call count of the circuit breaker is incremented when a call through
        ///     the circuit breaker is made.
        /// </summary>
        [Test]
        public void TestCircuitBreakerIncrementsTotalCallCount()
        {
            // Setup a circuit breaker.
            var circuitBreaker = new CircuitBreaker();

            // Setup a mock object to call in the circuit breaker.
            var mockOperation = new Mock<ITestAction>();
            mockOperation.Setup(action => action.ToString());

            // The initial call count of the circuit breaker.
            var initCallCount = circuitBreaker.TotalCallCount;

            // Execute the call through the circuit breaker.
            circuitBreaker.Execute(() => mockOperation.Object.ToString());

            // The final call count through the circuit breaker.
            var finalCallCount = circuitBreaker.TotalCallCount;

            // Make sure that the call count has incremented by 1.
            Assert.AreEqual(initCallCount + 1, finalCallCount);
        }

        /// <summary>
        ///     Tests that the latency of the call through the circuit breaker is recorded.
        /// </summary>
        [Test]
        public void TestCircuitBreakerRecordsCallLatency()
        {
            // Setup a circuit breaker.
            var circuitBreaker = new CircuitBreaker();

            // Setup a mock object to call in the circuit breaker.  We want to make sure it takes
            // 1 second so that we can see the latency.
            var mockOperation = new Mock<ITestAction>();
            mockOperation.Setup(action => action.ToString()).Callback(() => Thread.Sleep(1000));

            // Execute the call through the circuit breaker.
            circuitBreaker.Execute(() => mockOperation.Object.ToString());
            
            // Make sure that the call took about 1 second.7
            Assert.AreEqual(TimeSpan.FromSeconds(1).Seconds, circuitBreaker.Latency.Seconds);
        }
    }
}
