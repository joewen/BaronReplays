using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using BaronReplays.Database;
using BaronReplays.BRapi.Model;

namespace BaronReplays
{
    public class LoLRecorder : ProcedureTask
    {
        public LoLExeData exeData;
        public int chunkGot;
        public int keyframeGot;
        public LoLRecord record;
        public event MainWindow.RecordDoneDelegate doneEvent;

        public delegate void RecordEventDelegate(LoLRecorder recoder);
        public event RecordEventDelegate infoDoneEvent;
        public event RecordEventDelegate prepareContentDoneEvent;

        public bool selfGame = false;
        public PlayerInfo selfPlayerInfo;
        public string platformAddress;
        public string specAddressPrefix;
        public bool externalStart = false;
        private static List<LoLRecorder> Recorders = new List<LoLRecorder>();
        public System.Timers.Timer lolExeDataGetterTimer;
        private GameInfo gameInfo;
        public GameInfo GameInfo
        {
            get
            {
                return gameInfo;
            }
        }

        public LoLRecorder(GameInfo gameinfo, bool externalStart = false)
        {

            this.externalStart = externalStart;
            this.gameInfo = gameinfo;
            this.record = new LoLRecord();
            if (externalStart)
            {
                exeData = LoLWatcher.Instance.ExecutionData;
                selfGame = !exeData.IsSpectatorMode;
            }
            
        }



        private void AddGeneralTasks()
        {
            if (externalStart)
            {
                if (selfGame)
                {
                    record.gamePlatform = Properties.Settings.Default.Platform.ToCharArray();
                    if (BaronReplays.RiotAPI.Services.Request.RegionName.ContainsKey(record.GamePlatform))
                    {
                        AddTask(new Task(getCurrentGameDataFromRiotApi), "ErrorDetectGameId");
                    }
                    else
                    {
                        AddTask(new Task(getCurrentGameDataFromBRapi), "ErrorDetectGameId");
                    }
                }
                else
                {
                    AddTask(new Task(WaitForPlayersInfo), "ErrorDetectGameId");
                }
            }

            AddTask(new Task(fillSpectatorInfo), "ErrorGettingSpecMode");
            AddTask(new Task(OutputGameInfo), String.Empty);
            AddTask(new Task(CheckDuplicateRecorder), "ErrorDuplicateGame");
            AddTask(new Task(waitForSpectatorStart), "ErrorGettingSpecMode");
            AddTask(new Task(GetVersions), "ErrorGettingContent");
            AddTask(new Task(getGameContentFromServer), "ErrorGettingContent");
            AddTask(new Task(updateGameMetaData), "ErrorGettingContent");
            if (selfGame)
                AddTask(new Task(writeLocalPlayerInfoToDB), String.Empty);
            TaskDoneEvent += RecordDone;
        }

        private bool writeLocalPlayerInfoToDB()
        {
            GameDatabase.Instance.RegisterGamePlayer(this.record.GameId, this.record.GamePlatform, selfPlayerInfo.PlayerName);
            return true;
        }

        private bool WaitForPlayersInfo()
        {
            Logger.Instance.WriteLog("WaitForPlayersInfo");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            do
            {
                this.record.players = this.readPlayersInfoFromR3dLog();
                if (this.record.players != null)
                {
                    Logger.Instance.WriteLog("Players info done.");
                    return true;
                }
                else
                {
                    SpinWait.SpinUntil(() => false, 10000);
                }
            }   //等個5分好了
            while (sw.Elapsed.TotalMinutes < 8.0);
            sw.Stop();
            return false;
        }


        private bool getCurrentGameDataFromRiotApi()
        {
            BaronReplays.RiotAPI.CurrentGameInfo info = null;
            int tryTimes = 0;
            do
            {
                info = RiotAPI.Services.CurrentGame.GetCurrentGameBySummonerId(exeData.SummonerId, record.GamePlatform);
                if (info == null)
                {
                    SpinWait.SpinUntil(() => false, 20000);
                }
            }
            while (info == null && tryTimes++ < 3);

            if (info == null)
            {
                Logger.Instance.WriteLog("Get current game data failed");
            }
            else
            {
                Logger.Instance.WriteLog(String.Format("Game mode: {0}", info.gameMode));
                Logger.Instance.WriteLog(String.Format("Game type: {0}", info.gameType));
                gameInfo = new GameInfo(Utilities.LoLObserveServersIpMapping[info.platformId], info.platformId, info.gameId, info.observers.encryptionKey);
                record.players = new PlayerInfo[info.participants.Count];
                for (int i = 0; i < info.participants.Count; i++)
                {
                    PlayerInfo pi = new PlayerInfo();

                    pi.championName = LoLStaticData.Champion.Instance.GetKeyById((int)info.participants[i].championId);
                    pi.playerName = info.participants[i].summonerName;
                    pi.team = (uint)info.participants[i].teamId;
                    pi.clientID = i;
                    record.players[i] = pi;

                    if (exeData.SummonerId == info.participants[i].summonerId)
                    {
                        selfPlayerInfo = pi;
                    }
                }
            }

            return !(info == null);
        }

