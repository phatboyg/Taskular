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
namespace Taskular.Compensations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;


    public class TaskCompensation<T> :
        Compensation<T>
    {
        readonly Exception _exception;
        readonly Task<T> _faultedTask;
        readonly T _payload;
        readonly CancellationToken _cancellationToken;

        public TaskCompensation(Task<T> faultedTask, T payload, CancellationToken cancellationToken)
        {
            _faultedTask = faultedTask;
            _payload = payload;
            _cancellationToken = cancellationToken;
            _exception = faultedTask.Exception != null
                ? faultedTask.Exception.GetBaseException()
                : null;
        }

        public Task<T> FaultedTask
        {
            get { return _faultedTask; }
        }

        T Compensation<T>.Payload
        {
            get { return _payload; }
        }

        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
        }

        Exception Compensation<T>.Exception
        {
            get { return _exception; }
        }

        CompensationResult<T> Compensation<T>.Handled()
        {
            return new Result(TaskUtil.Completed(_payload));
        }

        CompensationResult<T> Compensation<T>.Handled(T payload)
        {
            return new Result(TaskUtil.Completed(payload));
        }

        CompensationResult<T> Compensation<T>.Task(Task<T> task)
        {
            return new Result(task);
        }

        CompensationResult<T> Compensation<T>.Throw<TException>(TException exception)
        {
            return new Result(TaskUtil.Faulted<T>(exception));
        }

        CompensationResult<T> Compensation<T>.Throw()
        {
            return new Result(_faultedTask);
        }


        class Result :
            CompensationResult<T>,
            CompensationResult
        {
            readonly Task<T> _task;

            public Result(Task<T> task)
            {
                _task = task;
            }

            Task CompensationResult.Task
            {
                get { return _task; }
            }

            Task<T> CompensationResult<T>.Task
            {
                get { return _task; }
            }
        }
    }
}