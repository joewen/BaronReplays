using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class RealmDto
    {
        public String cdn { get; set; }
        public String css { get; set; }
        public String dd { get; set; }
        public String l { get; set; }
        public String lg { get; set; }
        public Dictionary<String, String> n { get; set; }
        public int profileiconmax { get; set; }
        public String store { get; set; }
        public String v { get; set; }
    }
}
