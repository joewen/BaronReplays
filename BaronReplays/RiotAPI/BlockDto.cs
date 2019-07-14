using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class BlockDto
    {
        public List<BlockItemDto> items { get; set; }
        public Boolean recMath { get; set; }
        public String type { get; set; }
    }
}
