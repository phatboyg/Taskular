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
namespace Taskular.Intervals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public class FixedRepeatInterval :
        IEnumerator<RepeatInterval>,
        RepeatInterval
    {
        readonly TimeSpan _delay;
        long _repeatNumber;

        public FixedRepeatInterval(TimeSpan delay)
        {
            _delay = delay;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            _repeatNumber++;
            return true;
        }

        public void Reset()
        {
            _repeatNumber = 0;
        }

        public RepeatInterval Current
        {
            get { return this; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public TimeSpan Delay
        {
            get { return _delay; }
        }

        public long RepeatNumber
        {
            get { return _repeatNumber; }
        }
    }
}