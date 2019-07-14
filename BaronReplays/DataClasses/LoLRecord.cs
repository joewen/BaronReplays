using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Collections.Specialized;
using System.Text;

namespace BaronReplays
{
    public class LoLRecord
    {
        public bool IsBroken;
        public const int LPRLatestVersion = 4;
        public int ThisLPRVersion = 4;

        public string relatedFileName;
        public String RelatedFileName
        {
            get { return relatedFileName; }
        }

        public char[] spectatorClientVersion;
        public long gameId;
        public long GameId
        {
            get { return gameId; }
        }

        public int gameChunkTimeInterval;
        public int gameKeyFrameTimeInterval;
        public int gameStartChunkId;
        public int gameEndKeyFrameId;

        public int gameEndChunkId;

        public int gameEndStartupChunkId;
        public int gameNextAvailableChunk = -1;

        public int gameLength;

        public int gameClientAddLag;
        public int gameELOLevel;

        public char[] gamePlatform;
        public String GamePlatform
        {
            get
            {
                return new String(gamePlatform); 
            }
        }
        public char[] observerEncryptionKey;

        public char[] gameCreateTime;
        public char[] gameStartTime;
        public String GameStartTime
        {
            get { return DateTime.Parse(new String(gameStartTime)).ToString(CultureInfo.CurrentCulture); }
        }
        public char[] gameEndTime;
        public String GameEndTime
        {
            get { return DateTime.Parse(new String(gameEndTime)).ToString(CultureInfo.CurrentCulture); }
        }

        public int gameDelayTime;
        public int gameLastChunkTime;
        public int gameLastChunkDuration;

        public char[] lolVersion;
        public String LoLVersion
        {
            get { return new String(lolVersion); }
        }

        public Boolean HasResult
        {
            get
            {
                return gameStats != null;
            }
        }

        public EndOfGameStats gameStats;

        public Dictionary<int, byte[]> gameChunks;
        public Dictionary<int, byte[]> gameKeyFrames;

        private byte[] endOfGameStatsBytes;
        public byte[] EndOfGameStatsBytes
        {
            get
            {
                return endOfGameStatsBytes;
            }
        }

        public PlayerInfo[] players = null;

        public LoLRecord()
        {
            this.allocateChunkAndKeyFrameSpaces();
        }


        public void lastChunkInfoAnalyze(JObject lastChunkJson)
        {
            this.gameEndChunkId = int.Parse(lastChunkJson["chunkId"].ToString());
            this.gameNextAvailableChunk = int.Parse(lastChunkJson["nextAvailableChunk"].ToString());
            this.gameLastChunkTime = int.Parse(lastChunkJson["availableSince"].ToString());
            this.gameLastChunkDuration = int.Parse(lastChunkJson["duration"].ToString());
            this.gameEndKeyFrameId = int.Parse(lastChunkJson["keyFrameId"].ToString());
            this.gameEndStartupChunkId = int.Parse(lastChunkJson["endStartupChunkId"].ToString());
            this.gameStartChunkId = int.Parse(lastChunkJson["startGameChunkId"].ToString());
        }

        public void gameMetaAnalyze(JObject metaJson)
        {
            JObject gameIdPfJson = JsonConvert.DeserializeObject<JObject>(metaJson["gameKey"].ToString());
            this.gameId = long.Parse(gameIdPfJson["gameId"].ToString());
            this.gamePlatform = gameIdPfJson["platformId"].ToString().ToCharArray();
            this.gameChunkTimeInterval = int.Parse(metaJson["chunkTimeInterval"].ToString());
            this.gameStartTime = metaJson["startTime"].ToString().ToCharArray();
            if (Boolean.Parse(metaJson["gameEnded"].ToString()))
                this.gameEndTime = metaJson["endTime"].ToString().ToCharArray();
            this.gameEndChunkId = int.Parse(metaJson["lastChunkId"].ToString());
            this.gameEndKeyFrameId = int.Parse(metaJson["lastKeyFrameId"].ToString());
            this.gameEndStartupChunkId = int.Parse(metaJson["endStartupChunkId"].ToString());
            this.gameDelayTime = int.Parse(metaJson["delayTime"].ToString());
            this.gameKeyFrameTimeInterval = int.Parse(metaJson["keyFrameTimeInterval"].ToString());
            this.gameStartChunkId = int.Parse(metaJson["startGameChunkId"].ToString());
            this.gameLength = int.Parse(metaJson["gameLength"].ToString());
            this.gameClientAddLag = int.Parse(metaJson["clientAddedLag"].ToString());
            this.gameELOLevel = int.Parse(metaJson["interestScore"].ToString());
            this.gameCreateTime = metaJson["createTime"].ToString().ToCharArray();
        }

