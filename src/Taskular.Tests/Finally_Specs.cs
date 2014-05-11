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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public class Using_finally
    {
        [Test]
        public void Should_always_be_called()
        {
            bool called = false;

            Task task = ComposerFactory.Compose(composer =>
            {
                composer.Execute(() => { throw new InvalidOperationException("This is expected"); });

                composer.Finally(status => { called = true; });
            });

            Assert.Throws<InvalidOperationException>(async () => await task);

            Assert.IsTrue(called);
        }

        [Test]
        public async void Should_call_even_when_cancelled()
        {
            bool firstCalled = false;
            bool secondCalled = false;
            bool finallyCalled = false;

            var cancel = new CancellationTokenSource();

            Task task = ComposerFactory.Compose(composer =>
            {
                composer.Execute(() => firstCalled = true);
                composer.Execute(cancel.Cancel);
                composer.Execute(() => secondCalled = true);
                composer.Finally(() => finallyCalled = true);
            }, cancel.Token);

            Assert.Throws<TaskCanceledException>(async () => await task);

            Assert.IsTrue(firstCalled);
            Assert.IsFalse(secondCalled);
            Assert.IsTrue(finallyCalled);

            Assert.IsTrue(cancel.Token.IsCancellationRequested);
        }

        [Test]
        public async void Should_call_even_when_cancelled_synchronously()
        {
            bool firstCalled = false;
            bool secondCalled = false;
            bool finallyCalled = false;

            var cancel = new CancellationTokenSource();

            Task task = ComposerFactory.Compose(composer =>
            {
                composer.Execute(() => firstCalled = true, ExecuteOptions.RunSynchronously);
                composer.Execute(cancel.Cancel, ExecuteOptions.RunSynchronously);
                composer.Execute(() => secondCalled = true, ExecuteOptions.RunSynchronously);
                composer.Finally(() => finallyCalled = true, ExecuteOptions.RunSynchronously);
            }, cancel.Token);

            Assert.Throws<TaskCanceledException>(async () => await task);

            Assert.IsTrue(firstCalled);
            Assert.IsFalse(secondCalled);
            Assert.IsTrue(finallyCalled);

            Assert.IsTrue(cancel.Token.IsCancellationRequested);
        }
    }
}