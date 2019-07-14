using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI.Services
{
    public class CurrentGame
    {
        public static CurrentGameInfo GetCurrentGameBySummonerId(long id, string platform)
        {
            Logger.Instance.WriteLog(string.Format("Get current game for summoner id {0} in {1}", id, platform));
            CurrentGameInfo result = Request.GetData(platform, String.Format("observer-mode/rest/consumer/getSpectatorGameInfo/{0}/{1}?", platform.ToUpperInvariant(), id), typeof(CurrentGameInfo));
            return result;
        }

        public static CurrentGameInfo GetCurrentGameBySummonerName(string name, string platform)
        {
            CurrentGameInfo result;
            try
            {
                SummonerDto summoners = Summoner.GetSummonerByName(name, platform);
                result = Request.GetData(platform, String.Format("observer-mode/rest/consumer/getSpectatorGameInfo/{0}/{1}?", platform.ToUpperInvariant(), summoners.id), typeof(CurrentGameInfo));
            }
            catch(Exception)
            {
                return null;
            }
            return result;
        }
    }
}