        public string getMetaData()
        {
            JObject gameKey = new JObject();
            gameKey.Add("gameId", this.gameId);
            gameKey.Add("platformId",new String(this.gamePlatform));
            JObject meta = new JObject();
            meta.Add("gameKey", gameKey);
            meta.Add("gameServerAddress", "");
            meta.Add("port", 0);
            meta.Add("encryptionKey", "");
            meta.Add("chunkTimeInterval", this.gameChunkTimeInterval);
            meta.Add("startTime", new string(this.gameStartTime));
            if (this.gameEndTime != null)
            {
                meta.Add("endTime", new string(this.gameEndTime));
            }
            meta.Add("gameEnded", true);
            meta.Add("lastChunkId", this.gameEndChunkId);
            meta.Add("lastKeyFrameId", this.gameEndKeyFrameId);
            meta.Add("endStartupChunkId", this.gameEndStartupChunkId);
            meta.Add("delayTime", this.gameDelayTime);
            meta.Add("pendingAvailableChunkInfo", "");
            meta.Add("pendingAvailableKeyFrameInfo", "");
            meta.Add("keyFrameTimeInterval", this.gameKeyFrameTimeInterval);
            meta.Add("decodedEncryptionKey", "");
            meta.Add("startGameChunkId", this.gameStartChunkId);
            meta.Add("gameLength", this.gameLength);
            meta.Add("clientAddedLag", this.gameClientAddLag);
            meta.Add("clientBackFetchingEnabled", true);
            meta.Add("clientBackFetchingFreq", "50");
            meta.Add("interestScore", this.gameELOLevel);
            meta.Add("featuredGame", "false");
            meta.Add("createTime", new string(this.gameCreateTime));
            return meta.ToString();
        }

        public string getLastChunkInfo()
        {
            JObject lastChunkInfo = new JObject();
            lastChunkInfo.Add("chunkId", this.gameEndChunkId);
            lastChunkInfo.Add("availableSince", this.gameLastChunkTime);
            lastChunkInfo.Add("nextAvailableChunk", 10000);
            lastChunkInfo.Add("keyFrameId", this.gameEndKeyFrameId);

            lastChunkInfo.Add("endStartupChunkId", this.gameEndStartupChunkId);
            lastChunkInfo.Add("startGameChunkId", this.gameStartChunkId);
            if (gameEndTime == null)
            {
                lastChunkInfo.Add("nextChunkId", this.gameEndChunkId + 1);
                lastChunkInfo.Add("endGameChunkId", 0);
            }
            else
            {
                lastChunkInfo.Add("nextChunkId", this.gameEndChunkId);
                lastChunkInfo.Add("endGameChunkId", this.gameEndChunkId);
            }
            lastChunkInfo.Add("duration", this.gameLastChunkDuration);
            return lastChunkInfo.ToString();
        }

        public string getStartUpChunkInfo()
        {
            JObject lastChunkInfo = new JObject();
            lastChunkInfo.Add("chunkId", this.gameStartChunkId);
            lastChunkInfo.Add("availableSince", 30000);
            lastChunkInfo.Add("nextAvailableChunk", 1000);
            lastChunkInfo.Add("keyFrameId", 1);
            lastChunkInfo.Add("nextChunkId", this.gameStartChunkId + 1);
            lastChunkInfo.Add("endStartupChunkId", this.gameEndStartupChunkId);
            lastChunkInfo.Add("startGameChunkId", this.gameStartChunkId);
            lastChunkInfo.Add("endGameChunkId", 0);
            lastChunkInfo.Add("duration", 30000);
            return lastChunkInfo.ToString();
        }

