using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace BaronReplays.Database
{
    public class PrivateDatabaseManager : DatabaseManager
    {
        private static PrivateDatabaseManager instance;
        

        protected PrivateDatabaseManager()
            : base(Constants.PrivateDatabaseName)
        {
        }


        protected override void CreateTables()
        {
            base.CreateTables();
            ExecuteSingleCommand("CREATE TABLE Favorites(GameId INTEGER, Platform Varchar(10), PRIMARY KEY (GameId, Platform))");
            ExecuteSingleCommand("CREATE TABLE LocalPlayer(GameId INTEGER, Platform Varchar(10), Name TEXT, PRIMARY KEY (GameId, Platform))");
        }


        public void RegisterLocalPlayer(long gameId, String platform, String playerName)
        {
            try
            {
                ExecuteSingleCommand(String.Format("INSERT INTO LocalPlayer(GameId,Platform,Name) values ({0}, '{1}','{2}')", gameId, platform, playerName));
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Register local game player failed: {0}", e.Message));
            }
        }

        public String QueryLocalPlayer(long gameId, String platform)
        {
            SQLiteCommand command = CreateCommand();
            command.CommandText = String.Format("SELECT * FROM LocalPlayer where GameId = {0} AND Platform = '{1}'", gameId, platform);
            SQLiteDataReader reader = command.ExecuteReader();
            String result = String.Empty;
            if (reader.Read())
            {
                result = reader["Name"].ToString();
            }
            return result;
        }


        public void AddToFavoriteGames(long gameId, String platform)
        {
            try
            {
                ExecuteSingleCommand(String.Format("INSERT INTO Favorites(GameId,Platform) values ({0}, '{1}')", gameId, platform));
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Add game to favorite failed: {0}", e.Message));
            }
        }

        public void DeleteFromFavoriteGames(long gameId, String platform)
        {
            try
            {
                ExecuteSingleCommand(String.Format("DELETE FROM Favorites where GameId = {0} AND Platform = '{1}'", gameId, platform));
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Delete game from favorite failed: {0}", e.Message));
            }
        }



        public Boolean IsAFavoriteGame(long gameId, String platform)
        {
            Boolean result = false;
            SQLiteCommand command = CreateCommand();
            try
            {
                command.CommandText = String.Format("SELECT * FROM Favorites where GameId = {0} AND Platform = '{1}'", gameId, platform);
                SQLiteDataReader reader = command.ExecuteReader();
                result = reader.HasRows;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Query favorite game failed: {0}", e.Message));
            }
            return result;
        }



        public static PrivateDatabaseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PrivateDatabaseManager();
                }
                return instance;
            }
        }
    }
}
