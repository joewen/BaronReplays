using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class PassiveDto
    {
        public String description { get; set; }
        public ImageDto image { get; set; }
        public String name { get; set; }
        public String sanitizedDescription { get; set; }
    }
}
