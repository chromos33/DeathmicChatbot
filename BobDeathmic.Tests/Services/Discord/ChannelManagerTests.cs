using BobDeathmic.Services.Discords;
using NUnit.Framework;
using System.Collections.Generic;

namespace BobDeathmic.Tests.Services.Discords
{
    [TestFixture]
    public class ChannelManagerTests
    {
        RelayChannelManager mng;
        [SetUp]
        public void Init()
        {
            mng = new RelayChannelManager(new List<string>() { "channel1", "channel2" });
        }
        [Test]
        public void Initialise_ChannelNames_InList()
        {
            int expectedAmount = 2;
            Assert.That(() => mng.ListCount, Is.EqualTo(expectedAmount));
        }

        [TestCase("channel3",false)]
        [TestCase("channel2",false)]
        [TestCase("stream_test",true)]
        public void AddChannel_ChannelName_ExpectedResult(string channelname,bool expectedresult)
        {
            Assert.That(() => mng.AddChannel(channelname), Is.EqualTo(expectedresult));
        }

        [TestCase("channel1", true)]
        [TestCase("channel3", false)]
        public void RemoveChannel_ChannelName_RexpectedResult(string channelname, bool expectedresult)
        {
            Assert.That(() => mng.RemoveChannel(channelname),Is.EqualTo(expectedresult));
        }

    }
}
