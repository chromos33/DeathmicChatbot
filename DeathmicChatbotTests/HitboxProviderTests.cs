#region Using

using System;
using System.Collections.Generic;
using System.IO;
using DeathmicChatbot;
using DeathmicChatbot.StreamInfo.Hitbox;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using Stream = System.IO.Stream;

#endregion


namespace DeathmicChatbotTests
{
    [TestFixture]
    internal class HitboxProviderTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            SetUp_UseRandomTempDirectory();

            _restClient = Substitute.For<RestClient>("");

            _logManager = Substitute.For<LogManager>(Path.GetRandomFileName());

            _subject = new HitboxProvider(_restClient, _logManager);
        }

        [TearDown]
        public void TearDown()
        {
            _subject.Dispose();
            TearDown_UndoRandomTempDirectory();
        }

        #endregion

        private void SetUp_UseRandomTempDirectory()
        {
            do
            {
                _testDirectory = Path.Combine(Directory.GetCurrentDirectory(),
                                              Path.GetRandomFileName());
            } while (Directory.Exists(_testDirectory));

            Directory.CreateDirectory(_testDirectory);
            Directory.SetCurrentDirectory(_testDirectory);
        }

        private void TearDown_UndoRandomTempDirectory()
        {
            Directory.SetCurrentDirectory(_testDirectory + "\\..");
            Directory.Delete(_testDirectory, true);
        }

        private const string STREAM_NAME = "teststream";

        private HitboxProvider _subject;
        private LogManager _logManager;
        private RestClient _restClient;
        private string _testDirectory;
        private const string STREAM_REQUEST_RESPONSE_OFFLINE_SAMPLE_FILE = "StreamRequestResponseOffline.txt";
        private const string STREAM_REQUEST_RESPONSE_ONLINE_SAMPLE_FILE = "StreamRequestResponseOnline.txt";

        [Test]
        public void AddingStreamShouldSucceed()
        {
            _subject.RemoveStream(STREAM_NAME);

            var result = _subject.AddStream(STREAM_NAME);

            Assert.IsTrue(result);
        }

        [Test]
        public void AddingStreamTwiceShouldReturnFalseOnSecondTime()
        {
            _subject.AddStream(STREAM_NAME);

            var result = _subject.AddStream(STREAM_NAME);

            Assert.IsFalse(result);
        }

        [Test]
        public void CheckStreamsShouldThrowNoException()
        {
            _subject.AddStream(STREAM_NAME);

            _subject.CheckStreams();
        }

        [Test]
        public void GetStreamInfoArrayShouldReturnStringList()
        {
            var result = _subject.GetStreamInfoArray();
            Assert.IsInstanceOf<IEnumerable<string>>(result);
        }

        [Test]
        public void GetStreamInfoArrayWithDataShouldReturnData()
        {
            var streamReader = new StreamReader("..\\"+STREAM_REQUEST_RESPONSE_ONLINE_SAMPLE_FILE);
            var content = streamReader.ReadToEnd();
            
            var dummyRequestResponse = Substitute.For<IRestResponse>();
            dummyRequestResponse.Content = content;

            _restClient.Execute(Arg.Any<IRestRequest>())
                       .Returns(dummyRequestResponse);

            _subject.AddStream(STREAM_NAME);

            var result = _subject.GetStreamInfoArray();

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddingRunningStreamShouldFireStreamAddedEvent()
        {
            var streamReader = new StreamReader("..\\" + STREAM_REQUEST_RESPONSE_ONLINE_SAMPLE_FILE);
            var content = streamReader.ReadToEnd();
            streamReader.Close();

            var dummyRequestResponse = Substitute.For<IRestResponse>();
            dummyRequestResponse.Content = content;

            _restClient.Execute(Arg.Any<IRestRequest>())
                       .Returns(dummyRequestResponse);

            var fired = false;

            _subject.StreamStarted += (sender, args) => fired = true;

            _subject.AddStream(STREAM_NAME);

            Assert.IsTrue(fired);
        }

        [Test]
        public void InstancingWithLogManagerNullShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _subject = new HitboxProvider(_restClient, null));
        }

        [Test]
        public void InstancingWithRestClientNullShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _subject = new HitboxProvider(null, _logManager));
        }

        [Test]
        public void RemovingStreamShouldWork()
        {
            _subject.AddStream(STREAM_NAME);

            _subject.RemoveStream(STREAM_NAME);

            var result = _subject.AddStream(STREAM_NAME);

            Assert.IsTrue(result);
        }
    }
}