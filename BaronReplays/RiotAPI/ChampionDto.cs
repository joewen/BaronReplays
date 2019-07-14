using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class ChampionDto
    {
        public List<String> allytips { get; set; }
        public String blurb { get; set; }
        public List<String> enemytips { get; set; }
        public int id { get; set; }
        public ImageDto image { get; set; }
        public InfoDto info { get; set; }
        public String key { get; set; }
        public String lore { get; set; }
        public String name { get; set; }
        public String partype { get; set; }
        public PassiveDto passive { get; set; }
        public List<RecommendedDto> recommended { get; set; }
        public List<SkinDto> skins { get; set; }
        public List<ChampionSpellDto> spells { get; set; }
        public StatsDto stats { get; set; }
        public List<String> tags { get; set; }
        public String title{ get; set; }
    }
}
