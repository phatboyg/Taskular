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


    public static class ComposerExtensions
    {
        public static Composer<T> Execute<T>(this Composer<T> composer, Action continuation,
            ExecuteOptions options = ExecuteOptions.None)
        {
            return composer.Execute(payload => continuation(), options);
        }

        /// <summary>
        ///     Adds a continuation that is always run, regardless of a successful or exceptional condition
        /// </summary>
        /// <param name="composer"></param>
        /// <param name="continuation"></param>
        /// <param name="options"></param>
        public static Composer Finally(this Composer composer, Action continuation,
            ExecuteOptions options = ExecuteOptions.None)
        {
            return composer.Finally(status => continuation(), options);
        }

        /// <summary>
        ///     Adds a continuation that is always run, regardless of a successful or exceptional condition
        /// </summary>
        /// <param name="composer"></param>
        /// <param name="continuation"></param>
        /// <param name="options"></param>
        public static Composer<T> Finally<T>(this Composer<T> composer, Action<T> continuation,
            ExecuteOptions options = ExecuteOptions.None)
        {
            return composer.Finally((payload, status) => continuation(payload), options);
        }

        /// <summary>
        ///     Adds a continuation that is always run, regardless of a successful or exceptional condition
        /// </summary>
        /// <param name="composer"></param>
        /// <param name="continuation"></param>
        /// <param name="options"></param>
        public static Composer<T> Finally<T>(this Composer<T> composer, Action continuation,
            ExecuteOptions options = ExecuteOptions.None)
        {
            return composer.Finally((payload, status) => continuation(), options);
        }

        /// <summary>
        ///     ComposeTask a completed Task that can be added to an existing composition
        /// </summary>
        /// <returns>A completed Task</returns>
        public static Task<T> ComposeCompleted<T>(this Composer<T> composer)
        {
            return composer.CancellationToken.IsCancellationRequested
                ? TaskUtil.Canceled<T>()
                : TaskUtil.Completed(composer.Payload);
        }

        /// <summary>
        ///     Creates a new Composer, which can be used to compose a task chain, which can be added to an
        ///     existing composer as a single task. Note that this executes on the task chain, and not immediately.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="composer">The existing composer</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Composer<T> ComposeTask<T>(this Composer<T> composer, Action<Composer<T>> callback)
        {
            composer.ExecuteTask((payload, cancellationToken) => ComposerFactory.Compose(payload, callback, composer.CancellationToken));

            return composer;
        }

        /// <summary>
        ///     Creates a new Composer, which can be used to compose a task chain, which can be added to an
        ///     existing composer as a single task. Note that this executes on the task chain, and not immediately.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <typeparam name="TPayload">The child payload type</typeparam>
        /// <param name="composer">The existing composer</param>
        /// <param name="payload">The child payload</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Composer<T> ComposeTask<T, TPayload>(this Composer<T> composer, Func<T, TPayload> payload,
            Action<Composer<TPayload>> callback)
        {
            composer.ExecuteTask(async (p, cancellationToken) =>
            {
                TPayload taskPayload = payload(p);

                await ComposerFactory.Compose(taskPayload, callback, composer.CancellationToken);

                return composer.Payload;
            });

            return composer;
        }


        /// <summary>
        ///     Creates a new Composer, which can be used to compose a task chain, which can be added to an
        ///     existing composer as a single task. Note that this executes on the task chain, and not immediately.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="composer">The existing composer</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Composer<T> ComposeTask<T>(this Composer<T> composer, Action<Composer> callback)
        {
            composer.ExecuteTask(async (payload, cancellationToken) =>
            {
                await ComposerFactory.Compose(callback, composer.CancellationToken);

                return composer.Payload;
            });

            return composer;
        }

        /// <summary>
        ///     Creates a new Composer, which can be used to compose a task chain, which can be added to an
        ///     existing composer as a single task. Note that this executes on the task chain, and not immediately.
        /// </summary>
        /// <param name="composer">The existing composer</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Composer ComposeTask(this Composer composer, Action<Composer> callback)
        {
            composer.ExecuteTask(cancellationToken => ComposerFactory.Compose(callback, composer.CancellationToken));

            return composer;
        }

        /// <summary>
        ///     Creates a new Composer, which can be used to compose a task chain, which can be added to an
        ///     existing composer as a single task. Note that this executes on the task chain, and not immediately.
        /// </summary>
        /// <param name="compensation"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static CompensationResult ComposeTask(this Compensation compensation, Action<Composer> callback)
        {
            return compensation.Task(ComposerFactory.Compose(callback, compensation.CancellationToken));
        }

        /// <summary>
        ///     Creates a new Composer, which can be used to compose a task chain, which can be added to an
        ///     existing composer as a single task. Note that this executes on the task chain, and not immediately.
        /// </summary>
        /// <param name="compensation"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static CompensationResult<T> ComposeTask<T>(this Compensation<T> compensation, Action<Composer<T>> callback)
        {
            return compensation.Task(ComposerFactory.Compose(compensation.Payload, callback, compensation.CancellationToken));
        }


        /// <summary>
        ///     Creates a new Composer, which can be used to compose a task chain, which can be added to an
        ///     existing composer as a single task. Note that this executes on the task chain, and not immediately.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="compensation">The compensation</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static CompensationResult<T> ComposeTask<T>(this Compensation<T> compensation, Action<Composer> callback)
        {
            return compensation.Task(ComposeCompensationTask(compensation.Payload, callback, compensation.CancellationToken));
        }

        static async Task<T> ComposeCompensationTask<T>(T payload, Action<Composer> callback, CancellationToken cancellationToken)
        {
            await ComposerFactory.Compose(callback, cancellationToken);

            return payload;
        }
    }
}