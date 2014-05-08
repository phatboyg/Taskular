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
    /// <typeparam name="T">The payload type for the composer</typeparam>
    public interface Composer<T>
    {
        /// <summary>
        ///     The cancellation token that is shared by all tasks in the task chain
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        ///     The last Task in the chain. This task changes with every task that is added to the chain
        /// </summary>
        Task<T> Task { get; }

        /// <summary>
        ///     The payload of the composer
        /// </summary>
        T Payload { get; }

        /// <summary>
        ///     Execute an taskFactory with the specified payload.
        /// </summary>
        /// <param name="action">The taskFactory to execute</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        Composer<T> Execute(Action<T> action, ExecuteOptions options = ExecuteOptions.None);

        /// <summary>
        ///     Execute an taskFactory with the specified payload.
        /// </summary>
        /// <param name="taskFactory">The taskFactory to execute</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        Composer<T> ExecuteTask(Func<T, CancellationToken, Task<T>> taskFactory,
            ExecuteOptions options = ExecuteOptions.None);

        /// <summary>
        ///     If a previous task faulted, run a compensating taskFactory to handle the fault.
        /// </summary>
        /// <param name="compensation">The compensation context</param>
        /// <returns>A compensation result indicating the disposition of the fault</returns>
        Composer<T> Compensate(Func<Compensation<T>, CompensationResult<T>> compensation);

        /// <summary>
        ///     Adds a action that is always run, regardless of a successful or exceptional condition
        /// </summary>
        /// <param name="continuation">The continuation method</param>
        /// <param name="options">The task execution options</param>
        /// <returns></returns>
        Composer<T> Finally(Action<T, TaskStatus> continuation, ExecuteOptions options);

        /// <summary>
        ///     Add a task that faults the composition
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="exception"></param>
        /// <returns></returns>
        Composer<T> Fault<TException>(TException exception)
            where TException : Exception;
    }
}