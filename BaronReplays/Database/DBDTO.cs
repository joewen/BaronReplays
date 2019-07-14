using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BaronReplays.Database
{
    public abstract class DBDTO
    {
        public DBDTO(SQLiteDataReader reader)
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo info in properties)
            {
                var value = reader[info.Name];
                if (!(value is DBNull))
                {
                    info.SetValue(this, Convert.ChangeType(reader[info.Name], info.PropertyType));
                }
                else if (info.PropertyType == typeof(string))
                {
                    info.SetValue(this, string.Empty);
                }
            }
        }
    }
}
