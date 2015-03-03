#region Using

using System.Collections.Generic;
using DeathmicChatbot.Interfaces;

#endregion


namespace DeathmicChatbot
{
    public class User
    {
        private readonly IModel _model;

        public User(IModel model, string nick = "")
        {
            _model = model;
            Nick = nick;

            if (Nick != "")
                LoadUserByNick();
        }

        public User(IModel model, int id)
        {
            _model = model;
            ID = id;
            LoadUserByID();
        }

        public string Nick { get; set; }

        public int ID { get; private set; }

        public List<string> Aliases { get; private set; }

        private void LoadUserByNick()
        {
            ID = _model.GetUserIDByNick(Nick);
            LoadUserByID();
        }

        private void LoadUserByID()
        {
            LoadUserNick(ID);
            LoadUserAliases(ID);
        }

        private void LoadUserNick(int iID) { Nick = _model.GetUserNickByID(iID); }

        private void LoadUserAliases(int iID) { Aliases = _model.GetAliasesForUser(iID); }

        public static User GetByAlias(IModel model, string sAlias)
        {
            var sMainNick = GetMainNickByAlias(model, sAlias);
            return string.IsNullOrEmpty(sMainNick)
                       ? null
                       : new User(model, sMainNick);
        }

        private static string GetMainNickByAlias(IModel model, string sAlias) { return model.GetMainNickForAlias(sAlias); }

        public void Save()
        {
            _model.SaveUser(this);
            if (ID != 0)
                LoadUserByID();
            else if (Nick != "")
                LoadUserByNick();
        }

        public void Delete() { _model.DeleteUser(this); }
    }
}