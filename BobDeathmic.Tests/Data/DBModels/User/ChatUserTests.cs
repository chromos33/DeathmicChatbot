using System;
using System.Collections.Generic;
using System.Text;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Data.DBModels.User;
using NUnit.Framework;
using Microsoft.Extensions.Identity;

namespace BobDeathmic.Tests.Data.DBModels.User
{
    [TestFixture]
    class ChatUserTests
    {
        [Test]
        [TestCase("TestName",true)]
        [TestCase("StreamNotIncluded",false)]
        public void IsSubscribed_PreparedChatUserModelWithSubscriptions_ReturnsExpectedResult(string streamname, bool expectedResult)
        {
            Stream stream = new Stream("TestName");
            ChatUserModel model = new ChatUserModel(new List<Stream>() { stream });

            Assert.That(model.IsSubscribed(streamname), Is.EqualTo(expectedResult));


        }
        [Test]
        public void IsSubscribed_PreparedChatUserModelWithEmptySubscriptions_ReturnsFalse()
        {
            ChatUserModel model = new ChatUserModel();
            string streamName = "StreamName";

            Assert.That(model.IsSubscribed(streamName), Is.EqualTo(false));
        }
        [Test]
        public void IsSubscribed_EmptyStreamName_Throws()
        {
            ChatUserModel model = new ChatUserModel();
            string streamName = "";

            Assert.That(() => model.IsSubscribed(streamName), Throws.ArgumentException);
        }
        [Test]
        [TestCase("Test|UserName","TestUserName")]
        [TestCase("Test'fülk","Testfülk")]
        public void ChatUserModel_InputUserName_HasCorrectlyCleanedUserName(string UserName,string expectedResult)
        {
            ChatUserModel model = new ChatUserModel(UserName);

            Assert.That(model.UserName, Is.EqualTo(expectedResult));
        }
    }
}
