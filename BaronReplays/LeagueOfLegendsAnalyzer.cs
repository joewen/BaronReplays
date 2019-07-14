using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows;

namespace BaronReplays
{
    static class LeagueOfLegendsAnalyzer
    {
        public static Process LoLProcess;

        public static bool IsLoLExists()
        {
            LoLProcess = Utilities.GetProcess(Constants.LoLExeName);
            return (LoLProcess != null);
        }




        private static String[] LoLExeDefaultPaths = { 
                                                        @"C:\Program Files (x86)\GarenaLoLTW\GameData\Apps\LoLTW\Game\League of Legends.exe" ,
                                                        @"C:\Program Files (x86)\GarenaLoLID\GameData\Apps\LoLID\Game\League of Legends.exe" ,
                                                        @"C:\Program Files (x86)\GarenaLoLVN\GameData\Apps\LoLVN\Game\League of Legends.exe" ,
                                                        @"C:\Program Files (x86)\GarenaLoLPH\GameData\Apps\LoLPH\Game\League of Legends.exe" ,
                                                        @"C:\Program Files\GarenaLoLTW\GameData\Apps\LoLTW\Game\League of Legends.exe",
                                                        @"C:\Program Files\GarenaLoLID\GameData\Apps\LoLID\Game\League of Legends.exe" ,
                                                        @"C:\Program Files\GarenaLoLVN\GameData\Apps\LoLVN\Game\League of Legends.exe" ,
                                                        @"C:\Program Files\GarenaLoLPH\GameData\Apps\LoLPH\Game\League of Legends.exe" ,
                                                     };

        public static String GetRiotRegionExePath()
        {
            String basePath;
            RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software", false);
            reg = reg.OpenSubKey("Riot Games", false);
            if (reg != null)
            {
                reg = reg.OpenSubKey(Constants.LoLExeName, false);
                if (reg == null)
                    return null;
                else
                    basePath = reg.GetValue("Path") as String;
                try
                {
                    String gameSln = basePath + "RADS\\solutions\\lol_game_client_sln\\releases";

                    DirectoryInfo dt = new DirectoryInfo(basePath + "RADS\\solutions\\lol_game_client_sln\\releases");
                    String fullpath = dt.GetDirectories()[0].FullName;
                    if (File.Exists(fullpath + "\\deploy\\League of Legends.exe"))
                        return fullpath + "\\deploy\\League of Legends.exe";
                }
                catch { return null; }
            }
            return null;
        }

        public static Boolean ResetLoLExePath()
        {
            Properties.Settings.Default.LoLGameExe = string.Empty;
            return CheckLoLExeSelected();
        }

        public static Boolean CheckLoLExeSelected()
        {
            if (Properties.Settings.Default.LoLGameExe.Length != 0)
            {

                if (!File.Exists(Properties.Settings.Default.LoLGameExe))
                {
                    Properties.Settings.Default.LoLGameExe = String.Empty;
                }
                else if (Properties.Settings.Default.LoLGameExe.Contains("LoLExecutable"))
                {
                    String gameExeRoot = Properties.Settings.Default.LoLGameExe.Substring(0, Properties.Settings.Default.LoLGameExe.IndexOf("LoLExecutable"));
                    if (String.Compare(gameExeRoot, AppDomain.CurrentDomain.BaseDirectory) == 0)
                    {
                        Properties.Settings.Default.LoLGameExe = String.Empty;
                    }
                }
            }

            if (Properties.Settings.Default.LoLGameExe.Length == 0)
            {
                String path = GetRiotRegionExePath();
                if (path != null)
                {
                    Properties.Settings.Default.LoLGameExe = path;
                    return true;
                }
                foreach (String s in LoLExeDefaultPaths)
                {
                    if (File.Exists(s))
                    {
                        Properties.Settings.Default.LoLGameExe = s;
                        Properties.Settings.Default.Save();
                        return true;
                    }
                }
                PopupWindow.ShowMessage(MainWindow.Instance,Utilities.GetString("SelectLoLExe") as string);
                if (!Utilities.SelectLoLExePath())
                    return false;

            }
            return true;
        }

        public static String GetMainModuleFilepath(int processId)
        {
            try
            {
                string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
                using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                {
                    using (var results = searcher.Get())
                    {
                        ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                        if (mo != null)
                        {
                            return (string)mo["ExecutablePath"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog("GetMainModuleFilepath: " + e.Message);
                return null;
            }
            return null;
        }

        public static String GetLoLExecutingPath()
        {
            string exePath = Utilities.GetExecutablePath(Constants.LoLExeName);
            if (exePath == null)
            {
                Logger.Instance.WriteLog("Can not get LoL path, use default path.");
                if (CheckLoLExeSelected())
                    exePath = Properties.Settings.Default.LoLGameExe;
            }
            else
            {
                Properties.Settings.Default.LoLGameExe = exePath;
                Properties.Settings.Default.Save();
            }
            return exePath;
        }


    }
}
