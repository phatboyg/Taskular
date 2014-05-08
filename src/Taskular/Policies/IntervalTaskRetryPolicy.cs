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
    using System.Collections.Generic;
    using System.Linq;


    public class IntervalTaskRetryPolicy :
        ITaskRetryPolicy
    {
        readonly IEnumerable<TimeSpan> _intervals;

        public IntervalTaskRetryPolicy(params TimeSpan[] intervals)
        {
            if (intervals == null)
                throw new ArgumentNullException("intervals");
            if (intervals.Length == 0)
                throw new ArgumentOutOfRangeException("intervals", "At least one interval must be specified");

            _intervals = intervals;
        }

        public IntervalTaskRetryPolicy(params int[] intervals)
        {
            if (intervals == null)
                throw new ArgumentNullException("intervals");
            if (intervals.Length == 0)
                throw new ArgumentOutOfRangeException("intervals", "At least one interval must be specified");

            _intervals = intervals.Select(x => TimeSpan.FromMilliseconds(x)).ToArray();
        }

        public IEnumerator<RetryAttempt> GetRetryInterval()
        {
            return _intervals.Select((x, index) => new TaskRetryAttempt(this, index, x)).GetEnumerator();
        }

        public bool CanRetry(Exception exception)
        {
            return true;
        }
    }
}