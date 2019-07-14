using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace BaronReplays.BRapi.Service
{
    class RecordingService
    {
        public static void ReportSuccess(string platform)
        {
            ReportStatus("success", platform);
        }

        public static void ReportFail(string platform)
        {
            ReportStatus("fail", platform);
        }

        private static void ReportStatus(string status,string platform)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = Constants.BRapiPrefix + string.Format("recording_result/{0}/{1}",status, platform);
                    wc.DownloadStringAsync(new Uri(url));
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
