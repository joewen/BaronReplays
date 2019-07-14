using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class BannedChampion
    {
        public long championId { get; set; }
        public int pickTurn { get; set; }
        public long teamId { get; set; }
    }
}