        private bool getCurrentGameDataFromBRapi()
        {
            GameMetaData gmm; 

            int tryTimes = 0;
            do
            {
                gmm = BRapi.Service.GarenaService.GetGameBySummonerId(exeData.SummonerId, record.GamePlatform);
                if (gmm == null)
                {
                    SpinWait.SpinUntil(() => false, 20000);
                }
            }
            while (gmm == null && tryTimes++ < 3);

            if (gmm == null)
            {
                Logger.Instance.WriteLog("Get current game data failed");
            }
            else
            {
                gameInfo = new GameInfo(gmm.ObserverServerIp + ":" + gmm.ObserverServerPort, gmm.PlatformId, gmm.GameId, gmm.ObserverEncryptionKey);
                record.players = new PlayerInfo[gmm.TeamOne.Length + gmm.TeamTwo.Length];
                
                for (int i = 0; i < gmm.TeamOne.Length; i++)
                {
                    PlayerInfo pi = playerChampionSelectionToPlayerInfo(gmm.TeamOne[i], 100);
                    pi.clientID = i;
                    record.players[i] = pi;
                    if (gmm.TeamOne[i].SummonerId != 0)
                    {
                        selfPlayerInfo = pi;
                    }
                }

                int j = gmm.TeamOne.Length;
                for (int i = 0; i < gmm.TeamTwo.Length; i++)
                {
                    PlayerInfo pi = playerChampionSelectionToPlayerInfo(gmm.TeamTwo[i], 200);
                    pi.clientID = i+j;
                    record.players[i+j] = pi;
                    if (gmm.TeamTwo[i].SummonerId != 0)
                    {
                        selfPlayerInfo = pi;
                    }
                }
            }

            return !(gmm == null);
        }

        private PlayerInfo playerChampionSelectionToPlayerInfo(Player pcs, uint team)
        {
            PlayerInfo pi = new PlayerInfo();
            pi.championName = LoLStaticData.Champion.Instance.GetKeyById(pcs.ChampionId);
            pi.playerName = pcs.SummonerInternalName;
            pi.team = team;
            return pi;
        }


        public void startRecording()
        {
            AddGeneralTasks();
            RunTasks();
        }

        private void RecordDone(bool isSuccess, String msg)
        {
            if (isSuccess)
                getEndOfGameStats();    //這不成功也可以弄出可撥放的replay,所以不放在tasks list中
            removeFromList();
            StopRecording(isSuccess, msg);
            if (isSuccess)
            {
                Logger.Instance.WriteLog("Recording succeed.");
                CheckNewPlatform();
            }
            else
            {
                Logger.Instance.WriteLog("Recording failed because of " + msg);
            }

        }

        private void CheckNewPlatform()
        {
            String p = new String(record.gamePlatform);
            if (!Utilities.LoLObserveServersIpMapping.ContainsKey(p))
            {
                BRapi.Service.PlatformService.NewPlatform(p, platformAddress);
            }
            else
            {
                if (String.Compare(platformAddress, Utilities.LoLObserveServersIpMapping[p]) != 0)
                    BRapi.Service.PlatformService.NewPlatform(p, platformAddress);
            }
        }

        private String getPrefixUrl(String address)
        {
            return "http://" + address + "/observer-mode/rest/consumer/";
        }

        public void setPlatformAddress(String url)
        {
            this.platformAddress = url;
            this.specAddressPrefix = getPrefixUrl(this.platformAddress);
        }



        private bool testObserverServer(String address, int port = 80)
        {
            return null != getUrlString(String.Format("http://{0}:{1}/observer-mode/rest/consumer/getLastChunkInfo/{2}/{3}/30000/token", address, port, this.record.GamePlatform, this.record.gameId), 0);
        }

        private bool fillSpectatorInfo()
        {
            this.record.gameId = gameInfo.GameId;
            this.record.observerEncryptionKey = gameInfo.ObKey.ToCharArray();
            this.record.gamePlatform = gameInfo.PlatformId.ToCharArray();
            setPlatformAddress(gameInfo.ServerAddress);
            return true;
        }


