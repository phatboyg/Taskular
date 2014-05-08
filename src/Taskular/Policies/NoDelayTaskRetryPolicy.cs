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


    public class NoDelayTaskRetryPolicy :
        ITaskRetryPolicy
    {
        readonly int _retryCount;

        public NoDelayTaskRetryPolicy(int retryCount)
        {
            _retryCount = retryCount;
        }

        public IEnumerator<RetryAttempt> GetRetryInterval()
        {
            for (int i = 0; i < _retryCount; i++)
                yield return new TaskRetryAttempt(this, i, TimeSpan.Zero);
        }

        public bool CanRetry(Exception exception)
        {
            return true;
        }
    }
}