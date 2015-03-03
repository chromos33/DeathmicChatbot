#region Using

using System.Collections.Generic;

#endregion


namespace DeathmicChatbot.Interfaces
{
    public interface IModel
    {
        string GetMainNickForAlias(string sAlias);
        List<string> GetAliasesForUser(int sNick);
        int GetUserIDByNick(string sNick);
        string GetUserNickByID(int iID);
        void SaveUser(User user);
        void DeleteUser(User user);
    }
}