        public void StopRecording(bool isSuccess, String reason)
        {
            this.doneEvent(this, isSuccess, reason);
        }

        private Boolean OutputGameInfo()
        {
            Logger.Instance.WriteLog(String.Format("Game ID: {0} {1}", this.record.GameId, this.record.GamePlatform));
            Logger.Instance.WriteLog(String.Format("Spectator server: {0} ", this.platformAddress));
            Logger.Instance.WriteLog(String.Format("ObserverEncryptionKey: {0}", new String(this.record.observerEncryptionKey)));
            return true;
        }

        private Boolean CheckDuplicateRecorder()
        {
            foreach (LoLRecorder r in Recorders)
            {
                if (this.record.gameId == r.record.gameId)
                {
                    Logger.Instance.WriteLog("Duplicate recorder found!");
                    return false;
                }
            }
            Recorders.Add(this);
            return true;
        }

        public void removeFromList()
        {
            if (Recorders.Contains(this))
                Recorders.Remove(this);
        }

        private Boolean IsAObserverServer(String address)
        {
            String ver = getUrlString(getPrefixUrl(address) + "version", 0);
            return ver != null;
        }

        private bool GetVersions()
        {
            String lolv = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.LoLGameExe).ProductVersion;
            String specv = getUrlString(this.specAddressPrefix + "version");
            this.record.lolVersion = lolv.ToCharArray();
            this.record.spectatorClientVersion = specv.ToCharArray();
            Logger.Instance.WriteLog(String.Format("LoL version is {0}, spectator version is {1}", lolv, specv));
            return true;
        }

        private bool updateGameMetaData()
        {
            JObject meta = this.getGameMeta();
            if (meta == null)
                return false;
            this.record.gameMetaAnalyze(meta);
            return true;
        }

        private bool updateLastChunkInfo()
        {
            JObject lastChunkInfo = this.getLastChunkInfo();
            if (lastChunkInfo == null)
                return false;
            this.record.lastChunkInfoAnalyze(lastChunkInfo);
            return true;
        }

        private bool getEndOfGameStats()
        {
            Logger.Instance.WriteLog("Get end of game stats.");
            string result = getUrlString(this.specAddressPrefix + "endOfGameStats/" + new String(this.record.gamePlatform) + "/" + this.record.gameId + "/null");
            byte[] encodedDataAsBytes = null;

            try
            {
                encodedDataAsBytes = Convert.FromBase64String(result);
                //encodedDataAsBytes = Convert.FromBase64String(result.Replace(" ", "+"));
                record.setEndOfGameStats(encodedDataAsBytes);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog(String.Format("Can not get end of game stats due to {0}", ex.Message));
                return false;
            }
            return true;
        }

