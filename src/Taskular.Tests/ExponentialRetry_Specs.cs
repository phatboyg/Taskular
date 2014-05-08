namespace Taskular.Tests
{
    using System;
    using System.Linq;
    using NUnit.Framework;


    [TestFixture]
    public class ExponentialRetry_Specs
    {
        [Test]
        public void Should_have_a_working_algorithm()
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(100);
            TimeSpan maxDelay = TimeSpan.FromSeconds(60);
            TimeSpan delayDelta = TimeSpan.FromMilliseconds(500);
            int retryLimit = 10;

            Random rand = new Random();
            var intervals = Enumerable.Repeat(minDelay.TotalMilliseconds, retryLimit)
                .Select((min, index) => min + (int)((Math.Pow(2, index) * rand.Next((int)(delayDelta.TotalMilliseconds * 0.8), (int)(delayDelta.TotalMilliseconds * 1.2)))))
                .Select(x => Math.Min((int)maxDelay.TotalMilliseconds, x))
                .Select(TimeSpan.FromMilliseconds)
                .ToArray();

            foreach (var interval in intervals)
            {
                Console.WriteLine(interval);
            }
            
        }
    }
}
