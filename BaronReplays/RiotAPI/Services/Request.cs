using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BaronReplays.RiotAPI.Services
{
    public static class Request
    {
        private static String apiLanguage;
        public static String ApiLanguage
        {
            get
            {
                if (apiLanguage == null)
                    UpdateAPILanguage();
                return apiLanguage;
            }

            set { apiLanguage = value; }
        }

        public static Dictionary<String, String> EndPointsMap = new Dictionary<String, String>()
        {
            {"BR1","br.api.pvp.net"},
            {"EUN1","eune.api.pvp.net"},
            {"EUW1","euw.api.pvp.net"},
            {"KR","kr.api.pvp.net"},
            {"LA1","lan.api.pvp.net"},
            {"LA2","las.api.pvp.net"},
            {"NA1","na.api.pvp.net"},
            {"OC1","oce.api.pvp.net"},
            {"TR1","tr.api.pvp.net"},
            {"RU","ru.api.pvp.net"},
            {"PBE1","pbe.api.pvp.net"},
            {"JP1","jp.api.pvp.net"}
        };

        public static Dictionary<String, String> RegionName = new Dictionary<String, String>()
        {
            {"BR1","br"},
            {"EUN1","eune"},
            {"EUW1","euw"},
            {"KR","kr"},
            {"LA1","lan"},
            {"LA2","las"},
            {"NA1","na"},
            {"OC1","oce"},
            {"TR1","tr"},
            {"RU","ru"},
            {"PBE1","pbe"},
            {"JP1","jp"}
        };

        public static bool UpdateAPILanguage()
        {
            List<String> languages = GetStaticData("na/v1.2/languages?", typeof(List<String>));
            try
            {
                if (languages.Contains(Properties.Settings.Default.Language.Replace('-', '_')))
                {
                    apiLanguage = Properties.Settings.Default.Language.Replace('-', '_');
                }
                else
                {
                    String majorLang = Properties.Settings.Default.Language.Split(new char[] { '-' })[0];
                    majorLang = majorLang + "_" + majorLang.ToUpper();
                    if (languages.Contains(majorLang))
                    {
                        apiLanguage = majorLang;
                    }
                    else
                        ApiLanguage = "en_US";
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog("UpdateAPILanguage failed: " + e.Message);
                return false;
            }
            return true;
        }

        public static dynamic GetStaticData(String route, Type responseClass, String writeFilePath = "")
        {
            return GetData("NA1", "api/lol/static-data/" + route, responseClass, writeFilePath);
        }

        public static dynamic GetData(String platformId, String route, Type responseClass, String writeFilePath = "")
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                throw new NetworkException("未連接至網際網路"); 
            }

            String endpointAddress = null;
            if (!EndPointsMap.ContainsKey(platformId.ToUpperInvariant()))
            {
                throw new UnsupportedException(String.Format("不支援的平台: {0}", platformId.ToUpperInvariant()));
            }
            else
            {
                endpointAddress = EndPointsMap[platformId.ToUpperInvariant()];
            }

            dynamic result = null;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Accept-Charset", "utf-8");
                    wc.Headers.Add("Api-Host", endpointAddress);
                    wc.Encoding = Encoding.UTF8;
                    String fullRequest = String.Format("{0}{1}", BaronReplays.Constants.RiotApiRelayHost , route);
                    string content = Encoding.UTF8.GetString(wc.DownloadData(fullRequest));
                    if (writeFilePath != String.Empty)
                    {
                        File.WriteAllText(writeFilePath, content);
                    }
                    result = JsonConvert.DeserializeObject(content, responseClass);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("RiotAPI.Services.Request.GetData route: {0} Platform: {1} Message: {2}", route, platformId, e.Message));
            }
            return result;
        }

        public static dynamic ReadFromFile(String filePath, Type responseClass)
        {
            dynamic result = null;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string content = File.ReadAllText(filePath);
                    result = JsonConvert.DeserializeObject(content, responseClass);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(string.Format("{0}: {1}", System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName, e.Message));
            }
            return result;
        }

    }
}
