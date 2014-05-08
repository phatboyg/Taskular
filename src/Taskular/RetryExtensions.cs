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
    using Policies;


    public static class RetryExtensions
    {
        /// <summary>
        /// Retry a task composition using the specified retry policy
        /// </summary>
        /// <param name="composer">The task composer</param>
        /// <param name="retryPolicy">The retry policy</param>
        /// <param name="callback">The task composition callback</param>
        /// <returns>The original task composer</returns>
        public static Composer Retry(this Composer composer, IRetryPolicy retryPolicy, Action<Composer> callback)
        {
            IRetryContext retryContext = null;
            composer.ComposeTask(taskComposer =>
            {
                retryContext = retryPolicy.GetRetryContext();

                Attempt(taskComposer, retryContext, callback);
            });

            composer.Finally(status =>
            {
                if (retryContext != null)
                    retryContext.Complete(status);
            });

            return composer;
        }

        static void Attempt(Composer composer, IRetryContext retryContext, Action<Composer> callback)
        {
            composer.ComposeTask(callback);

            composer.Compensate(compensation =>
            {
                TimeSpan delay;
                if (!retryContext.CanRetry(compensation.Exception, out delay))
                    return compensation.Throw();

                return compensation.ComposeTask(x =>
                {
                    x.Delay(delay);

                    Attempt(x, retryContext, callback);
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
        public static Composer<T> Retry<T>(this Composer<T> composer, IRetryPolicy retryPolicy, Action<Composer<T>> callback)
        {
            IRetryContext retryContext = null;
            composer.ComposeTask(taskComposer =>
            {
                retryContext = retryPolicy.GetRetryContext();

                Attempt(taskComposer, retryContext, callback);
            });

            composer.Finally((_, status) =>
            {
                if (retryContext != null)
                    retryContext.Complete(status);
            });

            return composer;
        }

        static void Attempt<T>(Composer<T> composer, IRetryContext retryContext, Action<Composer<T>> callback)
        {
            composer.ComposeTask(callback);

            composer.Compensate(compensation =>
            {
                TimeSpan delay;
                if (!retryContext.CanRetry(compensation.Exception, out delay))
                    return compensation.Throw();

                return compensation.ComposeTask(x =>
                {
                    x.Delay(delay);

                    Attempt(x, retryContext, callback);
                });
            });
        }
    }
}