        private void allocateChunkAndKeyFrameSpaces()
        {
            this.gameKeyFrames = new Dictionary<int, byte[]>();
            this.gameChunks = new Dictionary<int, byte[]>();
        }


        public byte[] getKeyFrameContent(int n)
        {
            if (gameKeyFrames.ContainsKey(n))
                return gameKeyFrames[n];
            return null;
        }

        public void setChunkContent(int n, byte[] contentBytes)
        {
            if (gameChunks.ContainsKey(n))
                gameChunks.Remove(n);
            gameChunks.Add(n, contentBytes);
        }

        public byte[] getChunkContent(int n)
        {
            if (gameChunks.ContainsKey(n))
                return gameChunks[n];
            return null;
        }


        public void setEndOfGameStats(byte[] content)
        {
            if (content == null)
            {
                Logger.Instance.WriteLog("EndOfGameStatas bytes is null");
                return;
            }
            this.endOfGameStatsBytes = content;
            this.gameStats = new EndOfGameStats(this.endOfGameStatsBytes);
            if (!this.gameStats.DecodeData())
            {
                return;
            }
            if (this.gameStats.Players != null)
            {
                if (this.gameStats.Players.Count > 0)
                {
                    this.players = new PlayerInfo[this.gameStats.Players.Count];
                    for (int i = 0; i < this.gameStats.Players.Count; i++)
                    {
                        this.players[i] = new PlayerInfo();
                        this.players[i].championName = this.gameStats.Players[i].SkinName;
                        this.players[i].playerName = this.gameStats.Players[i].SummonerName;
                        this.players[i].team = this.gameStats.Players[i].TeamId;
                        this.players[i].clientID = i;
                    }
                }
            }
        }


        public void writeToFile(string path)
        {
            FileStream lprFile = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryWriter dataWriter = new BinaryWriter(lprFile);
            dataWriter.Write(LPRLatestVersion);
            dataWriter.Write(this.spectatorClientVersion.Length);   //4 byte for recording spec version string length
            dataWriter.Write(this.spectatorClientVersion);          //n byte for recording spec version string
            dataWriter.Write(this.gameId);
            dataWriter.Write(this.gameEndStartupChunkId);
            dataWriter.Write(this.gameStartChunkId);
            dataWriter.Write(this.gameEndChunkId);
            dataWriter.Write(this.gameEndKeyFrameId);
            dataWriter.Write(this.gameLength);
            dataWriter.Write(this.gameDelayTime);
            dataWriter.Write(this.gameClientAddLag);
            dataWriter.Write(this.gameChunkTimeInterval);
            dataWriter.Write(this.gameKeyFrameTimeInterval);
            dataWriter.Write(this.gameELOLevel);
            dataWriter.Write(this.gameLastChunkTime);
            dataWriter.Write(this.gameLastChunkDuration);

            dataWriter.Write(this.gamePlatform.Length);
            dataWriter.Write(this.gamePlatform);
            dataWriter.Write(this.observerEncryptionKey.Length);
            dataWriter.Write(this.observerEncryptionKey);

            if (gameCreateTime == null)
            {
                gameCreateTime = string.Empty.ToCharArray();
            }
            dataWriter.Write(this.gameCreateTime.Length);
            dataWriter.Write(this.gameCreateTime);

            if (gameStartTime == null)
            {
                gameStartTime = string.Empty.ToCharArray();
            }
            dataWriter.Write(this.gameStartTime.Length);
            dataWriter.Write(this.gameStartTime);

            if (gameEndTime == null)
            {
                gameEndTime = string.Empty.ToCharArray();
            }
            dataWriter.Write(this.gameEndTime.Length);
            dataWriter.Write(this.gameEndTime);

            dataWriter.Write(this.lolVersion.Length);
            dataWriter.Write(this.lolVersion);

            dataWriter.Write(this.HasResult);
            if (this.HasResult)
            {
                dataWriter.Write(this.endOfGameStatsBytes.Length);
                dataWriter.Write(this.endOfGameStatsBytes);
            }

            if (this.players != null)
            {
                dataWriter.Write(true);
                dataWriter.Write(this.players.Length);
                foreach (PlayerInfo p in this.players)
                {
                    char[] pNameStr = p.playerName.ToCharArray();
                    dataWriter.Write(pNameStr.Length);
                    dataWriter.Write(pNameStr);
                    char[] cNameStr = p.championName.ToCharArray();
                    dataWriter.Write(cNameStr.Length);
                    dataWriter.Write(cNameStr);
                    dataWriter.Write(p.team);
                    dataWriter.Write(p.clientID);
                }
            }
            else
                dataWriter.Write(false);


            dataWriter.Write(gameKeyFrames.Count);
            foreach (KeyValuePair<int, byte[]> keyframe in gameKeyFrames)
            {
                dataWriter.Write(keyframe.Key);
                dataWriter.Write(keyframe.Value.Length);
                dataWriter.Write(keyframe.Value);
            }

            dataWriter.Write(gameChunks.Count);

            foreach (KeyValuePair<int, byte[]> chunk in gameChunks)
            {
                dataWriter.Write(chunk.Key);
                dataWriter.Write(chunk.Value.Length);
                dataWriter.Write(chunk.Value);
            }
            dataWriter.Close();
            lprFile.Close();
            this.relatedFileName = path;
        }


