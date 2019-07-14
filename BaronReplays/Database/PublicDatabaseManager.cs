using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace BaronReplays.Database
{
    public class PublicDatabaseManager : DatabaseManager
    {
        private static PublicDatabaseManager instance;

        protected PublicDatabaseManager()
            : base(Constants.PublicDatabaseName)
        {
        }



        protected override void CreateTables()
        {
            base.CreateTables();
            ExecuteSingleCommand("CREATE TABLE Games(Id INTEGER, Platform Varchar(10), GameVersion Varchar(15),Length INTEGER, Time INTEGER, WinTeam INTEGER,GameMode TEXT,GameType TEXT, PRIMARY KEY (Id, Platform))");
            ExecuteSingleCommand("CREATE TABLE Players(Name TEXT, GameId INTEGER, Platform Varchar(10), Champion TEXT, Team INTEGER, Level INTEGER, K INTEGER, D INTEGER, A INTEGER, Spell1 INTEGER, Spell2 INTEGER, Item0 INTEGER, Item1 INTEGER, Item2 INTEGER, Item3 INTEGER, Item4 INTEGER, Item5 INTEGER, Item6 INTEGER, Gold INTEGER, Minions INTEGER, WardPlaced INTEGER, WardKilled INTEGER, PRIMARY KEY (Name, GameId, Platform),FOREIGN KEY(GameId) REFERENCES Games(Id))");
            ExecuteSingleCommand("CREATE TABLE Summoner(Id INTEGER, Platform Varchar(10), Name TEXT, PRIMARY KEY (Id, Platform))");
        }


        public void AddSummonerId(long Id, String name, String platform)
        {
            try
            {
                if (QuerySummonerName(Id, platform) == null)
                    ExecuteSingleCommand(String.Format("INSERT INTO Summoner(Id, Platform, Name) values ({0}, '{1}','{2}')", Id, platform, name));
                else
                    ExecuteSingleCommand(String.Format("UPDATE Summoner SET Name = '{0}' where Id = {1} AND Platform = '{2}'", name, Id, platform));
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Add id to summoner failed: {0}", e.Message));
            }
        }

        public SummonerDBDTO QuerySummonerName(long id, String platform)
        {
            SQLiteCommand command = CreateCommand();
            command.CommandText = String.Format("SELECT * FROM Summoner where Id = {0} AND Platform = '{1}'", id, platform);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                return new SummonerDBDTO(reader);
            }
            return null;
        }

        public SummonerDBDTO QuerySummonerId(String name, String platform)
        {
            SQLiteCommand command = CreateCommand();
            command.CommandText = String.Format("SELECT * FROM Summoner where Name = '{0}' AND Platform = '{1}'", name, platform);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                return new SummonerDBDTO(reader);
            }
            return null;
        }


        public void AddGame(LoLRecord[] records)
        {
            DbTransaction trans = sqlConnection.BeginTransaction();
            try
            {
                foreach (LoLRecord r in records)
                {
                    AddGame(r);
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
            }
        }

        public void AddGame(LoLRecord record)
        {
            try
            {
                String platform = new String(record.gamePlatform);
                if (!IsExistsGame(record.GameId, platform))
                {
                    UInt32 timeStamp = 0;
                    try
                    {
                        timeStamp = (uint)(DateTime.Parse(record.GameEndTime) - DateTime.MinValue).TotalSeconds;
                    }
                    catch (Exception)
                    {
                        timeStamp = (uint)(DateTime.Now - DateTime.MinValue).TotalSeconds;
                    }
                    
                    if (record.HasResult)
                    {
                        ExecuteSingleCommand(String.Format("INSERT INTO Games(Id, Platform,GameVersion , Length, Time, WinTeam, GameMode, GameType) values ({0}, '{1}', '{2}', {3}, {4}, {5}, '{6}', '{7}')", record.GameId, platform, record.LoLVersion, record.gameLength, timeStamp, record.gameStats.WonTeam, record.gameStats.GameMode, record.gameStats.GameType));
                        foreach (PlayerStats player in record.gameStats.Players)
                        {
                            AddPlayer(player, record.GameId, platform);
                        }
                    }
                    else
                    {
                        ExecuteSingleCommand(String.Format("INSERT INTO Games(Id, Platform, GameVersion, Length, Time) values ({0}, '{1}','{2}', {3}, {4})", record.GameId, platform, record.LoLVersion, record.gameLength, timeStamp));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Add game to public failed: {0}", e.Message));
            }
        }

        private void AddPlayer(PlayerStats player, long gameId, String platform)
        {
            try
            {
                ExecuteSingleCommand(String.Format("INSERT INTO Players(Name, GameId, Platform, Champion, Team, Level, K, D, A, Spell1, Spell2, Item0, Item1, Item2, Item3, Item4, Item5, Item6, Gold, Minions, WardPlaced, WardKilled) values ('{0}', {1}, '{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21})",
                    player.SummonerName, gameId, platform, player.SkinName, player.TeamId, player.Statistics.Level, player.Statistics.K, player.Statistics.D, player.Statistics.A, player.Spell1Id, player.Spell2Id, player.Statistics.Item0, player.Statistics.Item1, player.Statistics.Item2, player.Statistics.Item3, player.Statistics.Item4, player.Statistics.Item5, player.Statistics.Item6, player.Statistics.GoldEarned, player.Statistics.MinionsKilled + player.Statistics.NeutralMinionsKilled, player.Statistics.WardPlaced, player.Statistics.WardKilled));
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Add player failed: {0}", e.Message));
            }
        }

        public GameDBDTO QueryGame(String restriction = "")
        {
            SQLiteCommand command = CreateCommand();
            if (restriction == String.Empty)
                command.CommandText = String.Format("SELECT * FROM Games");
            else
                command.CommandText = String.Format("SELECT * FROM Games where {0}", restriction);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                return new GameDBDTO(reader);
            }
            return null;
        }

        public PlayerDBDTO[] QueryPlayer(String restriction = "")
        {
            SQLiteCommand command = CreateCommand();
            if (restriction == String.Empty)
                command.CommandText = String.Format("SELECT * FROM Players");
            else
                command.CommandText = String.Format("SELECT * FROM Players where {0}", restriction);
            SQLiteDataReader reader = command.ExecuteReader();
            List<PlayerDBDTO> result = new List<PlayerDBDTO>();
            while (reader.Read())
            {
                result.Add(new PlayerDBDTO(reader));
            }
            return result.ToArray();
        }

        public Boolean IsExistsGame(long gameId, String platform)
        {
            Boolean result = false;
            try
            {
                SQLiteCommand command = CreateCommand();
                command.CommandText = String.Format("SELECT * FROM Games where Id = {0} AND Platform = '{1}'", gameId, platform);
                SQLiteDataReader reader = command.ExecuteReader();
                result = reader.HasRows;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Query game failed: {0}", e.Message));
            }
            return result;
        }

        public static PublicDatabaseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PublicDatabaseManager();
                }
                return instance;
            }
        }


    }
}
