using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class SummonerSpellDto
    {
        public List<Double> cooldown { get; set; }
        public String cooldownBurn { get; set; }
        public List<Int32> cost { get; set; }
        public String costBurn { get; set; }
        public String costType { get; set; }
        public String description { get; set; }
        public List<Double> effect { get; set; }
        public List<String> effectBurn { get; set; }
        public Int32 id { get; set; }
        public ImageDto image { get; set; }
        public String key { get; set; }
        public LevelTipDto leveltip { get; set; }
        public Int32 maxrank { get; set; }
        public List<String> modes { get; set; }
        public String name { get; set; }
        //public Object range API not chear
        public String rangeBurn { get; set; }
        public String resource { get; set; }
        public String sanitizedDescription { get; set; }
        public String sanitizedTooltip { get; set; }
        public Int32 summonerLevel { get; set; }
        public String tooltip { get; set; }
        public List<SpellVarsDto> vars { get; set; }
    } 
}
