using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluorineFx;
using FluorineFx.AMF3;
using FluorineFx.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace BaronReplays
{
    public class EndOfGameStats : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private byte[] _rawData;
        private Boolean _broken;
        public Boolean Broken
        {
            get
            {
                return _broken;
            }
        }

        private Boolean _ranked;
        public Boolean Ranked
        {
            get
            {
                DecodeData();
                return _ranked;
            }
            set
            {
                _ranked = value;
                NotifyPropertyChanged("Ranked");
            }
        }

        private String _gameType;
        public String GameType
        {
            get
            {
                DecodeData();
                return _gameType;
            }
            set
            {
                _gameType = value;
                NotifyPropertyChanged("GameType");
            }
        }

        private UInt64 _gameId;
        public UInt64 GameId
        {
            get
            {
                DecodeData();
                return _gameId;
            }
            set
            {
                _gameId = value;
                NotifyPropertyChanged("GameId");
            }

        }

        private UInt32 _gameLength;
        public UInt32 GameLength
        {
            get
            {
                DecodeData();
                return _gameLength;
            }
            set
            {
                _gameLength = value;
                NotifyPropertyChanged("GameLength");
            }

        }

        private String _gameMode;
        public String GameMode
        {
            get
            {
                DecodeData();
                return _gameMode;
            }
            set
            {
                _gameMode = value;
                NotifyPropertyChanged("GameMode");
            }
        }

        private List<PlayerStats> _players;
        public List<PlayerStats> Players
        {
            get
            {
                DecodeData();
                return _players;
            }
            set
            {
                _players = value;
                NotifyPropertyChanged("Players");
            }
        }

        public List<PlayerStats> BlueTeamPlayers
        {
            get
            {
                return _players.GetRange(0, BlueTeamPlayerCount);
            }
        }

        public List<PlayerStats> PurpleTeamPlayers
        {
            get
            {
                return _players.GetRange(BlueTeamPlayerCount, _players.Count - BlueTeamPlayerCount);
            }
        }

        public String BlueTeamKDA
        {
            get
            {
                DecodeData();
                UInt32 K = 0, D = 0, A = 0;
                foreach (PlayerStats ps in Players)
                {
                    if (ps.TeamId == 100)
                    {
                        K += ps.Statistics.K;
                        D += ps.Statistics.D;
                        A += ps.Statistics.A;
                    }
                }
                return String.Format("{0,-3} / {1,-3} / {2,-3}", K, D, A);
            }
        }

        public UInt32 WonTeam
        {
            get
            {
                if (Players.Count == 0)
                    return 0;

                foreach (PlayerStats ps in Players)
                {
                    if (ps.Statistics.Win)
                    {
                        return ps.TeamId;
                    }
                }
                return 0;
            }
        }

        public String WonTeamStr
        {
            get
            {
                if (WonTeam == 0)
                    return Utilities.GetString("NoResult") as String;
                if (WonTeam == 100)
                {
                    return Utilities.GetString("BlueTeamWon") as String;
                }
                return Utilities.GetString("PurpleTeamWon") as String;
            }
        }

        public String PurpleTeamKDA
        {
            get
            {
                DecodeData();
                UInt32 K = 0, D = 0, A = 0;
                foreach (PlayerStats ps in Players)
                {
                    if (ps.TeamId == 200)
                    {
                        K += ps.Statistics.K;
                        D += ps.Statistics.D;
                        A += ps.Statistics.A;
                    }
                }
                return String.Format("{0,-3} / {1,-3} / {2,-3}", K, D, A);
            }
        }

        private String _blueTeamInfo;
        public String BlueTeamInfo
        {
            get
            {
                DecodeData();
                return _blueTeamInfo;
            }
            set
            {
                _blueTeamInfo = value;
                NotifyPropertyChanged("BlueTeamInfo");
            }
        }

        private String _purpleTeamInfo;
        public String PurpleTeamInfo
        {
            get
            {
                DecodeData();
                return _purpleTeamInfo;
            }
            set
            {
                _purpleTeamInfo = value;
                NotifyPropertyChanged("PurpleTeamInfo");
            }
        }

        private UInt32[] blueTeamWard;
        public UInt32[] BlueTeamWard
        {
            get
            {
                DecodeData();
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
                DecodeData();
                return purpleTeamWard;
            }
            set
            {
                purpleTeamWard = value;
                NotifyPropertyChanged("PurpleTeamWard");
            }
        }

        private UInt32 blueTeamTurret;
        public UInt32 BlueTeamTurret
        {
            get
            {
                DecodeData();
                return blueTeamTurret;
            }
            set
            {
                blueTeamTurret = value;
                NotifyPropertyChanged("BlueTeamTurret");
            }
        }

        private UInt32 purpleTeamTurret;
        public UInt32 PurpleTeamTurret
        {
            get
            {
                DecodeData();
                return purpleTeamTurret;
            }
            set
            {
                purpleTeamTurret = value;
                NotifyPropertyChanged("PurpleTeamTurret");
            }
        }




        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));

        }


        public Int32 BlueTeamPlayerCount
        {
            get;
            set;
        }

        private ASObject originalAMFObject;
        public ASObject OriginalAMFObject
        {
            get
            {
                return originalAMFObject;
            }

        }
    

        private bool _decoded = false;
        public Boolean DecodeData()
        {

            if (_decoded)
                return !Broken;
            _decoded = true;
            Stream input = new MemoryStream(_rawData);

            using (var amf = new AMFReader(input))
            {
                try
                {
                    originalAMFObject = (ASObject)amf.ReadAMF3Data();
                    Ranked = (Boolean)originalAMFObject["ranked"];
                    GameType = originalAMFObject["gameType"] as String;
                    GameLength = UInt32.Parse(originalAMFObject["gameLength"].ToString());
                    GameMode = originalAMFObject["gameMode"] as String;
                    GameId = UInt64.Parse(originalAMFObject["gameId"].ToString());

                    ArrayCollection blueTeam = originalAMFObject["teamPlayerParticipantStats"] as ArrayCollection;
                    ArrayCollection purpleTeam = originalAMFObject["otherTeamPlayerParticipantStats"] as ArrayCollection;
                    BlueTeamPlayerCount = blueTeam.Count;
                    int playerCount = blueTeam.Count + purpleTeam.Count;
                    Players = new List<PlayerStats>();
                    for (int i = 0; i < blueTeam.Count; i++)
                    {
                        Players.Add(new PlayerStats(blueTeam[i] as ASObject));
                    }

                    for (int i = 0; i < purpleTeam.Count; i++)
                    {
                        Players.Add(new PlayerStats(purpleTeam[i] as ASObject));
                    }

                    if (OriginalAMFObject["myTeamInfo"] != null)
                    {
                        ASObject teamInfo = originalAMFObject["myTeamInfo"] as ASObject;
                        BlueTeamInfo = String.Format("[{0}]{1}", teamInfo["tag"], teamInfo["name"]);
                        teamInfo = originalAMFObject["otherTeamInfo"] as ASObject;
                        PurpleTeamInfo = String.Format("[{0}]{1}", teamInfo["tag"], teamInfo["name"]);
                    }
                    InitWardAndTurretInfo();
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteLog("EndOfGameStatas decode failed");
                    Logger.Instance.WriteLog(e.Message);
                    _broken = true;
                    return false;
                }
            }
            return true;
        }

        private void InitWardAndTurretInfo()
        {
            try
            {
                BlueTeamWard = new UInt32[2];
                PurpleTeamWard = new UInt32[2];
                BlueTeamTurret = 0;
                PurpleTeamTurret = 0;
                foreach (PlayerStats ps in Players)
                {
                    UInt32[] targetTeamWard = ps.TeamId == 100 ? BlueTeamWard : PurpleTeamWard;
                    targetTeamWard[0] += ps.Statistics.WardPlaced;
                    targetTeamWard[1] += ps.Statistics.WardKilled;
                    UInt32 turretCount = ps.Statistics.TurretsKilled;
                    if (ps.TeamId == 100)
                        BlueTeamTurret = BlueTeamTurret + turretCount;
                    else
                        PurpleTeamTurret = PurpleTeamTurret + turretCount;
                }
            }
            catch (Exception)
            { 
            
            }
        }

        public EndOfGameStats()
        {
            _broken = false;
        }

        public EndOfGameStats(byte[] statBytes)
        {
            _broken = false;
            _rawData = statBytes;

        }
    }
}
