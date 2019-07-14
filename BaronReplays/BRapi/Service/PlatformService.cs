using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace BaronReplays.BRapi.Service
{
    class PlatformService
    {
        public static void NewPlatform(string platform, string address)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = Constants.BRapiPrefix + string.Format("platform/new/{0}/{1}" ,address, platform);
                    wc.DownloadStringAsync(new Uri(url));
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
