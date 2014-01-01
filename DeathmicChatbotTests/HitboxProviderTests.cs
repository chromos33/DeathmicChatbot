#region Using

using System.Collections.Generic;
using DeathmicChatbot.Interfaces;
using DeathmicChatbot.StreamInfo.Hitbox;
using NSubstitute;
using NUnit.Framework;
using RestSharp;

#endregion


namespace DeathmicChatbotTests
{
    [TestFixture]
    internal class HitboxProviderTests
    {
        private const string STREAM_NAME = "teststream";

        [Test]
        public void AddStream_AddStreamFirstTime_ReturnsTrue()
        {
            var restClientProvider = Substitute.For<IRestClientProvider>();
            var logManager = Substitute.For<ILogManagerProvider>();
            var textFile = Substitute.For<ITextFile>();
            textFile.ReadWholeFileInLines().Returns(new List<string>());

            var subject = new HitboxProvider(restClientProvider,
                                             logManager,
                                             textFile);

            var result = subject.AddStream(STREAM_NAME);

            Assert.IsTrue(result);
        }

        [Test]
        public void AddStream_AddStreamSecondTime_ReturnsFalse()
        {
            var restClientProvider = Substitute.For<IRestClientProvider>();
            var logManager = Substitute.For<ILogManagerProvider>();
            var textFile = Substitute.For<ITextFile>();
            textFile.ReadWholeFileInLines().Returns(new List<string>());

            var subject = new HitboxProvider(restClientProvider,
                                             logManager,
                                             textFile);

            subject.AddStream(STREAM_NAME);

            var result = subject.AddStream(STREAM_NAME);

            Assert.IsFalse(result);
        }

        [Test]
        public void CheckStreams_HasStreamAdded_CallsRestClientExecute()
        {
            var restClientProvider = Substitute.For<IRestClientProvider>();
            var logManager = Substitute.For<ILogManagerProvider>();
            var textFile = Substitute.For<ITextFile>();
            textFile.ReadWholeFileInLines().Returns(new List<string>());

            var subject = new HitboxProvider(restClientProvider,
                                             logManager,
                                             textFile);

            subject.AddStream(STREAM_NAME);

            subject.CheckStreams();

            restClientProvider.Received().Execute(Arg.Any<IRestRequest>());
        }

        [Test]
        public void RemoveStream_StreamWasAddedBeforeRemoving_DoesNotThrow()
        {
            var restClientProvider = Substitute.For<IRestClientProvider>();
            var logManager = Substitute.For<ILogManagerProvider>();
            var textFile = Substitute.For<ITextFile>();
            textFile.ReadWholeFileInLines().Returns(new List<string>());

            var subject = new HitboxProvider(restClientProvider,
                                             logManager,
                                             textFile);

            subject.AddStream(STREAM_NAME);

            subject.RemoveStream(STREAM_NAME);
        }
    }
}