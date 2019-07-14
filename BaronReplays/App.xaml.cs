using BaronReplays.Database;
using HockeyApp;
using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace BaronReplays
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public Boolean NormalStart = true;

        public App()
        {
            Thread.CurrentThread.Name = "Main";
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Utilities.UnhandledExceptonHandler);
        }

        private void Init()
        {
            LanguageControl.LoadLanguage();

            if (!CheckDotNetVersion())
            {
                MessageBox.Show("Please install the latest version of .Net Framework \n請先安裝最新版本的 .Net Framework");
                Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=48137");
                App.Current.Shutdown();
                return;
            }

            SettingsSaver.CheckSettings();
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            WriteBasicInfoIntoLog();
            BRapi.Service.LaunchService.Launch();
            AnalyzeCommandArgs();
            DetectDuplicateExecute();
            ExternalLinkControl.CheckRegistrySettings();
            CheckNecessaryDirs();
            GameDatabase.Instance.InitDatabases();
            if (NormalStart)
                StartupUri = new Uri(Utilities.AppUri + "MainWindow.xaml");
            else
                App.Current.Shutdown();

        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Init();

            var brid = Utilities.Brid;

            #region HOCKEYAPP SAMPLE CODE

            HockeyClient.Current.Configure("bcf38c301af04a9da582f0a31ddf402b").SetContactInfo(brid, brid);

#if DEBUG
            ((HockeyClient)HockeyClient.Current).OnHockeySDKInternalException += (s, args) =>
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            };
#endif
            //send crashes to the HockeyApp server
            await HockeyClient.Current.SendCrashesAsync(true);
            #endregion

        }

        private bool CheckDotNetVersion()
        {
            if (Environment.Version.Revision >= 18000)
            {
                return true;
            }
            return false;
        }

        private void CheckNecessaryDirs()
        {
            CheckReplayDir();
            CheckMovieDir();
            CheckDataDir();
        }

        private void CheckDataDir()
        {
            Utilities.IfDirectoryNotExitThenCreate("Data");
        }

        private void CheckMovieDir()
        {

            if (BaronReplays.Properties.Settings.Default.MovieDir.Length == 0 || !Directory.Exists(BaronReplays.Properties.Settings.Default.MovieDir))
                BaronReplays.Properties.Settings.Default.MovieDir = AppDomain.CurrentDomain.BaseDirectory + "Movies";
            Utilities.IfDirectoryNotExitThenCreate(BaronReplays.Properties.Settings.Default.MovieDir);
        }

        private void CheckReplayDir()
        {
            if (BaronReplays.Properties.Settings.Default.ReplayDir.Length == 0 || !Directory.Exists(BaronReplays.Properties.Settings.Default.ReplayDir))
                BaronReplays.Properties.Settings.Default.ReplayDir = AppDomain.CurrentDomain.BaseDirectory + "Replays";
            Utilities.IfDirectoryNotExitThenCreate(BaronReplays.Properties.Settings.Default.ReplayDir);
        }

        private void WriteBasicInfoIntoLog()
        {
            Logger.Instance.WriteLog(String.Format("BaronReplays version: {0}", Utilities.Version));
            Logger.Instance.WriteLog($"Device Id: {Utilities.Brid}");
            if (Environment.Is64BitOperatingSystem)
                Logger.Instance.WriteLog(Environment.OSVersion.ToString() + " 64-bit");
            else
                Logger.Instance.WriteLog(Environment.OSVersion.ToString() + " 32-bit");
            Logger.Instance.WriteLog(Environment.CommandLine);
        }

        private Boolean DisableDuplicateExecuteAlert = false;
        private void DetectDuplicateExecute()
        {
            if (Utilities.GetProcessCount("BaronReplays") > 1)
            {
                if (!DisableDuplicateExecuteAlert)
                {
                    PopupWindow.ShowMessage(null, Utilities.GetString("BRAlreadyRunning") as string);
                }
                NormalStart = false;
            }
        }

        private void AnalyzeCommandArgs()
        {
            string[] cmdLineArgs = Environment.GetCommandLineArgs();
            int argsCount = cmdLineArgs.Length;
            if (argsCount > 1)
            {
                for (int i = 1; i < argsCount; i++)
                {
                    Logger.Instance.WriteLog(String.Format("{0} {1}", i, cmdLineArgs[i]));
                    if (String.Compare(cmdLineArgs[i], "-minimized") == 0)
                    {
                        Utilities.StartupMinimized = true;
                        //minimized window
                    }
                    else if (String.Compare(cmdLineArgs[i], "-reset") == 0)
                    {
                        BaronReplays.Properties.Settings.Default.Reset();
                        BaronReplays.Properties.Settings.Default.Save();
                    }
                    else if (String.Compare(cmdLineArgs[i], "-clean") == 0)
                    {
                        try
                        {
                            if (Directory.Exists("Data"))
                                Directory.Delete("Data", true);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else if (String.Compare(cmdLineArgs[i], "-replay") == 0)
                    {
                        try
                        {
                            String path = cmdLineArgs[i + 1];
                            Logger.Instance.WriteLog(String.Format("{0} {1}", i, path));
                            if ((Utilities.GetProcessCount("BaronReplays") + Utilities.GetProcessCount("BaronReplays.vshost")) > 1)
                            {
                                SendMessageToExistBR(path);
                                Application.Current.Shutdown();
                            }

                        }
                        catch (Exception) { }
                    }
                    else if (String.Compare(cmdLineArgs[i], "-cmd") == 0)
                    {
                        try
                        {
                            String path = cmdLineArgs[i + 1];
                            Logger.Instance.WriteLog(String.Format("{0} {1}", i, path));
                            if ((Utilities.GetProcessCount("BaronReplays") + Utilities.GetProcessCount("BaronReplays.vshost")) > 1)
                            {
                                SendMessageToExistBR(path);
                                Application.Current.Shutdown();
                            }
                        }
                        catch (Exception) { }
                    }
                    else if (String.Compare(cmdLineArgs[i], "-force") == 0)
                    {
                        try
                        {
                            int selfPid = Process.GetCurrentProcess().Id;
                            Process[] processes = Process.GetProcessesByName("BaronReplays");
                            foreach (Process p in processes)    //強制關閉所有不是自己的BaronReplays
                            {
                                if (p.Id != selfPid)
                                {
                                    p.Kill();
                                }
                            }
                            while (Utilities.GetProcessCount("BaronReplays") + Utilities.GetProcessCount("BaronReplays.vshost") > 1)
                                Thread.Sleep(100);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void SendMessageToExistBR(String msg)
        {
            Logger.Instance.WriteLog(String.Format("Send message: {0}", msg));
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 52039);
            UdpClient uc = new UdpClient();
            byte[] exitBytes = Encoding.Default.GetBytes(msg);
            uc.Send(exitBytes, exitBytes.Length, ipep);
            uc.Close();
        }


        private void UpdateLprFileSupport()
        {
            try
            {
                String Extension = ".lpr";//副檔名名稱
                String Extension_Icopath = @"\IconLpr.ico";//ICO的路徑
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(Extension, true);
                if (key == null)
                {
                    //Registry.ClassesRoot.DeleteSubKeyTree(".lpr");
                    Registry.ClassesRoot.CreateSubKey(Extension).SetValue("", "BaronReplays replay file");
                    Registry.ClassesRoot.CreateSubKey(Extension + "\\DefaultIcon").SetValue("", System.Windows.Forms.Application.StartupPath + Extension_Icopath);
                    Registry.ClassesRoot.CreateSubKey(Extension + "\\shell\\open\\command").SetValue("", Process.GetCurrentProcess().MainModule.FileName + " \"%1\"", RegistryValueKind.ExpandString);
                    SHChangeNotify(0x8000000, 0, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog("UpdateLprFileSupport failed: " + e.Message);
            }
        }


        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Utilities.UploadBugReport();
        }
    }
}
