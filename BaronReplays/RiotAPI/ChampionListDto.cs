using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class ChampionListDto
    {
        public Dictionary<String, ChampionDto> data { get; set; }
        public String format { get; set; }
        public Dictionary<String, String> keys { get; set; }
        public String type { get; set; }
        public String version { get; set; }
    }
}
