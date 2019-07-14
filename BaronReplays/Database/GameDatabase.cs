using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.Database
{
    public class GameDatabase
    {
        private static GameDatabase instance;
        private GameDatabase() { }

        public void InitDatabases()
        {
            PrivateDatabaseManager.Instance.CreateIfNotExist();
            PublicDatabaseManager.Instance.CreateIfNotExist();
        }

        public void AddGame(LoLRecord record)
        {
            PublicDatabaseManager.Instance.AddGame(record);
        }

        public void AddGame(LoLRecord[] records)
        {
            PublicDatabaseManager.Instance.AddGame(records);
        }

        #region Simple Redirection
        public Boolean IsExistsGame(long gameId, String platform)
        {
            return PublicDatabaseManager.Instance.IsExistsGame(gameId,platform);
        }

        public Boolean IsAFavoriteGame(long gameId, String platform)
        {
            return PrivateDatabaseManager.Instance.IsAFavoriteGame(gameId, platform);
        }

        public void AddToFavoriteGames(long gameId, String platform)
        {
            PrivateDatabaseManager.Instance.AddToFavoriteGames(gameId, platform);
        }

        public void DeleteFromFavoriteGames(long gameId, String platform)
        {
            PrivateDatabaseManager.Instance.DeleteFromFavoriteGames(gameId, platform);
        }
        public PlayerDBDTO[] QueryPlayer(String restriction = "")
        {
            return PublicDatabaseManager.Instance.QueryPlayer(restriction);
        }
        public GameDBDTO QueryGame(String restriction = "")
        {
            return PublicDatabaseManager.Instance.QueryGame(restriction);
        }
        public void RegisterGamePlayer(long gameId, String platform, String playerName)
        {
            PrivateDatabaseManager.Instance.RegisterLocalPlayer( gameId,platform, playerName);
        }
        public String QueryGamePlayer(long gameId, String platform)
        {
            return PrivateDatabaseManager.Instance.QueryLocalPlayer(gameId, platform);
        }
        #endregion


        public static GameDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameDatabase();
                }
                return instance;
            }
        }
    }
}
