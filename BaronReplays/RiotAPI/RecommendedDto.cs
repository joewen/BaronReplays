using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class RecommendedDto
    {
        public List<BlockDto> blocks { get; set; }
        public String champion { get; set; }
        public String map { get; set; }
        public String mode { get; set; }
        public Boolean priority { get; set; }
        public String title { get; set; }
        public String type { get; set; }
    }
}
