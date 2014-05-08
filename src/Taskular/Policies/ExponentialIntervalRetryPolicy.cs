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
namespace Taskular.Policies
{
    using System;
    using System.Linq;


    public class ExponentialIntervalRetryPolicy :
        IRetryPolicy
    {
        readonly TimeSpan[] _intervals;

        public ExponentialIntervalRetryPolicy(int retryLimit, TimeSpan minDelay, TimeSpan maxDelay, TimeSpan delayDelta)
        {
            var rand = new Random();

            _intervals = Enumerable.Repeat(minDelay.TotalMilliseconds, retryLimit)
                .Select((min, index) => min + (int)((Math.Pow(2, index)
                                                     * rand.Next((int)(delayDelta.TotalMilliseconds * 0.8),
                                                         (int)(delayDelta.TotalMilliseconds * 1.2)))))
                .Select(x => Math.Min((int)maxDelay.TotalMilliseconds, x))
                .Select(TimeSpan.FromMilliseconds)
                .ToArray();
        }

        public IRetryContext GetRetryContext()
        {
            return new IntervalRetryContext(this, _intervals);
        }

        public bool CanRetry(Exception exception)
        {
            return true;
        }
    }
}