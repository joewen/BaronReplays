using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaronReplays.JsonData;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace BaronReplays
{
    public class FeaturedGameJson : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));

        }

        public long gameId;
        public int mapId;
        public int MapId
        {
            get
            {
                return mapId;
            }
        }

        public string gameMode;
        public string GameMode
        {
            get
            {
                return gameMode;
            }
        }

        public string gameType;
        public string GameType
        {
            get
            {
                return gameType;
            }
        }

        public int gameQueueConfigId;
        public string GameQueue
        {
            get
            {
                return Utilities.GetString(RiotAPI.Constants.QueueType[gameQueueConfigId]);
            }
        }


        public FeaturedGameParticipant[] participants;

        private FeaturedGameParticipant[] _purpleTeamParticipants;
        public FeaturedGameParticipant[] PurpleTeamParticipants
        {
            get
            {

                if (_purpleTeamParticipants == null)
                {
                    CreateTeamArray();
                }
                return _purpleTeamParticipants;
            }
        }

        private FeaturedGameParticipant[] _blueTeamParticipants;
        public FeaturedGameParticipant[] BlueTeamParticipants
        {
            get
            {

                if (_blueTeamParticipants == null)
                {
                    CreateTeamArray();
                }
                return _blueTeamParticipants;
            }
        }

        private void CreateTeamArray()
        {
            //Team有可能沒滿五人..所以要用List再轉Array
            List<FeaturedGameParticipant> bluePlayer = new List<FeaturedGameParticipant>();
            List<FeaturedGameParticipant> purplePlayer = new List<FeaturedGameParticipant>();

            foreach (FeaturedGameParticipant f in participants)
            {
                if (f.TeamId == 100)
                    bluePlayer.Add( f);
                else
                    purplePlayer.Add(f);
            }
            _blueTeamParticipants = bluePlayer.ToArray();
            _purpleTeamParticipants = purplePlayer.ToArray();
        }

        public JContainer observers;

        public FeaturedGameBanned[] bannedChampions;
        public FeaturedGameBanned[] BannedChampions
        {
            get
            {
                if (BlueTeamBannedChampions == null)
                {
                    CreateBannedArray();
                }
                return bannedChampions;
            }
            set
            {
                bannedChampions = value;
            }
        }

        public void CreateBannedArray()
        {
            IEnumerable<FeaturedGameBanned> blueBan = from banned in bannedChampions
                                                      where banned.teamId == 100
                                                      select banned;
            BlueTeamBannedChampions = blueBan.ToArray();
            IEnumerable<FeaturedGameBanned> purpleBan = from banned in bannedChampions
                                                        where banned.teamId == 200
                                                        select banned;
            PurpleTeamBannedChampions = purpleBan.ToArray();
        }

        public FeaturedGameBanned[] BlueTeamBannedChampions
        {
            get;
            set;
        }

        public FeaturedGameBanned[] PurpleTeamBannedChampions
        {
            get;
            set;
        }


        public string platformId;
        public string PlatformId
        {
            get
            {
                return platformId;
            }
        }
        public int gameTypeConfigId;

        public UInt64 gameStartTime;
        public UInt32 gameLength;
        public UInt32 GameLength
        {
            get
            {
                DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                time = time.AddMilliseconds(gameStartTime);

                return (UInt32)(DateTime.UtcNow - time).TotalSeconds - 180;//扣掉延遲180秒鐘
            }
            set
            {
                gameLength = value;
                NotifyPropertyChanged("GameLength");
            }
        }
    }
}
