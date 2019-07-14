using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx;
using FluorineFx.AMF3;

namespace BaronReplays
{
    public class PlayerStats
    {
        protected String _skinName;
        public String SkinName
        {
            get
            {
                return _skinName;
            }
            set
            {
                _skinName = value;
            }
        }

        protected Boolean _botPlayer;
        public Boolean BotPlayer
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

        protected UInt64 _userId;
        public UInt64 UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
            }
        }

        protected UInt64 _gameId;
        public UInt64 GameId
        {
            get
            {
                return _gameId;
            }
            set
            {
                _gameId = value;
            }
        }

        protected String _summonerName;
        public String SummonerName
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

        protected Boolean _leaver;
        public Boolean Leaver
        {
            get
            {
                return  _leaver;
            }
            set
            {
                _leaver = value;
            }
        }

        protected UInt32 _teamId;
        public UInt32 TeamId
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

        protected Int32 _spell1Id;
        public Int32 Spell1Id
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

        protected Int32 _spell2Id;
        public Int32 Spell2Id
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

        private DetailStats _statistics;
        public DetailStats Statistics
        {
            get
            {
                return _statistics;
            }
            set
            {
                _statistics = value;
            }
        }


        public PlayerStats()
        {

        }

        public PlayerStats(ASObject pStats)
        {
            SkinName =  pStats["skinName"] as String;
            BotPlayer = (Boolean)pStats["botPlayer"];
            UserId = UInt64.Parse(pStats["userId"].ToString());
            GameId = UInt64.Parse(pStats["gameId"].ToString());
            SummonerName = pStats["summonerName"].ToString();
            Leaver = (Boolean)pStats["leaver"];
            TeamId = UInt32.Parse(pStats["teamId"].ToString());
            Spell1Id = Int32.Parse(pStats["spell1Id"].ToString());
            Spell2Id = Int32.Parse(pStats["spell2Id"].ToString());
            _statistics = new DetailStats(pStats["statistics"] as ArrayCollection);
        }
    }
}
