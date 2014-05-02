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
    using System.Threading.Tasks;


    public static class DelayExtensions
    {
        public static Composer<T> Delay<T>(this Composer<T> composer, int millisecondsDelay)
        {
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay",
                    "The delay must be non-negative or -1, and it must be less than or equal to Int32.MaxValue.");
            }

            composer.ExecuteTask((payload, cancellationToken) =>
                Task.Delay(millisecondsDelay, cancellationToken).ContinueWith(x => payload, cancellationToken,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default));

            return composer;
        }

        public static Composer<T> Delay<T>(this Composer<T> composer, TimeSpan delay)
        {
            if (delay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("delay",
                    "The delay must be non-negative or -1, and it must be less than or equal to Int32.MaxValue.");
            }

            composer.ExecuteTask((payload, cancellationToken) =>
                Task.Delay(delay, cancellationToken).ContinueWith(x => payload, cancellationToken,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default));

            return composer;
        }

        public static Composer Delay(this Composer composer, int millisecondsDelay)
        {
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay",
                    "The delay must be non-negative or -1, and it must be less than or equal to Int32.MaxValue.");
            }

            composer.ExecuteTask(cancellationToken => Task.Delay(millisecondsDelay, cancellationToken));

            return composer;
        }

        public static Composer Delay(this Composer composer, TimeSpan delay)
        {
            if (delay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("delay",
                    "The delay must be non-negative or -1, and it must be less than or equal to Int32.MaxValue.");
            }

            composer.ExecuteTask(cancellationToken => Task.Delay(delay, cancellationToken));

            return composer;
        }
    }
}