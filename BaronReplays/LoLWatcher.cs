using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows.Threading;

namespace BaronReplays
{
    public class LoLWatcher : DispatcherObject
    {
        private static LoLWatcher instance = new LoLWatcher();
        public LoLExeData ExecutionData;
        private Process lol;
        public Process LoL
        {
            get
            {
                return lol;
            }
        }

        public delegate void LoLStarted(LoLWatcher sender, LoLExeData executiondata, Boolean isSpectateMode);
        public delegate void LoLFinished();
        public event LoLStarted LoLStartedEvent;
        public event LoLFinished LoLFinishedEvent;

        private System.Timers.Timer watcher;

        public Boolean IsLoLExists()
        {
            return (lol != null);
        }

        private void UpdateLoL()
        {
            Process pastLol = lol;
            lol = Utilities.GetProcess(Constants.LoLExeName);
            if (pastLol == null && lol != null)
            {
                ExecutionData = new LoLExeData();
                ExecutionData.Process = lol;
                ExecutionData.ExecutablePath = LeagueOfLegendsAnalyzer.GetLoLExecutingPath();
                ExecutionData.LogFilePath = GetLogFilePath(ExecutionData.ExecutablePath);
                ExecutionData.CommandLine = Utilities.GetCommandLine(lol.Id);
                ExecutionData.LoLProcess = lol;
                LoLCommandAnalyzer analyzer = new LoLCommandAnalyzer(ExecutionData.CommandLine);

                ExecutionData.MatchInfo = analyzer.GetGameInfo();
                if (LoLStartedEvent != null)
                {
                    LoLStarted dele = new LoLStarted(LoLStartedEvent);
                    Dispatcher.Invoke(dele, this, ExecutionData, analyzer.IsSuccess);
                }

            }
            else if (pastLol != null && lol == null)
            {
                ExecutionData = null;
                if (LoLFinishedEvent != null)
                    LoLFinishedEvent();
            }
        }

        public void StartWatcher()
        {
            watcher = new System.Timers.Timer();
            watcher.Interval = 500;
            watcher.Elapsed += WatcherTick;
            watcher.Start();
        }

        private void WatcherTick(object sender, ElapsedEventArgs e)
        {
            UpdateLoL();
        }

        private LoLWatcher()
        {

        }

        public static LoLWatcher Instance
        {
            get
            {
                return instance;
            }
        }

        private String GetLogFilePath(String exePath)
        {
            DateTime lolStartTime = lol.StartTime;
            FileInfo newestFile = null;
            int tryTimes = 0;
            try
            {
                String[] logPath = GetR3dLogDirectory(exePath);
                do
                {
                    foreach (String s in logPath)
                    {
                        newestFile = GetNewestTxtFromDirectory(s);
                        if (newestFile.CreationTime < lolStartTime)
                        {
                            Logger.Instance.WriteLog(String.Format("Search log in {0}", s));
                            Logger.Instance.WriteLog(String.Format("Latest log file's creation time is earlier then LoL launch time: {0}", newestFile.CreationTime - lolStartTime));
                        }
                        else
                        {
                            Logger.Instance.WriteLog(String.Format("R3d logs folder is {0}", s));
                            return newestFile.FullName;
                        }
                    }
                    SpinWait.SpinUntil(() => false, 1000);
                } while (tryTimes++ < 3);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog(String.Format("Can not locate log file: {0}", ex.Message));
            }
            Logger.Instance.WriteLog("Can not locate log file");
            return null;
        }

        public static String[] GetR3dLogDirectory(String exePath)
        {
            List<String> logDirs = new List<string>();
            DirectoryInfo logdir = null;
            try
            {

                logdir = new DirectoryInfo(exePath.Substring(0, exePath.LastIndexOf('\\')));
                do
                {
                    if (Directory.Exists(logdir.FullName + @"\Logs\Game - R3d Logs"))
                    {
                        logDirs.Add(logdir.FullName + @"\Logs\Game - R3d Logs");
                    }
                    logdir = logdir.Parent;
                }
                while (logdir != null);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog("Failed to get r3d logs directory");
                Logger.Instance.WriteLog(String.Format("Error message is: {0}", e.Message));
            }
            return logDirs.ToArray();
        }



        public static FileInfo GetNewestTxtFromDirectory(String dirPath)
        {
            DirectoryInfo logdir = new DirectoryInfo(dirPath);
            FileInfo[] files = logdir.GetFiles("*.txt");
            if (files.Length == 0)
            {
                Logger.Instance.WriteLog("No files in log directory");
                throw new Exception("No files in log directory");
            }
            FileInfo newestFile = files.OrderByDescending(x => x.LastWriteTime).ToArray().First();
            return newestFile;
        }


        private String[] GetCommandArgs()
        {
            String cmdLine = Utilities.GetCommandLine(lol.Id);
            String[] splitedArgs = cmdLine.Split(new char[] { '\"' });
            List<String> result = new List<string>();
            foreach (String s in splitedArgs)
            {
                String trims = s.Trim();
                if (trims.Length != 0)
                    result.Add(trims);
            }

            return result.ToArray();
        }

        private String GetCmdAddress()
        {
            String[] argsArr = GetCommandArgs();
            return argsArr[argsArr.Length - 1];
        }
    }
}
