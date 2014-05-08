// Copyright 2007-2014 Chris Patterson
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
    public class Repeating_a_task
    {
        [Test]
        public void Should_repeat_until_canceled()
        {
            var tracker = new Tracker(10);

            Stopwatch timer = Stopwatch.StartNew();

            Task task = ComposerFactory.Compose(x => x.Repeat(10, composer =>
            {
                Console.WriteLine("Timespan: {0}", timer.Elapsed);
                composer.Execute(tracker.RepeatedMethod);
            }, tracker.Token));

            task.Wait();

            timer.Stop();

            Assert.AreEqual(10, tracker.CallCount);
        }


        class Tracker
        {
            readonly int _repeatCount;
            readonly CancellationTokenSource _source;

            int _callCount;

            public Tracker(int repeatCount)
            {
                _repeatCount = repeatCount;
                _source = new CancellationTokenSource();
            }

            public CancellationToken Token
            {
                get { return _source.Token; }
            }

            public int CallCount
            {
                get { return _callCount; }
            }

            public void RepeatedMethod()
            {
                int count = Interlocked.Increment(ref _callCount);
                if (count == _repeatCount)
                    _source.Cancel();
                if (count > _repeatCount)
                    throw new InvalidOperationException("Repeated too many times.");
            }
        }
    }
}