/*
#region Using

using System.Collections.Generic;
using DeathmicChatbot.Interfaces;

#endregion


namespace DeathmicChatbot
{
    internal class Model : IModel
    {
        private readonly IDatabaseProvider _databaseProvider;
        private EntityFrameworkSQliteProvider entityFrameworkSQliteProvider;

        public Model(IDatabaseProvider databaseProvider) { _databaseProvider = databaseProvider; }

        public Model(EntityFrameworkSQliteProvider entityFrameworkSQliteProvider)
        {
            // TODO: Complete member initialization
            this.entityFrameworkSQliteProvider = entityFrameworkSQliteProvider;
        }

        #region IModel Members

        public string GetMainNickForAlias(string sAlias) { return _databaseProvider.GetMainNickForAlias(sAlias); }
        public List<string> GetAliasesForUser(int iID) { return _databaseProvider.GetAliasesForUser(iID); }
        public int GetUserIDByNick(string sNick) { return _databaseProvider.GetUserIDByNick(sNick); }
        public string GetUserNickByID(int iID) { return _databaseProvider.GetUserNickByID(iID); }
        public void SaveUser(User user) { _databaseProvider.SaveUser(user); }
        public void DeleteUser(User user) { _databaseProvider.DeleteUser(user); }

        #endregion
    }
}
*/