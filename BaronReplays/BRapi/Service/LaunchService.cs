using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace BaronReplays.BRapi.Service
{
    class LaunchService
    {
        public static void Launch()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = Constants.BRapiPrefix + "launch/" + Utilities.Brid;
                    wc.DownloadDataAsync(new Uri(url));
                }
            }
            catch (Exception)
            { 
            }
        }
    }
}
