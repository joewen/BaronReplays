using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class SummonerSpellListDto
    {
        public Dictionary<String, SummonerSpellDto> data { get; set; }
        public String type { get; set; }
        public String version { get; set; }
    }
}