        private PlayerInfo[] readPlayersInfoFromR3dLog()
        {
            PlayerInfo[] result = null;
            try
            {
                if (exeData == null)
                    return null;
                String log = exeData.LogContent;

                Regex gameIdRegex = new Regex(@"Spawning champion [(](?<CNAME>.+)[)] with skinID [0-9]+ on team (?<TEAM>[0-9]+) for clientID (?<CID>-*[0-9]+) and summonername [(](?<PNAME>.+)[)] [(]is", RegexOptions.IgnoreCase);
                MatchCollection matches = gameIdRegex.Matches(log);
                if (matches.Count == 0)
                    return null;
                result = new PlayerInfo[matches.Count];

                for (int i = 0; i < matches.Count; i++)
                    result[i] = new PlayerInfo(matches[i].Groups["PNAME"].Value, matches[i].Groups["CNAME"].Value, UInt32.Parse(matches[i].Groups["TEAM"].Value), Int32.Parse(matches[i].Groups["CID"].Value));
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("readPlayersInfoFromR3dLog failed: {0}", e.Message));
            }
            return result;
        }

        private int hashCode(string str)
        {
            string stardardName = str.ToLower();

            int result = 0;
            for (int i = 0; i < str.Length; i++)
            {
                int exp = (int)(str.Length) - (i + 1);
                result += (Convert.ToInt32(stardardName[i]) * Pow(31, exp));
            }
            return result;
        }

        private int Pow(int x, int pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }


        private JObject getUrlJson(string url)
        {
            JObject result = null;
            try
            {
                result = JsonConvert.DeserializeObject<JObject>(getUrlString(url));
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        private string getUrlString(string url, int maxRetryTimes = 1)
        {
            string result = null;
            using (WebClient wc = new WebClient())
            {
                do
                {
                    try
                    {
                        result = wc.DownloadString(url);
                    }
                    catch (WebException we)
                    {
                        Logger.Instance.WriteLog(String.Format("Get {0} failed, {1}", url, we.Message));
                        if (maxRetryTimes-- > 0)
                        {
                            Thread.Sleep(5000);
                        }
                        else
                            return null;
                    }
                }
                while (result == null);
            }
            return result;
        }


        private bool waitForSpectatorStart()
        {
            this.infoDoneEvent(this);
            if (!this.selfGame)
                return true;
            int restTime = 0;
            DateTime funcStartTime = DateTime.Now;
            do
            {
                Thread.Sleep(10000);
                updateLastChunkInfo();
                restTime = this.record.gameNextAvailableChunk;
                if ((DateTime.Now - funcStartTime).TotalMinutes > 3.0)
                {
                    return false;
                }
            }
            while (restTime < 0);
            Logger.Instance.WriteLog(String.Format("Wait {0} msecs for data available", restTime));

            SpinWait.SpinUntil(() => false, restTime);
            return true;
        }


        private bool getGameContentFromServer()
        {
            this.updateLastChunkInfo();
            Logger.Instance.WriteLog(String.Format("End startup chunk is {0}, Game start chunk is {1}", record.gameEndStartupChunkId, record.gameStartChunkId));
            this.chunkGot = 0;
            this.keyframeGot = 0;
            int restTime = 0;
            bool firstLoop = true;
            updateGameMetaData();
            do
            {
                this.updateLastChunkInfo();
                if (!this.getGameChunks())
                    return false;

                if (!this.getKeyFrames())
                    return false;

                if ((prepareContentDoneEvent) != null && firstLoop) //如果是第一圈取得資料..回報給外面知道，就可以開始播放replay
                {
                    firstLoop = false;
                    prepareContentDoneEvent(this);
                }

                restTime = this.record.gameNextAvailableChunk;
                System.Threading.Thread.Sleep(restTime);
            }
            while (restTime != 0);

            return true;
        }

        public JObject getGameMeta()
        {
            return getUrlJson(this.specAddressPrefix + "getGameMetaData/" + this.record.GamePlatform + "/" + this.record.gameId + "/1/token");
        }

        public JObject getLastChunkInfo()
        {
            return getUrlJson(this.specAddressPrefix + "getLastChunkInfo/" + this.record.GamePlatform + "/" + this.record.gameId + "/30000/token");
        }

        private byte[] downloadData(String url)
        {
            for (int i = 1; i < 3; i++)
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        return wc.DownloadData(url);
                    }
                    catch (WebException we)
                    {
                        Logger.Instance.WriteLog($"Download {url} data failed: {we.Message} x {i}");
                        Thread.Sleep(10000);
                    }
                }
            }
            return null;
        }

        private bool downloadKeyFrame(int nth)
        {
            byte[] data;
            if (this.record.gameKeyFrames.ContainsKey(nth))
                data = this.record.gameKeyFrames[nth];
            else
            {
                data = downloadData(this.specAddressPrefix + "getKeyFrame/" + this.record.GamePlatform + "/" + this.record.gameId + "/" + nth + "/token");
                this.record.gameKeyFrames.Add(nth, data);
            }
            return (data != null);
        }

        private bool downloadChunk(int nth)
        {
            byte[] data;
            if (this.record.gameChunks.ContainsKey(nth))
                data = this.record.gameChunks[nth];
            else
            {
                data = downloadData(this.specAddressPrefix + "getGameDataChunk/" + this.record.GamePlatform + "/" + this.record.gameId + "/" + nth + "/token");
                this.record.gameChunks.Add(nth, data);
            }
            return (data != null);
        }


        private bool getGameChunks()
        {
            for (int i = chunkGot + 1; i <= this.record.gameEndChunkId; i++)
            {
                if (this.downloadChunk(i))
                    chunkGot = i;
                else
                    return false;
                //this.downloadChunk(chunkGot); //Test for Taiwan
            }
            return true;
        }

        private bool getKeyFrames()
        {
            for (int i = this.keyframeGot + 1; i <= this.record.gameEndKeyFrameId; i++)
            {
                if (this.downloadKeyFrame(i))
                    keyframeGot = i;
                else
                    return false;
                //this.downloadKeyFrame(this.keyframeGot); //Test for Taiwan
            }
            return true;
        }
    }
}
