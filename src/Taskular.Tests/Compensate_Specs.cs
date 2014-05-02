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
    using System.Diagnostics;
    using NUnit.Framework;


    [TestFixture]
    public class Using_the_compensate_statement
    {
        [Test]
        public void Should_compensate_on_exception()
        {
            bool called = false;

            Composer composer = new TaskComposer();

            composer.Execute(() => { throw new InvalidOperationException("This is expected"); });

            composer.Compensate(compensation =>
            {
                called = true;

                return compensation.Handled();
            });

            composer.Task.Wait();

            Assert.IsTrue(called);
        }

        [Test]
        public void Should_compensate_on_exception_async()
        {
            bool called = false;

            Composer composer = new TaskComposer();

            composer.Delay(Debugger.IsAttached
                ? 30000
                : 1000);

            composer.Execute(() => { throw new InvalidOperationException("This is expected"); });

            composer.Compensate(compensation =>
            {
                called = true;

                return compensation.Handled();
            });

            composer.Task.Wait();

            Assert.IsTrue(called);
        }

        [Test]
        public void Should_throw_an_exception_without_compensation()
        {
            Composer composer = new TaskComposer();

            composer.Execute(() => { throw new InvalidOperationException("This is expected"); });

            var exception = Assert.Throws<AggregateException>(() => composer.Task.Wait());
            Assert.IsInstanceOf<InvalidOperationException>(exception.InnerException);
        }

        [Test]
        public void Should_throw_the_same_exception_if_not_handled()
        {
            bool called = false;

            Composer composer = new TaskComposer();

            composer.Execute(() => { throw new InvalidOperationException("This is expected"); });

            composer.Compensate(compensation =>
            {
                called = true;

                return compensation.Throw();
            });

            var exception = Assert.Throws<AggregateException>(() => composer.Task.Wait());
            Assert.IsInstanceOf<InvalidOperationException>(exception.InnerException);

            Assert.IsTrue(called);
        }

        [Test]
        public void Should_throw_the_specified_exception_if_handled()
        {
            bool called = false;

            Composer composer = new TaskComposer();

            composer.Execute(() => { throw new InvalidOperationException("This is expected"); });

            composer.Compensate(compensation =>
            {
                called = true;

                return compensation.Throw(new NotImplementedException("This is also expected"));
            });

            var exception = Assert.Throws<AggregateException>(() => composer.Task.Wait());
            Assert.IsInstanceOf<NotImplementedException>(exception.InnerException);

            Assert.IsTrue(called);
        }
    }
}