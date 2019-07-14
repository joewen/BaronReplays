using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    public class ItemListDto
    {
        public BasicDataDto basic { get; set; }
        public Dictionary<String, ItemDto> data { get; set; }
        public List<GroupDto> groups { get; set; }
        public List<ItemTreeDto> tree { get; set; }
        public String type { get; set; }
        public String version { get; set; }
    }
}
