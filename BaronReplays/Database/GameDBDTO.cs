using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace BaronReplays.Database
{
    public class GameDBDTO:DBDTO
    {
        public Int64 Id { get; set; }
        public String Platform { get; set; }
        public String GameVersion { get; set; }
        public UInt32 Length { get; set; }
        public UInt32 Time { get; set; }
        public UInt16 WinTeam { get; set; }
        public String GameMode { get; set; }
        public String GameType { get; set; }


        public GameDBDTO(SQLiteDataReader reader)
            :base(reader)
        {

        }
    }
}
