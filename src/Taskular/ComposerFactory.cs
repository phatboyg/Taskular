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
    using TaskComposers;


    public static class ComposerFactory
    {
        public static Task Compose(Action<Composer> callback, CancellationToken cancellationToken = default(CancellationToken))
        {
            Composer composer = new TaskComposer(cancellationToken);

            callback(composer);

            return composer.Task;
        }

        public static Task<T> Compose<T>(T payload, Action<Composer<T>> callback,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Composer<T> composer = new TaskComposer<T>(payload, cancellationToken);

            callback(composer);

            return composer.Task;
        }
    }
}