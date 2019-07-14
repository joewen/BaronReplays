using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace BaronReplays.BRapi.Service
{
    class GarenaService
    {
        public static BRapi.Model.GameMetaData GetGameNyName(string summonerName, string platform)
        {
            BRapi.Model.GameMetaData gi = null;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = Constants.BRapiPrefix + string.Format("live/byName/{0}/{1}", platform, summonerName);
                    gi = Newtonsoft.Json.JsonConvert.DeserializeObject(wc.DownloadString(new Uri(url)), typeof(BRapi.Model.GameMetaData)) as BRapi.Model.GameMetaData;
                }

            }
            catch (Exception)
            {
            }
            return gi;
        }

        public static BRapi.Model.GameMetaData GetGameBySummonerId(int summonerId, string platform)
        {
            BRapi.Model.GameMetaData gi = null;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = Constants.BRapiPrefix + string.Format("live/bySummonerId/{0}/{1}", platform, summonerId);
                    gi = Newtonsoft.Json.JsonConvert.DeserializeObject(wc.DownloadString(new Uri(url)), typeof(BRapi.Model.GameMetaData)) as BRapi.Model.GameMetaData;
                }

            }
            catch (Exception)
            {
            }
            return gi;
        }
    }
}
