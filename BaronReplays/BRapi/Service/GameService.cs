using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace BaronReplays.BRapi.Service
{
    class GameService
    {
        public static void SendEndOfGameStats(string platform, string jsonEndOfGameStats, Int64 gameId)
        {
            using (WebClient  wc = new WebClient() )
            {
                try
                {
                    wc.UploadString(Constants.BRapiPrefix + string.Format("game/end/{0}/{1}", platform, gameId), jsonEndOfGameStats);
                }
                catch(Exception)
                {
                
                }
            }
        }
    }
}
