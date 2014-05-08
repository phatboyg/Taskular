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
    using System.Linq;
    using Policies;


    public static class Retry
    {
        static readonly IRetryExceptionFilter _all = new RetryAllRetryExceptionFilter();

        /// <summary>
        /// Create an immediate retry policy with the specified number of retries, with no
        /// delay between attempts.
        /// </summary>
        /// <param name="retryLimit">The number of retries to attempt</param>
        /// <returns></returns>
        public static IRetryPolicy Immediate(int retryLimit)
        {
            return new ImmediateRetryPolicy(All(), retryLimit);
        }

        /// <summary>
        /// Create an immediate retry policy with the specified number of retries, with no
        /// delay between attempts.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="retryLimit">The number of retries to attempt</param>
        /// <returns></returns>
        public static IRetryPolicy Immediate(this IRetryExceptionFilter filter, int retryLimit)
        {
            return new ImmediateRetryPolicy(filter, retryLimit);
        }

        /// <summary>
        /// Create an interval retry policy with the specified intervals. The retry count equals
        /// the number of intervals provided
        /// </summary>
        /// <param name="intervals">The intervals before each subsequent retry attempt</param>
        /// <returns></returns>
        public static IRetryPolicy Intervals(params TimeSpan[] intervals)
        {
            return new IntervalRetryPolicy(All(), intervals);
        }

        /// <summary>
        /// Create an interval retry policy with the specified intervals. The retry count equals
        /// the number of intervals provided
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="intervals">The intervals before each subsequent retry attempt</param>
        /// <returns></returns>
        public static IRetryPolicy Intervals(this IRetryExceptionFilter filter, params TimeSpan[] intervals)
        {
            return new IntervalRetryPolicy(filter, intervals);
        }

        /// <summary>
        /// Create an interval retry policy with the specified intervals. The retry count equals
        /// the number of intervals provided
        /// </summary>
        /// <param name="intervals">The intervals before each subsequent retry attempt</param>
        /// <returns></returns>
        public static IRetryPolicy Intervals(params int[] intervals)
        {
            return new IntervalRetryPolicy(All(), intervals);
        }

        /// <summary>
        /// Create an interval retry policy with the specified intervals. The retry count equals
        /// the number of intervals provided
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="intervals">The intervals before each subsequent retry attempt</param>
        /// <returns></returns>
        public static IRetryPolicy Intervals(this IRetryExceptionFilter filter, params int[] intervals)
        {
            return new IntervalRetryPolicy(filter, intervals);
        }

        /// <summary>
        /// Create an interval retry policy with the specified number of retries at a fixed interval
        /// </summary>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <param name="interval">The interval between each retry attempt</param>
        /// <returns></returns>
        public static IRetryPolicy Interval(int retryCount, TimeSpan interval)
        {
            return new IntervalRetryPolicy(All(), Enumerable.Repeat(interval, retryCount).ToArray());
        }

        /// <summary>
        /// Create an interval retry policy with the specified number of retries at a fixed interval
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <param name="interval">The interval between each retry attempt</param>
        /// <returns></returns>
        public static IRetryPolicy Interval(this IRetryExceptionFilter filter, int retryCount, TimeSpan interval)
        {
            return new IntervalRetryPolicy(filter, Enumerable.Repeat(interval, retryCount).ToArray());
        }

        /// <summary>
        /// Create an exponential retry policy with the specified number of retries at exponential
        /// intervals
        /// </summary>
        /// <param name="retryLimit"></param>
        /// <param name="minInterval"></param>
        /// <param name="maxInterval"></param>
        /// <param name="intervalDelta"></param>
        /// <returns></returns>
        public static IRetryPolicy Exponential(int retryLimit, TimeSpan minInterval, TimeSpan maxInterval, TimeSpan intervalDelta)
        {
            return new ExponentialIntervalRetryPolicy(All(), retryLimit, minInterval, maxInterval, intervalDelta);
        }

        /// <summary>
        /// Create an exponential retry policy with the specified number of retries at exponential
        /// intervals
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="retryLimit"></param>
        /// <param name="minInterval"></param>
        /// <param name="maxInterval"></param>
        /// <param name="intervalDelta"></param>
        /// <returns></returns>
        public static IRetryPolicy Exponential(this IRetryExceptionFilter filter, int retryLimit, TimeSpan minInterval, TimeSpan maxInterval,
            TimeSpan intervalDelta)
        {
            return new ExponentialIntervalRetryPolicy(filter, retryLimit, minInterval, maxInterval, intervalDelta);
        }

        /// <summary>
        /// Create an incremental retry policy with the specified number of retry attempts with an incrementing
        /// interval between retries
        /// </summary>
        /// <param name="retryLimit">The number of retry attempts</param>
        /// <param name="initialInterval">The initial retry interval</param>
        /// <param name="intervalIncrement">The interval to add to the retry interval with each subsequent retry</param>
        /// <returns></returns>
        public static IRetryPolicy Incremental(int retryLimit, TimeSpan initialInterval, TimeSpan intervalIncrement)
        {
            return new IncrementalRetryPolicy(All(), retryLimit, initialInterval, intervalIncrement);
        }

        /// <summary>
        /// Create an incremental retry policy with the specified number of retry attempts with an incrementing
        /// interval between retries
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="retryLimit">The number of retry attempts</param>
        /// <param name="initialInterval">The initial retry interval</param>
        /// <param name="intervalIncrement">The interval to add to the retry interval with each subsequent retry</param>
        /// <returns></returns>
        public static IRetryPolicy Incremental(this IRetryExceptionFilter filter, int retryLimit, TimeSpan initialInterval,
            TimeSpan intervalIncrement)
        {
            return new IncrementalRetryPolicy(filter, retryLimit, initialInterval, intervalIncrement);
        }

        /// <summary>
        /// Retry all exceptions except for the exception types specified
        /// </summary>
        /// <param name="exceptionTypes"></param>
        /// <returns></returns>
        public static IRetryExceptionFilter Except(params Type[] exceptionTypes)
        {
            return new RetryExceptRetryExceptionFilter(exceptionTypes);
        }

        /// <summary>
        /// Retry all exceptions except for the exception types specified
        /// </summary>
        /// <returns></returns>
        public static IRetryExceptionFilter Except<T1>()
        {
            return new RetryExceptRetryExceptionFilter(typeof(T1));
        }

        /// <summary>
        /// Retry all exceptions except for the exception types specified
        /// </summary>
        /// <returns></returns>
        public static IRetryExceptionFilter Except<T1, T2>()
        {
            return new RetryExceptRetryExceptionFilter(typeof(T1), typeof(T2));
        }

        /// <summary>
        /// Retry all exceptions except for the exception types specified
        /// </summary>
        /// <returns></returns>
        public static IRetryExceptionFilter Except<T1, T2, T3>()
        {
            return new RetryExceptRetryExceptionFilter(typeof(T1), typeof(T2), typeof(T3));
        }

        /// <summary>
        /// Retry only the exception types specified
        /// </summary>
        /// <param name="exceptionTypes"></param>
        /// <returns></returns>
        public static IRetryExceptionFilter Selected(params Type[] exceptionTypes)
        {
            return new RetrySelectedRetryExceptionFilter(exceptionTypes);
        }

        /// <summary>
        /// Retry only the exception types specified
        /// </summary>
        /// <returns></returns>
        public static IRetryExceptionFilter Selected<T1>()
        {
            return new RetrySelectedRetryExceptionFilter(typeof(T1));
        }

        /// <summary>
        /// Retry only the exception types specified
        /// </summary>
        /// <returns></returns>
        public static IRetryExceptionFilter Selected<T1, T2>()
        {
            return new RetrySelectedRetryExceptionFilter(typeof(T1), typeof(T2));
        }

        /// <summary>
        /// Retry only the exception types specified
        /// </summary>
        /// <returns></returns>
        public static IRetryExceptionFilter Selected<T1, T2, T3>()
        {
            return new RetrySelectedRetryExceptionFilter(typeof(T1), typeof(T2), typeof(T3));
        }

        /// <summary>
        /// Retry all exceptions
        /// </summary>
        /// <returns></returns>
        public static IRetryExceptionFilter All()
        {
            return _all;
        }

        /// <summary>
        /// Filter an exception type
        /// </summary>
        /// <typeparam name="T">The exception type</typeparam>
        /// <param name="filter">The filter expression</param>
        /// <returns>True if the exception should be retried, otherwise false</returns>
        public static IRetryExceptionFilter Filter<T>(Func<T, bool> filter)
            where T : Exception
        {
            return new FilterRetryExceptionFilter<T>(filter);
        }
    }
}