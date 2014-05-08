namespace Taskular.Tests
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using TaskComposers;


    [TestFixture]
    public class Using_a_delay
    {
        [Test]
        public void Should_delay_then_execute()
        {
            bool called = false;

            Composer composer = new TaskComposer();

            composer.Delay(100);
            composer.Execute(() => called = true);

            composer.Task.Wait();

            Assert.IsTrue(called);
        }

        [Test]
        public async void Should_compose_a_nested_delay()
        {
            int sequenceId = 0;
            int delayed = 0;
            int executed = 0;

            Composer composer = new TaskComposer();

            composer.ComposeTask(x =>
            {
                x.ExecuteTask(token => Task.Delay(100, token));
                x.Execute(() => delayed = ++sequenceId);
            });

            composer.Execute(() => executed = ++sequenceId);

            await composer.Task;

            Assert.AreEqual(1, delayed);
            Assert.AreEqual(2, executed);
        }
    }
}
