using BaronReplays.LoLStaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BaronReplays.JsonData
{
    public class FeaturedGameParticipant:PlayerStats
    {
        public UInt32 teamId
        {
            get
            {
                return _teamId;
            }
            set
            {
                _teamId = value;
            }
        }

        public Int32 spell1Id
        {
            get
            {
                return _spell1Id;
            }
            set
            {
                _spell1Id = value;
            }
        }

        public Int32 spell2Id
        {
            get
            {
                return _spell2Id;
            }
            set
            {
                _spell2Id = value;
            }
        }

        protected Int32 _championId;
        public Int32 championId
        {
            get
            {
                return _championId;
            }
            set
            {
                _championId = value;
            }
        }

        protected Int32 _skinIndex;
        public Int32 skinIndex
        {
            get
            {
                return _skinIndex;
            }
            set
            {
                _skinIndex = value;
            }
        }

        protected Int32 _profileIconId;
        public Int32 profileIconId
        {
            get
            {
                return _profileIconId;
            }
            set
            {
                _profileIconId = value;
            }
        }

        public String summonerName
        {
            get
            {
                return _summonerName;
            }
            set
            {
                _summonerName = value;
            }
        }

        public Boolean bot
        {
            get
            {
                return _botPlayer;
            }
            set
            {
                _botPlayer = value;
            }
        }

        public String QFSearchString
        {
            get 
            {
                return String.Format(Utilities.GetString("QFsearch") as String,SummonerName);
            }
        }

        public PlayerInfo ToPlayerInfo()
        {
            PlayerInfo info = new PlayerInfo();
            info.championName = Champion.Instance.GetKeyById(championId);
            info.playerName = SummonerName;
            info.team = TeamId;
            return info;
        }
    }
}
