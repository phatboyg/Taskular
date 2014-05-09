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


    /// <summary>
    ///     A composer allows a series of tasks to be chained together. Each task is executed in sequence.
    ///     Tasks can form a tree, meaning that one task in the chain may be composed of multiple tasks that roll up into a
    ///     single task on the parent chain. The Execute/Compensate/Finally methods allow the concepts of try/catch/finally to
    ///     be mapped to tasks as well.
    /// </summary>
    public interface Composer
    {
        CancellationToken CancellationToken { get; }

        /// <summary>
        ///     The last Task in the chain. This task changes with every task that is added to the chain
        /// </summary>
        Task Task { get; }

        /// <summary>
        ///     Execute an taskFactory with the specified payload.
        /// </summary>
        /// <param name="action">The taskFactory to execute</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        Composer Execute(Action action, ExecuteOptions options = ExecuteOptions.None);

        /// <summary>
        ///     Execute an taskFactory with the specified payload.
        /// </summary>
        /// <param name="taskFactory">The taskFactory to execute</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        Composer ExecuteAsync(Func<CancellationToken, Task> taskFactory, ExecuteOptions options = ExecuteOptions.None);

        /// <summary>
        ///     If a previous task faulted, run a compensating taskFactory to handle the fault.
        /// </summary>
        /// <param name="compensation">The compensation context</param>
        /// <returns>A compensation result indicating the disposition of the fault</returns>
        Composer Compensate(Func<Compensation, CompensationResult> compensation);

        /// <summary>
        ///     Adds a action that is always run, regardless of a successful or exceptional condition
        /// </summary>
        /// <param name="continuation">The continuation method</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        Composer Finally(Action<TaskStatus> continuation, ExecuteOptions options = ExecuteOptions.None);

        /// <summary>
        ///     Add a task that faults the composition
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="exception"></param>
        /// <returns></returns>
        Composer Fault<TException>(TException exception)
            where TException : Exception;
    }
}