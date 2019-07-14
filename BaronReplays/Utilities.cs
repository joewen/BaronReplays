using BaronReplays.RiotAPI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace BaronReplays
{
    public static class Utilities
    {
        public static Boolean StartupMinimized = false;
        public static Boolean HasNewVersion;
        public static Dictionary<int, String> IdToChampion;
        public static int Version
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return int.Parse($"{version.Major:00}{version.Revision:00}");
            }
        }
        public static String AppUri = @"pack://application:,,,/" + System.Reflection.Assembly.GetEntryAssembly().GetName().Name + ";component/";
        public static readonly List<Char> invaildFileNameChar = new List<char>(Path.GetInvalidFileNameChars());
        public static readonly string Brid;
        public static string BugReportString = string.Empty;

        static Utilities()
        {
            Brid = CalculateBrid();
        }

        static string CalculateBrid()
        {
            string macAddress = NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString();
            string brid = $"brid-{macAddress}-{Environment.MachineName}-{Environment.UserName}";
            var crypter = SHA256.Create();
            var hash = crypter.ComputeHash(Encoding.UTF8.GetBytes(brid));
            return Convert.ToBase64String(hash);
        }

        public static Dictionary<String, String> LoLObserveServersIpMapping = new Dictionary<String, String>()
        {
            { "TW","124.108.153.163:80" },
            { "NA1","spectator.na.lol.riotgames.com:80" },
            { "EUW1","spectator.euw1.lol.riotgames.com:80" },
            { "EUN1","spectator.eu.lol.riotgames.com:8088" },
            { "BR1","spectator.br.lol.riotgames.com:80" },
            { "LA1","spectator.la1.lol.riotgames.com:80" },
            { "LA2","spectator.la2.lol.riotgames.com:80" },
            { "BRLA","spectator.br.lol.riotgames.com:80" },
            {"TR1","spectator.tr.lol.riotgames.com:80"},
            {"RU","spectator.ru.lol.riotgames.com:80"},
            {"TRRU","spectator.tr.lol.riotgames.com:80"},
            {"PBE1","spectator.pbe1.lol.riotgames.com:8088"},
            {"SG","203.116.112.222:8088"},
            {"KR","spectator.kr.lol.riotgames.com:80"},
            {"OC1","spectator.oc1.lol.riotgames.com:80"},
            {"VN","210.211.119.15:80"},
            {"PH","125.5.6.152:8088"},
            {"ID1","103.248.58.26:80"},
            {"TH","112.121.157.15:8088"},
            {"TRSA","66.151.33.244:8088"},
            {"TRNA","216.52.241.149:8088"},
            {"TRTW","112.121.84.193:8088"},
            {"HN1_NEW","14.17.32.160:8088"},
            {"JP1","spectator.jp1.lol.riotgames.com:80" },
        };

        public static List<String> Regions = new List<String>()
        {
            "TW","NA1","EUW1","EUN1", "JP1" ,"BR1","LA1","LA2","TR1","RU","KR","OC1","SG",/*"VN","PH",*/"ID1"/*,"TH", "PBE1"*/
        };


        public static String GetSimpleVersionNumber(String s)
        {
            String[] token = s.Split('.');
            return String.Format("{0}.{1}", token[0], token[1]);
        }

        public static int CompareVersion(String v1, String v2)
        {
            String[] v1tok = v1.Split(new char[] { '.' }, 2);
            String[] v2tok = v2.Split(new char[] { '.' }, 2);
            if (UInt32.Parse(v1tok[0]) > UInt32.Parse(v2tok[0]))
                return 1;
            else if (UInt32.Parse(v1tok[0]) < UInt32.Parse(v2tok[0]))
                return -1;
            else
            {
                int i1 = v1.IndexOf('.');
                int i2 = v2.IndexOf('.');
                if (i1 == 0 || i2 == 0)
                    return 0;
                return CompareVersion(v1.Substring(i1 + 1), v2.Substring(i2 + 1));
            }
        }


        public static Process GetProcess(string pName)
        {
            Process[] detectLoL = System.Diagnostics.Process.GetProcessesByName(pName);
            if (detectLoL.Length == 0)
                return null;
            return detectLoL[0];
        }


        public static int GetProcessCount(string pName)
        {
            Process[] detectLoL = System.Diagnostics.Process.GetProcessesByName(pName);
            return detectLoL.Length;
        }

        public static void UnhandledExceptonHandler(object sender, UnhandledExceptionEventArgs args)
        {
            if (true == YesNoPopupWindow.AskQuestion(MainWindow.Instance, Utilities.GetString("AskSendReport") as string))
            {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\BaronReplays.exe", "-force");
            }
        }

        public async static Task<bool> IsUpdateAvailable()
        {
#if (!DEBUG)
            int newestVersion = 0;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    newestVersion = int.Parse(await wc.DownloadStringTaskAsync(Constants.BRapiPrefix + "version"));
                }
                catch (Exception)
                {

                }
            }

            if (Version < newestVersion)
            {
                HasNewVersion = true;
                return true;
            }

