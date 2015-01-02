using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using DeathmicChatbot.Properties;

namespace DeathmicChatbot
{
    class SQLiteDBController
    {

        private SQLiteConnection _connection;
        private 
void createDBFile()
        {
            SQLiteConnection.CreateFile(Settings.Default.SqliteDbFileName);
        }
        private SQLiteConnection connectDB()
        {
            return new SQLiteConnection("Data Source =" + Settings.Default.SqliteDbFileName);

        }
        public void initTables()
        {
            var initQueries = new[]
            {
                "CREATE TABLE IF NOT EXISTS 'user' ('ID' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,'nick' VARCHAR(20) NOT NULL,'visits' INTEGER NOT NULL, 'lastvisit' DATETIME NOT NULL)"
                ,
                "CREATE TABLE IF NOT EXISTS 'alias' ('ID' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,'user' INTEGER NOT NULL,'alias' VARCHAR(20) NOT NULL)"
                ,
                "CREATE TABLE IF NOT EXISTS 'stream' ('ID' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,'title' INTEGER NOT NULL,'starttime' DATETIME NOT NULL, 'lastannouncement' DATETIME NOT NULL)"
                
            };
            createDBFile();
            var DBConnection = connectDB();
            DBConnection.Open();
            foreach(var query in initQueries)
            {
                SQLiteCommand command = new SQLiteCommand(query, DBConnection);
                command.ExecuteNonQuery();
            }
            DBConnection.Close();
        }

        public SQLiteDataReader executeSearchQuery(string table, string field, string join = null, string filter = null)
        {
            
            string query = "SELECT "+table+"."+field;
            query += " FROM " + table;
            //join with complete instructions ("JOIN something ON (Condition)") in case of multi join
            if(join != null)
            {
                query +=" " + join;
            }
            //filter with complete instructions in case of multiple Where Clauses
            if(filter != null)
            {
                query += " " + filter;
            }
            var DBConnection = connectDB();
            DBConnection.Open();
            var command = new SQLiteCommand(query, DBConnection);

            return command.ExecuteReader();
        }
        public string executeWriteQuery(string _type, string _table, string _value = null, string _field = null, string[] _values = null, string[] _fields = null, string _filter = null)
        {
            System.Diagnostics.Debug.WriteLine("test");
            // _filter starts after the "WHERE"
            string query = "";
            switch (_type)
            {
                case "insert":
                    
                    query = "INSERT INTO " + _table +"(";
                    if(_fields!=null)
                    {
                        foreach(var field in _fields)
                        {
                            query += field+",";
                        }
                        query = query.Remove(query.Length - 1);
                        query += ")";
                    }
                    //check if this makes sense, probably not but too brainfucked right now.
                    //if (field != null)
                    //{
                    //    query +="." + field;
                    //}
                    if (_filter != null)
                    {
                        query += " " + _filter;
                    }
                    if(_value != null)
                    {
                        query += " Values('"+ _value +"')";
                    }
                    else if(_values != null)
                    {
                    query += " Values(";

                    foreach(string value in _values)
                    {
                        query += "'" + value +"',";
                    }
                    query = query.Remove(query.Length -1);
                    query += ")";
                    }
                    System.Diagnostics.Debug.WriteLine(query);
                    
                    break;
                case "update":
                    query = "UPDATE " + _table;
                    query += " SET " + _field + "='" + _value + "'";
                    if(_filter!=null)
                    {
                        query += " WHERE"+ _filter;
                    }

                    break;
                case "delete":
                    query = "DELETE FROM " + _table + "WHERE" + _filter;
                    break;
                default: return "unspecified/declared type of WriteQuery";
            }

                var DBConnection = connectDB();
                DBConnection.Open();


                var command = new SQLiteCommand(query, DBConnection);

                command.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine("success");
                return "success";

            
        }


    }
}
