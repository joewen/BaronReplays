using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BaronReplays.Database
{
    public class PlayerDBDTO: DBDTO
    {
        public String Name { get; set; }
        public Int64 GameId { get; set; }
        public String Platform { get; set; }
        public String Champion { get; set; }
        public UInt16 Team { get; set; }
        public UInt16 Level { get; set; }
        public UInt16 K { get; set; }
        public UInt16 D { get; set; }
        public UInt16 A { get; set; }
        public UInt16 Spell1 { get; set; }
        public UInt16 Spell2 { get; set; }
        public UInt16 Item0 { get; set; }
        public UInt16 Item1 { get; set; }
        public UInt16 Item2 { get; set; }
        public UInt16 Item3 { get; set; }
        public UInt16 Item4 { get; set; }
        public UInt16 Item5 { get; set; }
        public UInt16 Item6 { get; set; }
        public UInt32 Gold { get; set; }
        public UInt32 Minions { get; set; }
        public UInt32 WardPlaced { get; set; }
        public UInt32 WardKilled { get; set; }

        public PlayerDBDTO(SQLiteDataReader reader)
            :base(reader)
        {

        }
    }
}
