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


        class Payload
        {
            public string Name { get; set; }
        }


        [TestFixture]
        public class When_canceling_a_composer
        {
            [Test]
            public void Should_throw_task_canceled_exception()
            {
                var cancel = new CancellationTokenSource();

                var payload = new Payload {Name = "Chris"};

                Task<Payload> task = ComposerFactory.Compose(payload, composer =>
                {
                    composer.Execute(x =>
                    {
                        x.Name = "Joe";
                        cancel.Cancel();
                    });

                    composer.Execute(x => { x.Name = "Mark"; });
                }, cancel.Token);

                Assert.Throws<TaskCanceledException>(async () => { Payload p = await task; });

                Assert.AreEqual("Joe", payload.Name);
            }
        }
    }
}