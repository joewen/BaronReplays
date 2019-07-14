using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public class PlayerInfo
    {

        public PlayerInfo(string pName, string cName, UInt32 team, int cId)
        {
            this.playerName = pName;
            this.championName = cName;
            this.team = team;
            this.clientID = cId;
        }

        public PlayerInfo()
        {
        }
        
        public String playerName;
        public String PlayerName
        {
            get
            {
                return playerName;
            }
        }
        public String championName;
        public String ChampionName
        {
            get
            {
                return championName;
            }
        }
        public UInt32 team;
        public UInt32 Team
        {
            get
            {
                return team;
            }
        
        }

        public int clientID;
        public int ClientID
        {
            get
            {
                return clientID;
            }
        }
    }
}