        public void readFromFile(string path, bool withOutChunks)
        {
            this.relatedFileName = path;
            try
            {

                FileStream lprFile = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryReader dataReader = new BinaryReader(lprFile);
                int lenTemp;
                this.ThisLPRVersion = dataReader.ReadInt32();
                if (this.ThisLPRVersion >= 0)   //Lowest need Version
                {
                    lenTemp = dataReader.ReadInt32();   //4 byte for recording spec version string length
                    this.spectatorClientVersion = dataReader.ReadChars(lenTemp);          //n byte for recording spec version string

                    this.gameId = dataReader.ReadInt64();
                    this.gameEndStartupChunkId = dataReader.ReadInt32();
                    this.gameStartChunkId = dataReader.ReadInt32();
                    this.gameEndChunkId = dataReader.ReadInt32();
                    this.gameEndKeyFrameId = dataReader.ReadInt32();
                    this.gameLength = dataReader.ReadInt32();
                    this.gameDelayTime = dataReader.ReadInt32();
                    this.gameClientAddLag = dataReader.ReadInt32();
                    this.gameChunkTimeInterval = dataReader.ReadInt32();
                    this.gameKeyFrameTimeInterval = dataReader.ReadInt32();
                    this.gameELOLevel = dataReader.ReadInt32();
                    this.gameLastChunkTime = dataReader.ReadInt32();
                    this.gameLastChunkDuration = dataReader.ReadInt32();

                    lenTemp = dataReader.ReadInt32();
                    this.gamePlatform = dataReader.ReadChars(lenTemp);
                    lenTemp = dataReader.ReadInt32();
                    this.observerEncryptionKey = dataReader.ReadChars(lenTemp);
                    lenTemp = dataReader.ReadInt32();
                    this.gameCreateTime = dataReader.ReadChars(lenTemp);
                    lenTemp = dataReader.ReadInt32();
                    this.gameStartTime = dataReader.ReadChars(lenTemp);
                    lenTemp = dataReader.ReadInt32();
                    this.gameEndTime = dataReader.ReadChars(lenTemp);

                    lenTemp = dataReader.ReadInt32();
                    this.lolVersion = dataReader.ReadChars(lenTemp);

                    Boolean resultExists = dataReader.ReadBoolean();
                    if (resultExists)
                    {
                        lenTemp = dataReader.ReadInt32();
                        this.endOfGameStatsBytes = dataReader.ReadBytes(lenTemp);
                        this.gameStats = new EndOfGameStats(this.endOfGameStatsBytes);
                    }
                    if (dataReader.ReadBoolean())
                        readPlayerOldFormat(dataReader);

                    if (!withOutChunks)
                        readChunks(dataReader);
                    dataReader.Close();
                    lprFile.Close();
                }
            }
             catch
            {
                IsBroken = true;
            }
        }

