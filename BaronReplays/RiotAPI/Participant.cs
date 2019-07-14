using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class Participant
    {
        public bool bot { get; set; }
        public long championId { get; set; }
        public List<Mastery> mastery { get; set; }
        public long profileIconId { get; set; }
        public List<Rune> runes { get; set; }
        public long spell1Id { get; set; }
        public long spell2Id { get; set; }
        public long summonerId { get; set; }
        public string summonerName { get; set; }
        public long teamId { get; set; }
    }
}
