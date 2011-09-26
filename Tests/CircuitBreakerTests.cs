﻿using System;
using NUnit.Framework;
using ReliabilityPatterns;

namespace Tests
{
    [TestFixture]
    public class CircuitBreakerTests
    {
        [Test]
        [TestCase("", Result = 100d)]
        [TestCase("bad", Result = 80d)]
        [TestCase("bad good", Result = 100d)]
        [TestCase("bad bad", Result = 60d)]
        [TestCase("bad bad good", Result = 80d)]
        [TestCase("bad bad good good", Result = 100d)]
        [TestCase("bad good bad good", Result = 100d)]
        public double ServiceLevel(string callPattern)
        {
            var circuitBreaker = new CircuitBreaker();

            foreach (var call in callPattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (call)
                {
                    case "good":
                        circuitBreaker.Execute(() => { });
                        break;
                    case "bad":
                        try { circuitBreaker.Execute(() => { throw new Exception(); }); }
                        catch (OperationFailedException) { }
                        break;
                    default:
                        Assert.Fail("Unknown call sequence");
                        break;
                }
            }

            return circuitBreaker.ServiceLevel;
        }
    }
}
