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


    public class TaskComposer :
        Composer
    {
        readonly CancellationToken _cancellationToken;
        readonly Composer<Unit> _composer;

        public TaskComposer(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cancellationToken = cancellationToken;

            _composer = new TaskComposer<Unit>(default(Unit), cancellationToken);
        }


        CancellationToken Composer.CancellationToken
        {
            get { return _cancellationToken; }
        }

        Task Composer.Task
        {
            get { return _composer.Task; }
        }

        Composer Composer.Execute(Action action, ExecuteOptions options)
        {
            _composer.Execute(x => action(), options);
            return this;
        }

        Composer Composer.ExecuteTask(Func<CancellationToken, Task> taskFactory, ExecuteOptions options)
        {
            _composer.ExecuteTask(async (x, token) =>
            {
                await taskFactory(token);

                return x;
            }, options);

            return this;
        }

        Composer Composer.Compensate(Func<Compensation, CompensationResult> compensation)
        {
            _composer.Compensate(x =>
            {
                var taskCompensation = new CompensationProxy<Unit>(x);

                CompensationResult compensationResult = compensation(taskCompensation);

                var result = compensationResult as CompensationResult<Unit>;
                if (result != null)
                    return result;

                Func<Task<Unit>> awaiter = async () =>
                {
                    await compensationResult.Task;

                    return x.Payload;
                };

                return x.Task(awaiter());
            });

            return this;
        }

        Composer Composer.Finally(Action<TaskStatus> continuation, ExecuteOptions options)
        {
            _composer.Finally((x, status) => continuation(status), options);
            return this;
        }

        Composer Composer.Fault<TException>(TException exception)
        {
            _composer.Fault(exception);
            return this;
        }


        struct Unit
        {
        }
    }
}