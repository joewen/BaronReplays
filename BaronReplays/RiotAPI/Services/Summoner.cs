using BaronReplays.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaronReplays.RiotAPI.Services
{
    public class Summoner
    {
        public static SummonerDto GetSummonerByName(string name, string platform)
        {
            SummonerDto summoner = null;
            try
            {
                string encodedName = System.Web.HttpUtility.UrlPathEncode(name);
                Dictionary<String, SummonerDto> result = Request.GetData(platform, String.Format("api/lol/{0}/v1.4/summoner/by-name/{1}?", Request.RegionName[platform], encodedName), typeof(Dictionary<String, SummonerDto>));
                summoner = result.Values.First();
                BaronReplays.Database.PublicDatabaseManager.Instance.AddSummonerId(summoner.id, summoner.name, platform);
                Logger.Instance.WriteLog(String.Format("Summoner Id of {0} in {1} is {2}", name, platform, summoner.id));
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("GetSummonerByName failed: {0}", e.Message));
            }

            return summoner;
        }


    }
}
