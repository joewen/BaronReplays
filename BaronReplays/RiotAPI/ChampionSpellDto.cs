using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class ChampionSpellDto
    {
        public List<ImageDto> altimages { get; set; }
        public List<Double> cooldown { get; set; }
        public String cooldownBurn { get; set; }
        public List<Int32> cost { get; set; }
        public String costBurn { get; set; }
        public String costType { get; set; }
        public String description { get; set; }
        public List<List<Double>> effect { get; set; }
        public List<String> effectBurn { get; set; }
        public ImageDto image { get; set; }
        public String key { get; set; }
        public LevelTipDto leveltip { get; set; }
        public Int32 maxrank { get; set; }
        public String name { get; set; }
        //public Object range { get; set; } //API not clear
        public String rangeBurn { get; set; }
        public String resource { get; set; }
        public String sanitizedDescription { get; set; }
        public String sanitizedTooltip { get; set; }
        public String tooltip { get; set; }
        public List<SpellVarsDto> SpellVarsDto { get; set; }
    }
}
