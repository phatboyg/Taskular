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
    using System.Collections.Generic;
    using NUnit.Framework;


    [TestFixture]
    public class ExponentialRetry_Specs
    {
        [Test]
        public void Should_have_a_working_algorithm()
        {
            foreach (TimeSpan interval in GetIntervals())
                Console.WriteLine(interval);
        }

        int _retryLimit = 10;
        int _minInterval = 100;
        int _maxInterval = 60000;
        int _lowInterval = (int)(500 * 0.8);
        int _highInterval = (int)(500 * 1.2);

        IEnumerable<TimeSpan> GetIntervals()
        {
            var random = new Random();

            for (int i = 0; i < _retryLimit; i++)
            {
                var delta = (int)Math.Min(_minInterval + Math.Pow(2, i) * random.Next(_lowInterval, _highInterval), _maxInterval);

                yield return TimeSpan.FromMilliseconds(delta);
            }
        }
    }
}