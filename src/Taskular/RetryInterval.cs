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
namespace Taskular
{
    using System;


    /// <summary>
    /// The retry settings for the current retry interval
    /// </summary>
    public interface RetryInterval
    {
        /// <summary>
        /// Returns the attempt number of the current retry
        /// </summary>
        int Attempt { get; }

        /// <summary>
        /// The delay to wait before attempting the next retry
        /// </summary>
        TimeSpan Delay { get; }

        /// <summary>
        /// Determines if the exception can be retried per the retry policy
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool CanRetry(Exception exception);
    }
}