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
    namespace CancelT_Specs
    {
        using System.Threading;
        using System.Threading.Tasks;
        using NUnit.Framework;
        using TaskComposers;


        class Payload
        {
            public string Name { get; set; }
        }


        [TestFixture]
        public class When_canceling_a_composer
        {
            [Test]
            public async void Should_throw_task_canceled_exception()
            {
                var cts = new CancellationTokenSource();

                Composer<Payload> composer = new TaskComposer<Payload>(new Payload {Name = "Chris"}, cts.Token);

                composer.Execute(x =>
                    {
                        x.Name = "Joe";
                        cts.Cancel();
                    });

                composer.Execute(x =>
                    {
                        x.Name = "Mark";
                    });

                Assert.Throws<TaskCanceledException>(async () =>
                    {
                        var payload = await composer.Task;
                    });

                Assert.AreEqual("Joe", composer.Payload.Name);
            }
        }
    }
}