#endif
            HasNewVersion = false;
            return false;
        }


        public static string SearchFileInAncestor(string fullPath, string fName)
        {
            string[] piece = fullPath.Split('\\');
            for (int i = piece.Length - 1; i >= 0; i--)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < i; j++)
                {
                    sb.Append(piece[j]);
                    sb.Append('\\');
                }
                if (File.Exists(sb.ToString() + fName))
                    return (sb.Remove(sb.Length - 1, 1)).ToString();
            }
            return null;
        }


        public static bool SelectLoLExePath()
        {
            OpenFileDialog lolFolderSelector = new OpenFileDialog();
            lolFolderSelector.Filter = "League of Legends.exe|League of Legends.exe";
            lolFolderSelector.Title = Utilities.GetString("PickLoLExe") as string;
            lolFolderSelector.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            lolFolderSelector.ShowDialog();
            if (lolFolderSelector.FileName == string.Empty)
                return false;
            Properties.Settings.Default.LoLGameExe = lolFolderSelector.FileName;
            Properties.Settings.Default.Save();
            return true;
        }

        public static void IfDirectoryNotExitThenCreate(String path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static BitmapImage GetResourceBitmap(String uri)
        {
            StreamResourceInfo sri = null;
            try
            {
                sri = Application.GetResourceStream(new Uri(AppUri + uri));

            }
            catch (Exception)
            {
                return null;
            }
            using (Stream s = sri.Stream)
            {
                return ReadImage(s);
            }
        }

        public static BitmapImage GetBitmapImage(string s)
        {
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ms.SetLength(fs.Length);
                fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                ms.Flush();
                fs.Close();
            }
            return ReadImage(ms);
        }

        public static BitmapImage ReadImage(Stream ms)
        {
            BitmapImage src = new BitmapImage();
            try
            {
                src.BeginInit();
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.StreamSource = ms;
                src.EndInit();
                src.Freeze();
            }
            catch
            {
                return null;
            }
            return src;
        }

        public static String GetString(string name)
        {
            String str;
            try
            {
                str = Application.Current.FindResource(name) as string;
            }
            catch
            {
                return name;
            }
            return str;
        }

        public static String ReadFileTextSafety(String path)
        {
            if (path == null)
                return String.Empty;
            String tempFile = Path.GetTempFileName();
            System.IO.File.Copy(path, tempFile, true);
            string result = File.ReadAllText(tempFile);
            File.Delete(tempFile);
            return result;
        }

        public static String GetExecutablePath(String processName)
        {
            String exePath = null;
            Process p = Utilities.GetProcess(processName);
            ProcessModule module = null;
            try
            {
                module = p.MainModule;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog("Get process MainModule failed: " + e.Message);
            }

            try
            {
                exePath = module.FileName;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog("Get process MainModule FileName failed: " + e.Message);
            }

            if (exePath == null)
            {
                try
                {
                    exePath = LeagueOfLegendsAnalyzer.GetMainModuleFilepath(p.Id);
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteLog("GetExecutablePath: " + e.Message);
                }
            }

            if (exePath == null)
            {
                if (System.Environment.OSVersion.Version.Major >= 6)
                {
                    ProcessMemory pm = new ProcessMemory();
                    pm.openProcess(Constants.LoLExeName);
                    exePath = pm.GetProcessPath();
                    pm.closeProcess();
                }
            }

            if (exePath == null)
            {
                Logger.Instance.WriteLog(String.Format("Can't get {0} path", processName));
            }
            else
            {
                Logger.Instance.WriteLog(String.Format("{0} path is {1}", processName, exePath));
            }
            return exePath;
        }

        public static String GetExecutableDirectory(String processName)
        {
            String exePath = GetExecutablePath(processName);
            if (exePath != null)
                return BaronReplays.Helper.FileHelper.GetFileLocation(exePath);
            return null;
        }

        public static String GetCommandLine(int pid)
        {
            String cmdLine = String.Empty;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + pid))
            {
                foreach (ManagementObject @object in searcher.Get())
                {
                    cmdLine = @object["CommandLine"] as String;
                }
            }
            Logger.Instance.WriteLog(cmdLine);
            return cmdLine;
        }


        public static String ExecuteCmd(String input)
        {
            Process cmd = new Process();

            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            cmd.Start();
            cmd.StandardInput.WriteLine("\n");
            cmd.StandardInput.Flush();

            cmd.StandardInput.WriteLine(input);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            String result = cmd.StandardOutput.ReadToEnd();
            cmd.Close();
            return result;
        }

        public static void DownloadFile(String url, String filePath)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(url, filePath);
                }
            }
            catch (Exception e)
            {
                throw new Exception("下載檔案失敗!");
            }
        }

        public static Boolean NeedGoToEnglishSites()
        {
            String nowLang = Properties.Settings.Default.Language;
            return !(String.Compare(nowLang, "zh-TW") == 0);
        }

        private static object staticDataLock = new object();
        public static void ReDownloadAllStaticData()
        {
            lock (staticDataLock)
            {
                if (BaronReplays.RiotAPI.Services.Request.UpdateAPILanguage())
                {
                    BaronReplays.LoLStaticData.Champion.Instance.ForceRenew();
                    BaronReplays.LoLStaticData.Item.Instance.ForceRenew();
                    BaronReplays.LoLStaticData.SummonerSpell.Instance.ForceRenew();
                }
            }
        }

        public static void UpdateSoftware()
        {
            var dp = new DownloadProgress(@"http://ahri.tw/BaronReplays/BaronReplays_Auto.exe", "BaronReplays_Auto.exe");
            dp.Owner = MainWindow.Instance;
            dp.ShowDialog();
            if (dp.IsSuccess.HasValue)
            {
                if (dp.IsSuccess.Value)
                {
                    SettingsSaver.SaveSettings();
                    System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\BaronReplays_AutoUpdate.exe");
                }
            }
        }

        public static void UploadBugReport()
        {   
            //user 有反應才上傳
            if (BugReportString.Length != 0)
            {
                BaronReplays.BRapi.Service.ReportService.ReportBug(Brid, Logger.Instance.LogContent, BugReportString);
            }
        }
    }




}