        public void readPlayerOldFormat(BinaryReader dataReader)
        {
            int lenTemp;
            this.players = new PlayerInfo[dataReader.ReadInt32()];
            for (int i = 0; i < this.players.Length; i++)
            {
                this.players[i] = new PlayerInfo();
                lenTemp = dataReader.ReadInt32();
                char[] pNameStr = dataReader.ReadChars(lenTemp);
                this.players[i].playerName = new string(pNameStr);
                lenTemp = dataReader.ReadInt32();
                char[] cNameStr = dataReader.ReadChars(lenTemp);
                this.players[i].championName = new string(cNameStr);
                this.players[i].team = dataReader.ReadUInt32();
                this.players[i].clientID = dataReader.ReadInt32();
            }
        }

        private void readChunks(BinaryReader dataReader)
        {
            int lenTemp;
            this.allocateChunkAndKeyFrameSpaces();
            if (ThisLPRVersion == 0)
                readChunksVersion0(dataReader);
            else
            {
                lenTemp = dataReader.ReadInt32();
                for (int i = 0; i < lenTemp; i++)
                {
                    int KeyFramesNumber = dataReader.ReadInt32();
                    int KeyFramesLen = dataReader.ReadInt32();
                    gameKeyFrames.Add(KeyFramesNumber, dataReader.ReadBytes(KeyFramesLen));
                }

                lenTemp = dataReader.ReadInt32();
                for (int i = 0; i < lenTemp; i++)
                {
                    int chunkNumber = dataReader.ReadInt32();
                    int chunkLen = dataReader.ReadInt32();
                    gameChunks.Add(chunkNumber, dataReader.ReadBytes(chunkLen));
                }
            }
        }

        private void readChunksVersion0(BinaryReader dataReader)
        {
            int lenTemp;
            for (int i = 0; i < this.gameEndKeyFrameId; i++)
            {
                lenTemp = dataReader.ReadInt32();
                gameKeyFrames.Add(i + 1, dataReader.ReadBytes(lenTemp));
            }
            for (int i = 0; i < this.gameEndStartupChunkId; i++)
            {
                lenTemp = dataReader.ReadInt32();
                gameChunks.Add(i + 1, dataReader.ReadBytes(lenTemp));
            }
            for (int i = 0; i <= this.gameEndChunkId - this.gameStartChunkId; i++)
            {
                lenTemp = dataReader.ReadInt32();
                gameChunks.Add(this.gameStartChunkId + i, dataReader.ReadBytes(lenTemp));
            }
        }



        public PlayerInfo[] GetTeamInfo(uint teamId)
        {
            List<PlayerInfo> teamPlayerList = new List<PlayerInfo>();
            foreach (PlayerInfo info in players)
            {
                if (info.team == teamId)
                    teamPlayerList.Add(info);
            }
            return teamPlayerList.ToArray();
        }

        public static SimpleLoLRecord GetSimpleLoLRecord(LoLRecord record)
        {
            if (record.IsBroken)
                return null;

            SimpleLoLRecord slr = new SimpleLoLRecord();
            try
            {
                slr.GameId = record.GameId;
                slr.GamePlatform = new String(record.gamePlatform);
                slr.FileName = record.relatedFileName;
                slr.LoLVersion = record.LoLVersion;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog(String.Format("Failed on get simple record: {0}", ex.Message));
                return null;
            }
            try
            {
                slr.BlueTeamPlayerInfo = record.GetTeamInfo(100);
                slr.PurpleTeamPlayerInfo = record.GetTeamInfo(200);
            }
            catch (Exception ex) 
            {
                Logger.Instance.WriteLog(String.Format("Failed on get simple record team info: {0}", ex.Message));
                return slr;
            }
            return slr;
        }


    }
}
