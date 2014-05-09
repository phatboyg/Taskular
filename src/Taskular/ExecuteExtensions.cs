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
    using System.Threading.Tasks;


    public static class ExecuteExtensions
    {
        /// <summary>
        ///     Execute an taskFactory with the specified payload.
        /// </summary>
        /// <param name="composer">The source composer</param>
        /// <param name="continuation">The action to execute</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        public static Composer<T> Execute<T>(this Composer<T> composer, Action continuation,
            ExecuteOptions options = ExecuteOptions.None)
        {
            return composer.Execute(payload => continuation(), options);
        }

        /// <summary>
        ///     Execute an taskFactory with the specified payload.
        /// </summary>
        /// <param name="composer">The source composer</param>
        /// <param name="taskFactory">The taskFactory to execute</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        public static Composer<T> ExecuteAsync<T>(this Composer<T> composer, Func<CancellationToken, Task<T>> taskFactory,
            ExecuteOptions options = ExecuteOptions.None)
        {
            return composer.ExecuteAsync((payload, token) => taskFactory(token), options);
        }

        /// <summary>
        ///     Execute an taskFactory with the specified payload.
        /// </summary>
        /// <param name="composer">The source composer</param>
        /// <param name="taskFactory">The taskFactory to execute</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        public static Composer<T> ExecuteAsync<T>(this Composer<T> composer, Func<CancellationToken, Task> taskFactory,
            ExecuteOptions options = ExecuteOptions.None)
        {
            return composer.ExecuteAsync(async (payload, token) =>
            {
                await taskFactory(token);

                return payload;
            }, options);
        }

        /// <summary>
        ///     Execute an taskFactory with the specified payload.
        /// </summary>
        /// <param name="composer">The source composer</param>
        /// <param name="taskFactory">The taskFactory to execute</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        public static Composer<T> ExecuteAsync<T>(this Composer<T> composer, Func<T, CancellationToken, Task> taskFactory,
            ExecuteOptions options = ExecuteOptions.None)
        {
            return composer.ExecuteAsync(async (payload, token) =>
            {
                await taskFactory(payload, token);

                return payload;
            }, options);
        }
    }
}