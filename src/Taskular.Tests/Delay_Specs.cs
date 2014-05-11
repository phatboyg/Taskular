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
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public class Using_a_delay
    {
        [Test]
        public async void Should_compose_a_nested_delay()
        {
            int sequenceId = 0;
            int delayed = 0;
            int executed = 0;

            Task task = ComposerFactory.Compose(composer =>
            {
                composer.ComposeTask(x =>
                {
                    x.ExecuteAsync(token => Task.Delay(100, token));
                    x.Execute(() => delayed = ++sequenceId);
                });

                composer.Execute(() => executed = ++sequenceId);
            });
            await task;

            Assert.AreEqual(1, delayed);
            Assert.AreEqual(2, executed);
        }

        [Test]
        public async void Should_delay_then_execute()
        {
            bool called = false;

            Task task = ComposerFactory.Compose(composer =>
            {
                composer.Delay(100);
                composer.Execute(() => called = true);
            });

            await task;

            Assert.IsTrue(called);
        }
    }
}