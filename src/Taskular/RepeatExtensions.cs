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
    using System.Threading;


    public static class RepeatExtensions
    {
        /// <summary>
        /// Retry a task composition using the specified retry policy
        /// </summary>
        /// <param name="composer">The task composer</param>
        /// <param name="interval">The interval between each repetition</param>
        /// <param name="callback">The task composition callback</param>
        /// <param name="repeatCancellationToken">A cancellation token for the repeat-portion of the task composer</param>
        /// <returns>The original task composer</returns>
        public static Composer Repeat(this Composer composer, TimeSpan interval, Action<Composer> callback,
            CancellationToken repeatCancellationToken = default(CancellationToken))
        {
            if (interval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("interval",
                    "The interval must be non-negative, and it must be less than or equal to Int32.MaxValue.");
            }

            composer.ComposeTask(taskComposer => Attempt(taskComposer, interval, callback, repeatCancellationToken));

            return composer;
        }

        /// <summary>
        /// Retry a task composition using the specified retry policy
        /// </summary>
        /// <param name="composer">The task composer</param>
        /// <param name="initialDelay">The initial delay before the first execution</param>
        /// <param name="interval">The interval between each repetition</param>
        /// <param name="callback">The task composition callback</param>
        /// <param name="repeatCancellationToken">A cancellation token for the repeat-portion of the task composer</param>
        /// <returns>The original task composer</returns>
        public static Composer Repeat(this Composer composer, TimeSpan initialDelay, TimeSpan interval, Action<Composer> callback,
            CancellationToken repeatCancellationToken = default(CancellationToken))
        {
            if (interval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("interval",
                    "The interval must be non-negative, and it must be less than or equal to Int32.MaxValue.");
            }

            if (initialDelay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("initialDelay",
                    "The initialDelay must be non-negative, and it must be less than or equal to Int32.MaxValue.");
            }

            composer.Delay(initialDelay);

            composer.ComposeTask(taskComposer => Attempt(taskComposer, interval, callback, repeatCancellationToken));

            return composer;
        }

        /// <summary>
        /// Retry a task composition using the specified retry policy
        /// </summary>
        /// <param name="composer">The task composer</param>
        /// <param name="millisecondsInterval"></param>
        /// <param name="callback">The task composition callback</param>
        /// <param name="repeatCancellationToken"></param>
        /// <returns>The original task composer</returns>
        public static Composer Repeat(this Composer composer, int millisecondsInterval, Action<Composer> callback,
            CancellationToken repeatCancellationToken)
        {
            if (millisecondsInterval < 0)
            {
                throw new ArgumentOutOfRangeException("millisecondsInterval",
                    "The interval must be non-negative, and it must be less than or equal to Int32.MaxValue.");
            }

            composer.ComposeTask(
                taskComposer => Attempt(taskComposer, TimeSpan.FromMilliseconds(millisecondsInterval), callback, repeatCancellationToken));

            return composer;
        }

        /// <summary>
        /// Retry a task composition using the specified retry policy
        /// </summary>
        /// <param name="composer">The task composer</param>
        /// <param name="millisecondsInitialDelay">The initial delay before the first execution in milliseconds</param>
        /// <param name="millisecondsInterval"></param>
        /// <param name="callback">The task composition callback</param>
        /// <param name="repeatCancellationToken"></param>
        /// <returns>The original task composer</returns>
        public static Composer Repeat(this Composer composer, int millisecondsInitialDelay, int millisecondsInterval,
            Action<Composer> callback,
            CancellationToken repeatCancellationToken)
        {
            if (millisecondsInterval < 0)
            {
                throw new ArgumentOutOfRangeException("millisecondsInterval",
                    "The interval must be non-negative, and it must be less than or equal to Int32.MaxValue.");
            }

            if (millisecondsInitialDelay < 0)
            {
                throw new ArgumentOutOfRangeException("millisecondsInitialDelay",
                    "The initialDelay must be non-negative, and it must be less than or equal to Int32.MaxValue.");
            }

            composer.Delay(millisecondsInitialDelay);

            composer.ComposeTask(
                taskComposer => Attempt(taskComposer, TimeSpan.FromMilliseconds(millisecondsInterval), callback, repeatCancellationToken));

            return composer;
        }

        static void Attempt(Composer composer, TimeSpan interval, Action<Composer> callback, CancellationToken repeatCancellationToken)
        {
            composer.ComposeTask(callback);

            composer.ExecuteAsync(token =>
            {
                if (repeatCancellationToken.IsCancellationRequested)
                    return TaskUtil.Completed();

                return ComposerFactory.Compose(x =>
                {
                    x.Delay(interval);

                    Attempt(x, interval, callback, repeatCancellationToken);
                }, token);
            }, ExecuteOptions.RunSynchronously);
        }
    }
}