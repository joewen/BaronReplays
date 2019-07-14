using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BaronReplays.BRapi.Service
{
    public class ReportService
    {
        enum ReportType
        {
            Crash,
            Normal
        }

        public static void ReportBug(string deviceId, string content, string msg)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.UploadData($"{Constants.BRapiPrefix}report/bug/{deviceId}", Encoding.UTF8.GetBytes(msg + Environment.NewLine + content));
                }
                catch (Exception)
                { }
            }
        }

        public static void ReportCrash(string deviceId, string content)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.UploadData($"{Constants.BRapiPrefix}report/crash/{deviceId}", Encoding.UTF8.GetBytes(content));
                }
                catch (Exception)
                { }
            }
        }

    }
}
