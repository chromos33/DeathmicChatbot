#region Using

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using DeathmicChatbot.Interfaces;
using DeathmicChatbot.Properties;

#endregion


namespace DeathmicChatbot
{
    internal class SqliteDatabaseProvider : IDatabaseProvider
    {
        private SQLiteConnection _connection;

        public SqliteDatabaseProvider()
        {
            CreateDatabase();
            BuildInfrastructure();
        }

        #region IDatabaseProvider Members

        public string GetMainNickForAlias(string sAlias)
        {
            var sNick = "";

            using (new ConnectionOpener(_connection))
            using (var command = new SQLiteCommand(string.Format(@"
SELECT
    users.nick
FROM
    users
    JOIN aliases ON (aliases.user = users.ID)
WHERE
    aliases.alias='{0}';
", sAlias), _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    sNick = Convert.ToString(reader["nick"]);
            }

            return sNick;
        }

        public List<string> GetAliasesForUser(int sNick)
        {
            var aliases = new List<string>();

            using (new ConnectionOpener(_connection))
            using (var command = new SQLiteCommand(string.Format(@"
SELECT
    aliases.alias
FROM
    aliases
    JOIN users ON (aliases.user = users.ID)
WHERE
    users.nick='{0}';
", sNick), _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var alias = Convert.ToString(reader["nick"]);
                    if (!string.IsNullOrEmpty(alias))
                        aliases.Add(alias);
                }
            }

            return aliases;
        }

        public int GetUserIDByNick(string sNick)
        {
            var iID = 0;

            using (new ConnectionOpener(_connection))
            using (var command = new SQLiteCommand(string.Format(@"
SELECT
    users.ID
FROM
    users
WHERE
    users.nick='{0}';
", sNick), _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    iID = Convert.ToInt32(reader["nick"]);
            }

            return iID;
        }

        public string GetUserNickByID(int iID)
        {
            var sNick = "";

            using (new ConnectionOpener(_connection))
            using (var command = new SQLiteCommand(string.Format(@"
SELECT
    users.nick
FROM
    users
WHERE
    users.ID='{0}';
", iID), _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    sNick = Convert.ToString(reader["nick"]);
            }

            return sNick;
        }

        public void SaveUser(User user)
        {
            SaveUser_SaveNick(user);
            SaveUser_SaveAliases(user);
        }

        public void DeleteUser(User user)
        {
            using (new ConnectionOpener(_connection))
            {
                using (var command = new SQLiteCommand(string.Format(@"
DELETE FROM user WHERE ID='{0}';
", user.ID)))
                    command.ExecuteNonQuery();

                using (var command = new SQLiteCommand(string.Format(@"
DELETE FROM aliases WHERE user='{0}';
", user.ID)))
                    command.ExecuteNonQuery();
            }
        }

        #endregion

        private void SaveUser_SaveAliases(User user)
        {
            using (new ConnectionOpener(_connection))
            {
                using (var command = new SQLiteCommand(string.Format(@"
DELETE FROM aliases WHERE user='{0}';
", user.ID)))
                    command.ExecuteNonQuery();

                foreach (var alias in user.Aliases)
                {
                    using (var command = new SQLiteCommand(string.Format(@"
INSERT INTO aliases(user, alias)
VALUES('{0}', '{1}');
", user.ID, alias)))
                        command.ExecuteNonQuery();
                }
            }
        }

        private void SaveUser_SaveNick(User user)
        {
            using (new ConnectionOpener(_connection))
            {
                if (user.ID == 0)
                {
                    using (var command = new SQLiteCommand(string.Format(@"
INSERT INTO users (nick)
VALUES ('{0}');
", user.Nick)))
                        command.ExecuteNonQuery();
                }

                else
                {
                    using (var command = new SQLiteCommand(string.Format(@"
UPDATE users
SET nick='{0}'
WHERE ID='{1}';
", user.Nick, user.ID)))
                        command.ExecuteNonQuery();
                }
            }
        }

        private void CreateDatabase()
        {
            _connection = new SQLiteConnection
            {
                ConnectionString =
                    "Data Source=" + Settings.Default.SqliteDbFileName
            };
        }

        private void BuildInfrastructure()
        {
            var creationQueries = new[]
            {
                "CREATE TABLE IF NOT EXISTS `users` ( `ID` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `nick` VARCHAR(15) NOT NULL )"
                ,
                "CREATE TABLE IF NOT EXISTS aliases (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, user INTEGER NOT NULL, alias VARCHAR(15) NOT NULL)"
                ,
                "CREATE TABLE IF NOT EXISTS user_visits (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, user INTEGER NOT NULL, type INTEGER NOT NULL, datetime DATETIME NOT NULL)"
            };

            using (new ConnectionOpener(_connection))
            {
                foreach (var creationQuery in creationQueries)
                {
                    using (var command = new SQLiteCommand(_connection))
                    {
                        command.CommandText = creationQuery;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}