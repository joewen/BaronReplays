using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class ItemDto
    {
        public String colloq { get; set; }
        public Boolean consumeOnFull { get; set; }
        public Boolean consumed { get; set; }
        public Int32 depth { get; set; }
        public String description { get; set; }
        public Dictionary<String, String> effect { get; set; }
        public List<String> from { get; set; }
        public GoldDto gold { get; set; }
        public String group { get; set; }
        public Boolean hideFromAll { get; set; }
        public Int32 id { get; set; }
        public ImageDto image { get; set; }
        public Boolean inStore { get; set; }
        public List<String> into { get; set; }
        public Dictionary<String, Boolean> maps { get; set; }
        public String name { get; set; }
        public String plaintext { get; set; }
        public String requiredChampion { get; set; }
        public MetaDataDto rune { get; set; }
        public String sanitizedDescription { get; set; }
        public Int32 specialRecipe { get; set; }
        public Int32 stacks { get; set; }
        public BasicDataStatsDto stats { get; set; }
        public List<String> tags { get; set; }
    }
}
