#region Using

using DeathmicChatbot;
using NUnit.Framework;

#endregion


namespace DeathmicChatbotTests
{
    [TestFixture]
    internal class CounterTests
    {
        [Test]
        public void Count_CountNormally_ShouldFireCountRequestedEvent()
        {
            var subject = new Counter();

            var fired = false;

            subject.CountRequested += (sender, args) => fired = true;

            subject.Count("test");

            Assert.IsTrue(fired);
        }

        [Test]
        public void
            CounterReset_ResetCountedItem_ShouldFireCountResetRequestedEvent()
        {
            var subject = new Counter();

            var fired = false;

            subject.ResetRequested += (sender, args) => fired = true;

            subject.Count("test");

            subject.CounterReset("test");

            Assert.IsTrue(fired);
        }

        [Test]
        public void
            CounterReset_ResetNotCountedItem_ShouldNotFireCountResetRequestedEvent
            ()
        {
            var subject = new Counter();

            var fired = false;

            subject.ResetRequested += (sender, args) => fired = true;

            subject.CounterReset("test");

            Assert.IsFalse(fired);
        }

        [Test]
        public void CounterStats_CountedItem_ShouldFireStatRequestedEvent()
        {
            var subject = new Counter();

            var fired = false;

            subject.StatRequested += (sender, args) => fired = true;

            subject.Count("test");

            subject.CounterStats("test");

            Assert.IsTrue(fired);
        }

        [Test]
        public void CounterStats_UncountedItem_ShouldFireStatRequestedEvent()
        {
            var subject = new Counter();

            var fired = false;

            subject.StatRequested += (sender, args) => fired = true;

            subject.CounterStats("test");

            Assert.IsTrue(fired);
        }
    }
}