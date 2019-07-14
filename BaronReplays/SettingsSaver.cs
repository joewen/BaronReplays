using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public class SettingsSaver
    {
        public static void SaveSettings()
        {
            try
            {
                SettingsPropertyValueCollection collation = Properties.Settings.Default.PropertyValues;
                using (StreamWriter writer = new StreamWriter(Constants.SettingsFileName))
                {
                    foreach (SettingsPropertyValue property in collation)
                    {
                        writer.WriteLine(String.Format("{0}|{1}", property.Name, property.PropertyValue));
                    }
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("寫入設定發生錯誤 原因為: {0}", e.Message));
            }
        }

        public static void CheckSettings()
        {
            if (Properties.Settings.Default.DoRecover)
            {
                ReadSettings();
            }
        }

        public static void ReadSettings()
        {
            try
            {
                Char[] seperator = { '|' };
                SettingsPropertyValueCollection collection = Properties.Settings.Default.PropertyValues;
                if (File.Exists(Constants.SettingsFileName))
                {
                    using (StreamReader reader = new StreamReader(Constants.SettingsFileName))
                    {
                        while (!reader.EndOfStream)
                        {
                            String[] pair = reader.ReadLine().Split(seperator);
                            if (pair.Length > 1)
                            {
                                var setting = Properties.Settings.Default[pair[0]];
                                Properties.Settings.Default[pair[0]] = Convert.ChangeType(pair[1], setting.GetType());
                            }
                        }
                        reader.Close();
                    }
                }
                File.Delete(Constants.SettingsFileName);
                Properties.Settings.Default.DoRecover = false;

            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("讀取設定發生錯誤 原因為: {0}", e.Message));
                Properties.Settings.Default.Reset();
            }
            Properties.Settings.Default.Save();
        }
    }
}
