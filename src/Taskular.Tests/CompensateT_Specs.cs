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
    namespace CompensateT_Specs
    {
        using System;
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
            public void Should_throw_the_same_exception_if_unhandled()
            {
                Composer<Payload> composer = new TaskComposer<Payload>(new Payload {Name = "Chris"});

                composer.Execute(x =>
                {
                    x.Name = "Joe";
                    throw new InvalidOperationException("This is expected");
                });

                bool compensated = false;
                composer.Compensate(compensation =>
                {
                    compensated = true;
                    return compensation.Throw();
                });

                Assert.Throws<InvalidOperationException>(async () => { Payload payload = await composer.Task; });

                Assert.IsTrue(compensated);
            }

            [Test]
            public void Should_throw_the_specified_exception_if_unhandled()
            {
                Composer<Payload> composer = new TaskComposer<Payload>(new Payload {Name = "Chris"});

                composer.Execute(x =>
                {
                    x.Name = "Joe";
                    throw new InvalidOperationException("This is expected");
                });

                bool compensated = false;
                composer.Compensate(compensation =>
                {
                    compensated = true;
                    return compensation.Throw(new NotImplementedException("This is also expected"));
                });

                Assert.Throws<NotImplementedException>(async () => { Payload payload = await composer.Task; });

                Assert.IsTrue(compensated);
            }

            [Test]
            public async void Should_use_async_processing()
            {
                var originalPayload = new Payload {Name = "Chris"};

                Composer<Payload> composer = new TaskComposer<Payload>(originalPayload);

                composer.Execute(x =>
                {
                    x.Name = "Joe";
                    throw new InvalidOperationException();
                });

                composer.Compensate(x =>
                {
                    x.Payload.Name = "Mark";
                    return x.Handled();
                });

                Payload payload = await composer.Task;

                Assert.AreEqual("Mark", payload.Name);
            }
        }
    }
}