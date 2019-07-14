using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace BaronReplays.Database
{
    public class SummonerDBDTO : DBDTO
    {
        public Int64 Id { get; set; }
        public String Name { get; set; }
        public String Platform { get; set; }

        public SummonerDBDTO(SQLiteDataReader reader)
            :base(reader)
        {

        }
    }
}
