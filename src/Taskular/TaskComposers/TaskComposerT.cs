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
namespace Taskular.TaskComposers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Compensations;


    public class TaskComposer<T> :
        Composer<T>
    {
        readonly CancellationToken _cancellationToken;
        readonly T _payload;
        Task<T> _task;

        public TaskComposer(T payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            _payload = payload;
            _cancellationToken = cancellationToken;

            _task = cancellationToken.IsCancellationRequested
                ? TaskUtil.Canceled<T>()
                : TaskUtil.Completed(payload);
        }

        CancellationToken Composer<T>.CancellationToken
        {
            get { return _cancellationToken; }
        }

        Task<T> Composer<T>.Task
        {
            get { return _task; }
        }

        T Composer<T>.Payload
        {
            get { return _payload; }
        }

        Composer<T> Composer<T>.Execute(Action<T> action, ExecuteOptions options)
        {
            _task = Execute(_task, payload => TaskUtil.RunSynchronously(payload, action, _cancellationToken), options);
            return this;
        }

        Composer<T> Composer<T>.ExecuteTask(Func<T, CancellationToken, Task<T>> taskFactory, ExecuteOptions options)
        {
            _task = Execute(_task, payload => taskFactory(payload, _cancellationToken), options);
            return this;
        }

        Composer<T> Composer<T>.Compensate(Func<Compensation<T>, CompensationResult<T>> compensation)
        {
            if (_task.Status == TaskStatus.RanToCompletion)
                return this;

            _task = Compensate(_task, x => compensation(new TaskCompensation<T>(x, _payload, _cancellationToken)).Task);
            return this;
        }

        Composer<T> Composer<T>.Finally(Action<T, TaskStatus> continuation, ExecuteOptions options)
        {
            if (_task.IsCompleted && options.HasFlag(ExecuteOptions.RunSynchronously))
            {
                continuation(_payload, _task.Status);
                return this;
            }

            _task = FinallyAsync(_task, continuation, options);
            return this;
        }

        Composer<T> Composer<T>.Fault<TException>(TException exception)
        {
            _task = Execute(_task, payload => TaskUtil.Faulted<T>(exception), ExecuteOptions.None);
            return this;
        }

        Task<T> Execute(Task<T> task, Func<T, Task<T>> continuationTask, ExecuteOptions options)
        {
            if (task.IsCompleted && options.HasFlag(ExecuteOptions.RunSynchronously))
            {
                if (task.IsFaulted)
                    return TaskUtil.Faulted<T>(task.Exception.InnerExceptions);

                if (task.IsCanceled || _cancellationToken.IsCancellationRequested)
                    return TaskUtil.Canceled<T>();

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    try
                    {
                        return continuationTask(task.Result);
                    }
                    catch (Exception ex)
                    {
                        return TaskUtil.Faulted<T>(ex);
                    }
                }
            }

            return ExecuteAsync(task, continuationTask, options);
        }


        Task<T> ExecuteAsync(Task<T> task, Func<T, Task<T>> continuationTask, ExecuteOptions options)
        {
            var source = new TaskCompletionSource<Task<T>>();
            task.ContinueWith((Task<T> innerTask) =>
            {
                if (innerTask.IsFaulted)
                    source.TrySetException(innerTask.Exception.InnerExceptions);
                else if (innerTask.IsCanceled || _cancellationToken.IsCancellationRequested)
                    source.TrySetCanceled();
                else
                {
                    try
                    {
                        source.TrySetResult(continuationTask(innerTask.Result));
                    }
                    catch (Exception ex)
                    {
                        source.TrySetException(ex);
                    }
                }
            }, options.HasFlag(ExecuteOptions.RunSynchronously)
                ? TaskContinuationOptions.ExecuteSynchronously
                : TaskContinuationOptions.None);

            return source.Task.FastUnwrap();
        }

        Task<T> Compensate(Task<T> task, Func<Task<T>, Task<T>> compensationTask)
        {
            if (task.IsCompleted)
            {
                if (task.IsFaulted)
                {
                    try
                    {
                        Task<T> resultTask = compensationTask(task);
                        if (resultTask == null)
                            throw new InvalidOperationException("Sure could use a Task here buddy");

                        return resultTask;
                    }
                    catch (Exception ex)
                    {
                        return TaskUtil.Faulted<T>(ex);
                    }
                }

                if (task.IsCanceled)
                    return TaskUtil.Canceled<T>();

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    var tcs = new TaskCompletionSource<T>();
                    tcs.TrySetFromTask(task);
                    return tcs.Task;
                }
            }

            return CompensateAsync(task, compensationTask);
        }

        Task<T> CompensateAsync(Task<T> task, Func<Task<T>, Task<T>> compensationTask)
        {
            var source = new TaskCompletionSource<Task<T>>();

            task.ContinueWith((Task<T> innerTask) =>
            {
                if (innerTask.IsCanceled)
                    return source.TrySetCanceled();

                return source.TrySetResult(TaskUtil.Completed(innerTask.Result));
            },
                TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            task.ContinueWith((Task<T> innerTask) =>
            {
                try
                {
                    Task<T> resultTask = compensationTask(innerTask);
                    if (resultTask == null)
                        throw new InvalidOperationException("Sure could use a Task here buddy");

                    source.TrySetResult(resultTask);
                }
                catch (Exception ex)
                {
                    source.TrySetException(ex);
                }
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            return source.Task.FastUnwrap();
        }

        Task<T> FinallyAsync(Task<T> task, Action<T, TaskStatus> continuation, ExecuteOptions options)
        {
            var source = new TaskCompletionSource<T>();
            task.ContinueWith((Task<T> innerTask) =>
            {
                try
                {
                    continuation(_payload, innerTask.Status);
                    source.TrySetFromTask(innerTask);
                }
                catch (Exception ex)
                {
                    source.TrySetException(ex);
                }
            }, options.HasFlag(ExecuteOptions.RunSynchronously)
                ? TaskContinuationOptions.ExecuteSynchronously
                : TaskContinuationOptions.None);

            return source.Task;
        }
    }
}