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
namespace Taskular
{
    using System;
    using System.Collections.Generic;


    public static class RetryExtensions
    {
        /// <summary>
        /// Retry a task composition using the specified retry policy
        /// </summary>
        /// <param name="composer">The task composer</param>
        /// <param name="retryPolicy">The retry policy</param>
        /// <param name="callback">The task composition callback</param>
        /// <returns>The original task composer</returns>
        public static Composer Retry(this Composer composer, ITaskRetryPolicy retryPolicy, Action<Composer> callback)
        {
            IEnumerator<RetryInterval> retryInterval = null;
            composer.ComposeTask(taskComposer =>
            {
                retryInterval = retryPolicy.GetRetryInterval();

                Attempt(taskComposer, retryInterval, callback);
            });

            composer.Finally(() =>
            {
                if (retryInterval != null)
                    retryInterval.Dispose();
            });

            return composer;
        }

        static void Attempt(Composer composer, IEnumerator<RetryInterval> retryInterval, Action<Composer> callback)
        {
            composer.ComposeTask(callback);

            composer.Compensate(compensation =>
            {
                if (!retryInterval.MoveNext())
                    return compensation.Throw();

                if (!retryInterval.Current.CanRetry(compensation.Exception))
                    return compensation.Throw();

                return compensation.ComposeTask(x =>
                {
                    x.Delay(retryInterval.Current.Delay);

                    Attempt(x, retryInterval, callback);
                });
            });
        }

        /// <summary>
        /// Retry a task composition using the specified retry policy
        /// </summary>
        /// <typeparam name="T">The composer payload type</typeparam>
        /// <param name="composer">The task composer</param>
        /// <param name="retryPolicy">The retry policy</param>
        /// <param name="callback">The task composition callback</param>
        /// <returns>The original task composer</returns>
        public static Composer<T> Retry<T>(this Composer<T> composer, ITaskRetryPolicy retryPolicy,
            Action<Composer<T>> callback)
        {
            IEnumerator<RetryInterval> retryInterval = null;
            composer.ComposeTask(taskComposer =>
            {
                retryInterval = retryPolicy.GetRetryInterval();

                Attempt(taskComposer, retryInterval, callback);
            });

            composer.Finally(() =>
            {
                if (retryInterval != null)
                    retryInterval.Dispose();
            });

            return composer;
        }

        static void Attempt<T>(Composer<T> composer, IEnumerator<RetryInterval> retryInterval,
            Action<Composer<T>> callback)
        {
            composer.ComposeTask(callback);

            composer.Compensate(compensation =>
            {
                if (!retryInterval.MoveNext())
                    return compensation.Throw();

                if (!retryInterval.Current.CanRetry(compensation.Exception))
                    return compensation.Throw();

                return compensation.ComposeTask(x =>
                {
                    x.Delay(retryInterval.Current.Delay);

                    Attempt(x, retryInterval, callback);
                });
            });
        }
    }
}