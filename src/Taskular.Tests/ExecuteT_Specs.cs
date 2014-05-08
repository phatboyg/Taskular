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
namespace Taskular.Tests
{
    namespace ExecuteT_Specs
    {
        using System;
        using System.Threading;
        using NUnit.Framework;
        using TaskComposers;


        class Payload
        {
            public string Name { get; set; }
        }


        [TestFixture]
        public class When_executing_a_task_of_t
        {
            [Test]
            public async void Should_use_async_processing()
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                Composer<Payload> composer = new TaskComposer<Payload>(new Payload {Name = "Chris"});

                int asyncThreadId = threadId;
                composer.Execute(x =>
                    {
                        x.Name = "Joe";
                        asyncThreadId = Thread.CurrentThread.ManagedThreadId;
                    });

                var payload = await composer.Task;

                Assert.AreEqual("Joe", payload.Name);
                Assert.AreNotEqual(threadId, asyncThreadId);
            }

            [Test]
            public void Should_use_async_processing_and_capture_exceptions()
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                Composer<Payload> composer = new TaskComposer<Payload>(new Payload {Name = "Chris"});

                int asyncThreadId = threadId;
                composer.Execute(x =>
                    {
                        x.Name = "Joe";
                        asyncThreadId = Thread.CurrentThread.ManagedThreadId;

                        throw new InvalidOperationException("This is expected");
                    });

                Assert.Throws<InvalidOperationException>(async () =>
                    {
                        var payload = await composer.Task;
                    });

                Assert.AreNotEqual(threadId, asyncThreadId);
            }
        }
    }
}