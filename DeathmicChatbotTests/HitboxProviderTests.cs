using System;
using System.IO;
using System.Security.Cryptography;
using DeathmicChatbot;
using NUnit.Framework;
using DeathmicChatbot.StreamInfo.Hitbox;
using NSubstitute;


namespace DeathmicChatbotTests
{
    [TestFixture]
    class HitboxProviderTests
    {
        [Test]
        public void AddingStreamShouldSucceed()
        {
            var logManager = Substitute.For<LogManager>("test");

            var subject = new HitboxProvider(logManager);

            const string STREAM_NAME = "teststream";

            subject.RemoveStream(STREAM_NAME);

            var result = subject.AddStream(STREAM_NAME);

            Assert.IsTrue(result);
        }
        
        [Test]
        public void AddingStreamTwiceShouldReturnFalseOnSecondTime()
        {
            var logManager = Substitute.For<LogManager>("test");

            var subject = new HitboxProvider(logManager);

            const string STREAM_NAME = "teststream";

            subject.AddStream(STREAM_NAME);

            var result = subject.AddStream(STREAM_NAME);

            Assert.IsFalse(result);
        }

        [Test]
        public void RemovingStreamShouldWork()
        {
            var logManager = Substitute.For<LogManager>("test");

            var subject = new HitboxProvider(logManager);

            const string STREAM_NAME = "teststream";

            subject.AddStream(STREAM_NAME);

            subject.RemoveStream(STREAM_NAME);

            var result = subject.AddStream(STREAM_NAME);

            Assert.IsTrue(result);
        }

        [Test]
        public void CheckStreamsShouldThrowNoException()
        {
            var logManager = Substitute.For<LogManager>("test");

            var subject = new HitboxProvider(logManager);

            const string STREAM_NAME = "teststream";

            subject.AddStream(STREAM_NAME);

            subject.CheckStreams();
        }
    }
}
