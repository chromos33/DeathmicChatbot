#region Using

using System;
using System.Data.SQLite;

#endregion


namespace DeathmicChatbot
{
    internal class ConnectionOpener : IDisposable
    {
        private readonly SQLiteConnection _connection;

        public ConnectionOpener(SQLiteConnection connection)
        {
            _connection = connection;
            _connection.Open();
        }

        #region IDisposable Members

        public void Dispose() { _connection.Close(); }

        #endregion
    }
}