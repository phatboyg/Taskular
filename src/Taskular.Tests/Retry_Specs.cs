﻿// Copyright 2007-2014 Chris Patterson
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.
namespace Taskular.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public class When_using_the_retry_extension
    {
        [Test]
        public void Should_call_the_method_for_each_timespan()
        {
            var tracker = new Tracker(3);

            IRetryPolicy retryPolicy = Retry.Intervals(10, 50, 500, 1000);

            Task task = ComposerFactory.Compose(composer => composer.Retry(retryPolicy, x => x.Execute(tracker.FaultingMethod)));

            Stopwatch timer = Stopwatch.StartNew();

            task.Wait();

            timer.Stop();

            Console.WriteLine("Timespan: {0}", timer.Elapsed);

            Assert.AreEqual(4, tracker.CallCount);
        }

        [Test]
        public void Should_call_the_method_only_once()
        {
            var tracker = new Tracker(3);

            IRetryPolicy retryPolicy = Retry.Filter<InvalidOperationException>(x => false).Immediate(5);

            Task task = ComposerFactory.Compose(composer => composer.Retry(retryPolicy, x => x.Execute(tracker.FaultingMethod)));

            Assert.Throws<InvalidOperationException>(async () => await task);

            Assert.AreEqual(1, tracker.CallCount);
        }

        [Test]
        public void Should_call_the_method_three_times()
        {
            var tracker = new Tracker(3);

            IRetryPolicy retryPolicy = Retry.Immediate(5);

            Task task = ComposerFactory.Compose(composer => composer.Retry(retryPolicy, x => x.Execute(tracker.FaultingMethod)));

            task.Wait();

            Assert.AreEqual(4, tracker.CallCount);
        }

        [Test]
        public async void Should_include_the_handled_exception()
        {
            var tracker = new Tracker(3);

            IRetryPolicy retryPolicy = Retry.Selected<InvalidOperationException>().Immediate(5);

            Task task = ComposerFactory.Compose(composer => composer.Retry(retryPolicy, x => x.Execute(tracker.FaultingMethod)));

            await task;

            Assert.AreEqual(4, tracker.CallCount);
        }

        [Test]
        public void Should_not_include_the_excluded()
        {
            var tracker = new Tracker(3);

            IRetryPolicy retryPolicy = Retry.Except<InvalidOperationException>().Immediate(5);

            Task task = ComposerFactory.Compose(composer => composer.Retry(retryPolicy, x => x.Execute(tracker.FaultingMethod)));

            Assert.Throws<InvalidOperationException>(async () => await task);

            Assert.AreEqual(1, tracker.CallCount);
        }


        class Tracker
        {
            readonly int _failureCount;

            int _callCount;

            public Tracker(int failureCount)
            {
                _failureCount = failureCount;
            }

            public int CallCount
            {
                get { return _callCount; }
            }

            public void FaultingMethod()
            {
                int count = Interlocked.Increment(ref _callCount);

                if (count <= _failureCount)
                    throw new InvalidOperationException("This was expected");
            }
        }
    }
}