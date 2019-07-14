using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Threading;

namespace BaronReplays
{
    class LoLClientWatcher
    {
        private static LoLClientWatcher instance = new LoLClientWatcher();
        public LoLClientData ExecutionData;
        private Process process;
        public Process Process
        {
            get
            {
                return process;
            }
        }

        private static Dictionary<string, string> hostPlatformMap = new Dictionary<string, string>()
        {
            {"prodtw.lol.garenanow.com", "TW"},
            {"prod.lol.garenanow.com", "SG"},
            {"prodid.lol.garenanow.com", "ID1"},
            {"prodvn1.lol.garenanow.com", "VN"},
            {"prodph.lol.garenanow.com", "PH"},
            {"prodth.lol.garenanow.com", "TH"},
        };

        private static Dictionary<string, string> loginQueuePlatformMap = new Dictionary<string, string>()
        {
            {"https://loginqueuetw.lol.garenanow.com", "TW"},
            {"https://lq.lol.garenanow.com", "SG"},
            {"https://lqid.lol.garenanow.com", "ID1"},
            {"https://lqvn1.lol.garenanow.com", "VN"},
            {"https://lqph.lol.garenanow.com", "PH"},
            {"https://lqth.lol.garenanow.com", "TH"},
        };

        private Timer watcher;

        public Boolean IsLoLExists()
        {
            return (process != null);
        }

        private String FindPlatformIdByFile()
        {
            char[] sep = new char[] { '=' };
            using (StreamReader sr = new StreamReader(BaronReplays.Helper.FileHelper.GetFileLocation(ExecutionData.ExecutablePath) + Constants.LoLClientPropertiesName))
            {
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    String[] tokens = line.Split(sep);
                    if (tokens.Length > 1)
                    {
                        if (tokens[0].CompareTo("platformId") == 0)
                        {
                            String platform = tokens[1].ToUpper();
                            return platform;
                        }
                        else if (tokens[0].CompareTo("host") == 0)
                        {
                            if (hostPlatformMap.ContainsKey(tokens[1]))
                                return hostPlatformMap[tokens[1]];
                        }
                        else if (tokens[0].CompareTo("lq_uri") == 0)
                        {
                            if (loginQueuePlatformMap.ContainsKey(tokens[1]))
                                return loginQueuePlatformMap[tokens[1]];
                        }
                    }
                }
                sr.Close();
            }
            return null;
        }

        private String FindPlatformId()
        {
            String platform = FindPlatformIdByFile();
            if (platform != null)
                Properties.Settings.Default.Platform = platform;
            return platform;
        }


        private void Update()
        {
            Process pastLol = process;
            process = Utilities.GetProcess(Constants.LoLClientName);
            if (pastLol == null && process != null)
            {
                ExecutionData = new LoLClientData();
                ExecutionData.ExecutablePath = Utilities.GetExecutablePath(Constants.LoLClientName);
                ExecutionData.CommandLine = Utilities.GetCommandLine(process.Id);
                ExecutionData.Platform = FindPlatformId();
            }
            else if (pastLol != null && process == null)
            {
                ExecutionData = null;
            }
            else if (pastLol != null && process != null)
            {
                //exists awhile
            }
        }

        public void StartWatcher()
        {
            watcher = new Timer();
            watcher.Interval = 5000;
            watcher.Elapsed += WatcherTick;
            watcher.Start();
        }

        private LoLClientWatcher()
        {

        }

        private void WatcherTick(object sender, EventArgs e)
        {
            Update();
        }

        public static LoLClientWatcher Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
