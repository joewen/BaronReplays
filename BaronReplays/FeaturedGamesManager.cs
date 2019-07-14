using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Threading;

namespace BaronReplays
{
    public class FeaturedGamesManager
    {
        public static List<String> featuredPlatform = new List<String>() 
        {
            "TW","NA1","EUW1","EUN1", "JP1","BRLA","TRRU","SG","KR","OC1","VN","PH","ID1","TH", "PBE1"
        };

        public static List<String> FeaturedPlatform
        {
            get
            {
                return featuredPlatform;
            }
        }

        private String platform;

        private Boolean isLastTimeRefreshSuccess;
        private DispatcherTimer refreshTimer;

        private WebClient httpClient;
        public Boolean IsBusy
        {
            get
            {
                return httpClient.IsBusy;
            }
        }
        public IFeaturedGamesQuery CallbackInstance;

        public Boolean IsLastTimeRefreshSuccess
        {
            get
            {
                return isLastTimeRefreshSuccess;
            }
        }

        private FeaturedGameJson[] games;
        public FeaturedGameJson[] Games
        {
            get
            {
                return games;
            }
        }

        public String Platform
        {
            get
            {
                return platform;
            }
            set
            {
                platform = value;
                RenewAsync();
            }
        }

        public TimeSpan RefreshInterval
        {
            get
            {
                if (refreshTimer == null)
                    return TimeSpan.Zero;
                return refreshTimer.Interval;
            }
            set
            {
                if (refreshTimer == null)
                    createRefreshTimer(value);
                else
                {
                    if (value == TimeSpan.Zero)
                        refreshTimer.Stop();
                    else
                        refreshTimer.Interval = value;
                }
            }
        }

        private void createRefreshTimer(TimeSpan t)
        {
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = t;
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RenewAsync();
        }

        public FeaturedGamesManager(String platformId, IFeaturedGamesQuery callback = null)
        {
            isLastTimeRefreshSuccess = false;
            CallbackInstance = callback;
            platform = platformId;
            InitHttpClient();
            RenewAsync();
        }


        private void HttpClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                games = DecodeData(e.Result);
                isLastTimeRefreshSuccess = true;
            }
            else
            {
                isLastTimeRefreshSuccess = false;
            }
            CallDoneCallback();
        }

        private void InitHttpClient()
        {
            httpClient = new WebClient();
            httpClient.DownloadDataCompleted += HttpClient_DownloadDataCompleted;
        }


        private void CallDoneCallback()
        {
            if (CallbackInstance != null)
                CallbackInstance.RefreshDone(isLastTimeRefreshSuccess);
        }

        public void CancelAsync()
        {
            httpClient.CancelAsync();
        }

        public void RenewAsync()
        {
            isLastTimeRefreshSuccess = false;
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return;
            }

            try
            {
                String compAddr = String.Format(@"http://{0}/observer-mode/rest/featured", Utilities.LoLObserveServersIpMapping[platform]);
                httpClient.DownloadDataAsync(new Uri(compAddr));
            }
            catch (Exception e)
            {

            }

        }

        public void Renew()
        {
            isLastTimeRefreshSuccess = false;
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return;
            }
            try
            {
                String compAddr = String.Format(@"http://{0}/observer-mode/rest/featured", Utilities.LoLObserveServersIpMapping[platform]);
                byte[] content = httpClient.DownloadData(compAddr);
                games = DecodeData(content);
                isLastTimeRefreshSuccess = true;
            }
            catch (Exception e)
            {

            }
        }

        private FeaturedGameJson[] DecodeData(byte[] data)
        {
            string najs = Encoding.UTF8.GetString(data);
            JObject j = JsonConvert.DeserializeObject<JObject>(najs);
            return JsonConvert.DeserializeObject<FeaturedGameJson[]>(j["gameList"].ToString());
        }

    }
}
