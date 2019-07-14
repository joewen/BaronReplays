using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace BaronReplays.Database
{
    public abstract class DatabaseManager : Object
    {
        protected SQLiteConnection sqlConnection;
        protected String fileName;

        protected DatabaseManager(String filename)
        {
            fileName = filename;
            sqlConnection = new SQLiteConnection("data source=" + fileName);
            if (File.Exists(fileName))
                OpenConnection();
        }


        protected void OpenConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
                sqlConnection.Open();
        }

        protected void CloseConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
                sqlConnection.Close();
        }

        protected SQLiteCommand CreateCommand()
        {
            return new SQLiteCommand(sqlConnection);
        }


        protected void CreateDatabaseFile()
        {
            SQLiteConnection.CreateFile(fileName);
        }

        protected void CreateDatabase()
        {
            CreateDatabaseFile();
            OpenConnection();
            CreateTables();
        }

        public void ClearDatabase()
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            CreateDatabase();
        }

        public void CreateIfNotExist()
        {
            if (!File.Exists(fileName))
            {
                CreateDatabase();
            }
        }

        protected void ExecuteSingleCommand(String cmdStr)
        {
            try
            {
                SQLiteCommand command = CreateCommand();
                command.CommandText = cmdStr;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("SQL execute query failed: {0}", e.Message));
            }
        }

        virtual protected SQLiteDataReader ExecuteQuery(String cmdStr)
        {
            SQLiteDataReader reader = null;
            try
            {
                SQLiteCommand command = CreateCommand();
                command.CommandText = String.Format(cmdStr);
                reader = command.ExecuteReader();

            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("SQL execute query failed: {0}", e.Message));
            }
            return reader;
        }

        virtual protected void CreateTables()
        {

        }
    }
}
