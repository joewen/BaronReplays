using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class CurrentGameInfo
    {
        public List<BannedChampion> bannedChampions { get; set; }
        public long gameId { get; set; }
        public long gameLength { get; set; }
        public string gameMode { get; set; }
        public long gameQueueConfigId { get; set; }
        public long gameStartTime { get; set; }
        public string gameType { get; set; }
        public long mapId { get; set; }
        public Observer observers { get; set; }
        public List<Participant> participants { get; set; }
        public string platformId { get; set; } 
    }
}
