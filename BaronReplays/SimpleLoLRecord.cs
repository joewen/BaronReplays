using BaronReplays.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BaronReplays
{
    public class SimpleLoLRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Boolean favorite;
        public Boolean Favorite
        {
            get
            {
                return GameDatabase.Instance.IsAFavoriteGame(GameId, GamePlatform);
            }
            set
            {
                favorite = value;
                if (value)
                {
                    GameDatabase.Instance.AddToFavoriteGames(GameId, GamePlatform);
                }
                else
                {
                    GameDatabase.Instance.DeleteFromFavoriteGames(GameId, GamePlatform);
                }
                NotifyPropertyChanged("Favorite");
            }
        }

        private long gameId;
        public long GameId
        {
            get
            {
                return gameId;
            }
            set
            {
                gameId = value;
                NotifyPropertyChanged("GameId");
            }
        }

        private String gamePlatform;
        public String GamePlatform
        {
            get
            {
                return gamePlatform;
            }
            set
            {
                gamePlatform = value;
                NotifyPropertyChanged("GamePlatform");
            }
        }

        private String _lolVersion;
        public String LoLVersion
        {
            get
            {
                return _lolVersion;
            }
            set
            {
                _lolVersion = value;
                NotifyPropertyChanged("LolVersion");
            }
        }

        private LoLRecorder _recoringRecorder;
        public LoLRecorder RecoringRecorder
        {
            get
            {
                return _recoringRecorder;
            }
            set
            {
                _recoringRecorder = value;
                NotifyPropertyChanged("RecoringRecorder");
            }
        }

        public Boolean IsRecording
        {
            get
            {
                return RecoringRecorder != null;
            }
        }


        private PlayerInfo[] blueTeamPlayerInfo;
        public PlayerInfo[] BlueTeamPlayerInfo
        {
            get
            {
                return blueTeamPlayerInfo;

            }
            set
            {
                blueTeamPlayerInfo = value;
                NotifyPropertyChanged("BlueTeamPlayerInfo");
            }
        }

        private PlayerInfo[] purpleTeamPlayerInfo;
        public PlayerInfo[] PurpleTeamPlayerInfo
        {
            get
            {
                return purpleTeamPlayerInfo;

            }
            set
            {
                purpleTeamPlayerInfo = value;
                NotifyPropertyChanged("PurpleTeamPlayerInfo");
            }
        }

        private PlayerDBDTO[] players;
        private PlayerDBDTO displayPlayer;
        public PlayerDBDTO DisplayPlayer
        {
            get
            {
                return displayPlayer;
            }
            set
            {
                displayPlayer = value;
                NotifyPropertyChanged("DisplayPlayer");
            }
        }

        private GameDBDTO gameInfo;
        public GameDBDTO GameInfo
        {
            get
            {
                return gameInfo;
            }
            set
            {
                gameInfo = value;
                NotifyPropertyChanged("GameInfo");
            }
        }


        private Boolean isBlueTeamWin = false;
        public Boolean IsBlueTeamWin
        {
            get
            {
                return isBlueTeamWin;
            }
            set
            {
                isBlueTeamWin = value;
                NotifyPropertyChanged("IsBlueTeamWin");
            }
        }

        private Boolean isPurpleTeamWin = false;
        public Boolean IsPurpleTeamWin
        {
            get
            {
                return isPurpleTeamWin;
            }
            set
            {
                isPurpleTeamWin = value;
                NotifyPropertyChanged("IsPurpleTeamWin");
            }
        }

        private UInt32[] buleTeamKDA;
        public UInt32[] BlueTeamKDA
        {
            get
            {
                return buleTeamKDA;
            }
            set
            {
                buleTeamKDA = value;
                NotifyPropertyChanged("BlueTeamKDA");
            }
        }

        private UInt32[] purpleTeamKDA;
        public UInt32[] PurpleTeamKDA
        {
            get
            {
                return purpleTeamKDA;
            }
            set
            {
                purpleTeamKDA = value;
                NotifyPropertyChanged("PurpleTeamKDA");
            }
        }


        private UInt32[] blueTeamWard;
        public UInt32[] BlueTeamWard
        {
            get
            {
                return blueTeamWard;
            }
            set
            {
                blueTeamWard = value;
                NotifyPropertyChanged("BlueTeamWard");
            }
        }


        private UInt32[] purpleTeamWard;
        public UInt32[] PurpleTeamWard
        {
            get
            {
                return purpleTeamWard;
            }
            set
            {
                purpleTeamWard = value;
                NotifyPropertyChanged("PurpleTeamWard");
            }
        }

        public void UpdateSummonerData()
        {
            NotifyPropertyChanged("BlueTeamPlayerInfo");
            NotifyPropertyChanged("PurpleTeamPlayerInfo");
        }

        public void GetPlayersFromDB()
        {

            players = GameDatabase.Instance.QueryPlayer(String.Format("GameId = {0} AND Platform = '{1}'", GameId, GamePlatform));
            BlueTeamKDA = new UInt32[3];
            PurpleTeamKDA = new UInt32[3];
            BlueTeamWard = new UInt32[2];
            PurpleTeamWard = new UInt32[2];
            foreach (PlayerDBDTO p in players)
            {

                if (p.Team == 100)
                {
                    BlueTeamKDA[0] += p.K;
                    BlueTeamKDA[1] += p.D;
                    BlueTeamKDA[2] += p.A;
                    BlueTeamWard[0] += p.WardPlaced;
                    BlueTeamWard[1] += p.WardKilled;
                }
                else if (p.Team == 200)
                {
                    PurpleTeamKDA[0] += p.K;
                    PurpleTeamKDA[1] += p.D;
                    PurpleTeamKDA[2] += p.A;
                    PurpleTeamWard[0] += p.WardPlaced;
                    PurpleTeamWard[1] += p.WardKilled;
                }

            }
            DisplayLocalPlayer();
        }

        public void DisplayLocalPlayer()
        {
            String playerName = Properties.Settings.Default.SummonerName;
            if (playerName == String.Empty)
            {
                var dbPlayerName = GameDatabase.Instance.QueryGamePlayer(GameId, GamePlatform);
                if (dbPlayerName != string.Empty)
                    playerName = dbPlayerName;
            }
            SelectPlayer(playerName);
        }

        public void SelectPlayer(String name)
        {
            if (players.Length > 0 && name.Length > 0)
            {
                var playerQuery = from p in players where p.Name == name select p;
                if (playerQuery.Count() > 0)
                {
                    DisplayPlayer = playerQuery.First();
                }
            }
        }

        public void GetGameFromDB()
        {
            if (gameInfo == null)
            {
                gameInfo = GameDatabase.Instance.QueryGame(String.Format("Id = {0} AND Platform = '{1}'", GameId, GamePlatform));
                if (gameInfo != null)
                {
                    if (gameInfo.WinTeam == 100)
                        IsBlueTeamWin = true;
                    else if (gameInfo.WinTeam == 200)
                        IsPurpleTeamWin = true;
                }
            }
            GetPlayersFromDB();
        }




        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                NotifyPropertyChanged("FileName");
                NotifyPropertyChanged("FileNameShort");
            }
        }

        public string FileNameShort
        {
            get
            {
                if (FileName == null)
                    return Utilities.GetString("RecordingGame");
                string[] nameSplit = FileName.Split(new char[] { '\\' });
                return nameSplit[nameSplit.Length - 1];
            }

        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));

        }

        public event MainWindow.RecordDelegate WatchClickEvent;
        public event MainWindow.RecordDelegate ExportClickEvent;
        public event MainWindow.RecordDelegate DeleteClickEvent;

        public void DeleteRecord()
        {
            DeleteClickEvent?.Invoke(this);
        }

        public void PlayRecord()
        {
            WatchClickEvent?.Invoke(this);
        }

        public void ExportRecord()
        {
            ExportClickEvent?.Invoke(this);
        }

        public void OpenMatchHistory()
        {
            try
            {
                var region = RiotAPI.Services.Request.RegionName.ContainsKey(GamePlatform) ? RiotAPI.Services.Request.RegionName[GamePlatform] : GamePlatform.ToLower();
                Process.Start($"http://matchhistory.{region}.leagueoflegends.com/en/#match-details/{GamePlatform}/{GameId}");
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog("開啟對戰紀錄網頁錯誤");
            }
        }

        public event MainWindow.RecordDelegate ShowMetaEvent;


        public void ShowMeta()
        {
            if (ShowMetaEvent != null)
                ShowMetaEvent(this);
        }


        public void ShowRenameDialog()
        {
            OneLineWindow rw = new OneLineWindow();
            rw.OneLineTextBox.Text = FileNameShort;
            rw.Owner = Application.Current.MainWindow;
            rw.ShowDialog();
            if (!rw.OkayClick)
                return;

            String name = rw.DesireString;


            if (name.Length > 3)
            {
                if (name.Substring(name.Length - 4).CompareTo(".lpr") != 0)
                    name = name + ".lpr";
            }
            else
                name = name + ".lpr";

            String dir = FileName.Remove(FileName.LastIndexOf('\\'));
            String targetPath = dir + '\\' + name;
            Rename(targetPath);
        }

        private void Rename(String targetPath)
        {
            try
            {
                File.Move(FileName, targetPath);
                FileName = targetPath;
            }
            catch (Exception)
            {

            }
        }

        public Boolean Search(String keyWords)
        {
            if (SearchInTeam(blueTeamPlayerInfo, keyWords))
                return true;
            else if (SearchInTeam(purpleTeamPlayerInfo, keyWords))
                return true;
            return false;
        }

        public Boolean SearchInTeam(PlayerInfo[] team, String keyWords)
        {
            foreach (PlayerInfo info in team)
            {
                if (ExistWordsInPlayerInfo(info, keyWords))
                {
                    return true;
                }
            }
            return false;
        }

        private Boolean ExistWordsInPlayerInfo(PlayerInfo info, String keyWords)
        {
            if (info.PlayerName.IndexOf(keyWords, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            if (info.ChampionName.IndexOf(keyWords, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            return false;
        }

    }



}
