#region Using

using DeathmicChatbot;
using DeathmicChatbot.Interfaces;
using NSubstitute;
using NUnit.Framework;

#endregion


namespace DeathmicChatbotTests
{
    [TestFixture]
    internal class UserTests
    {
        private const string NICK = "test";
        private const int ID = 1;

        [Test]
        public void Constructor_IDGiven_ShouldGetDataFromModel()
        {
            var model = Substitute.For<IModel>();

            new User(model, ID);

            model.Received().GetUserNickByID(ID);
        }

        [Test]
        public void Constructor_NickGiven_ShouldGetUserIDFromModel()
        {
            var model = Substitute.For<IModel>();

            new User(model, NICK);

            model.Received().GetUserIDByNick(NICK);
        }

        [Test]
        public void Delete_ShouldDeleteDataFromModel()
        {
            var model = Substitute.For<IModel>();

            var subject = new User(model);

            subject.Delete();

            model.Received().DeleteUser(subject);
        }

        [Test]
        public void GetByAlias_ExistingAlias_ShouldReturnUserObject()
        {
            var model = Substitute.For<IModel>();
            model.GetMainNickForAlias(NICK).Returns(NICK);

            var result = User.GetByAlias(model, NICK);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetByAlias_NonExistingAlias_ShouldReturnNull()
        {
            var model = Substitute.For<IModel>();
            model.GetMainNickForAlias(NICK).Returns("");

            var result = User.GetByAlias(model, NICK);

            Assert.IsNull(result);
        }

        [Test]
        public void Save_ShouldSaveDataToModel()
        {
            var model = Substitute.For<IModel>();

            var subject = new User(model);

            subject.Save();

            model.Received().SaveUser(subject);
        }
    }
}