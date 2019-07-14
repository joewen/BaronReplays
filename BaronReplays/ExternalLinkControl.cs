using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BaronReplays
{
    public static class ExternalLinkControl
    {

        public static void CheckRegistrySettings()
        {
            UpdateLprFileSupport();
            UpdateCreateShortCutOnDesktop();
            UpdateStartAtSystemStartup();
            UpdateURLSupportSupport();
        }



        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static String OpenLprFileCommand = Process.GetCurrentProcess().MainModule.FileName + " -replay \"%1\"";
        public static String AnalyzeCmdCommand = Process.GetCurrentProcess().MainModule.FileName + " -cmd \"%1\"";
        public static String StartupCommand = Process.GetCurrentProcess().MainModule.FileName + " -minimized";
        public static String Protocol = "baronreplays";
        public static String ReplayFileExtension = ".lpr";
        public static String ReplayFileExtensionIco = @"\IconLpr.ico";//ICO的路徑



        #region URL support
        public static void UpdateURLSupportSupport()
        {
            if (!IsURLSupportExist())
                AddURLSupport();
        }

        public static void AddURLSupport()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.CreateSubKey(Protocol);
                key.SetValue("", "URL: BaronReplays Protocol");
                key.SetValue("URL Protocol", "");
                key.CreateSubKey("DefaultIcon").SetValue("", System.Windows.Forms.Application.StartupPath + ReplayFileExtensionIco);
                key.CreateSubKey("shell\\open\\command").SetValue("", AnalyzeCmdCommand, RegistryValueKind.ExpandString);
            }
            catch (Exception e)
            {

            }
        }

        public static Boolean IsURLSupportExist()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(Protocol + "\\shell\\open\\command", true);
                if (key == null)
                {
                    return false;
                }
                else
                {
                    String cmd = key.GetValue("") as String;
                    if (cmd.CompareTo(AnalyzeCmdCommand) != 0)
                    {
                        Registry.ClassesRoot.DeleteSubKeyTree(Protocol);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Query BR protocal supported failed: {0}", e.Message));
            }
            return false;
        }
        #endregion


        #region Lpr extension
        public static void UpdateLprFileSupport()
        {
            if (!IsLprFileSupportExist())
                AddLprFileSupport();
        }

        public static Boolean IsLprFileSupportExist()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(ReplayFileExtension + "\\shell\\open\\command", true);
                if (key == null)
                {
                    return false;
                }
                else
                {
                    String cmd = key.GetValue("") as String;
                    if (cmd.CompareTo(OpenLprFileCommand) != 0)
                    {
                        Registry.ClassesRoot.DeleteSubKeyTree(ReplayFileExtension);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Query Lpr file supported failed: {0}", e.Message));
            }
            return false;
        }

        public static void AddLprFileSupport()
        {
            try
            {
                Registry.ClassesRoot.CreateSubKey(ReplayFileExtension).SetValue("", "BaronReplays replay file");
                Registry.ClassesRoot.CreateSubKey(ReplayFileExtension + "\\DefaultIcon").SetValue("", System.Windows.Forms.Application.StartupPath + ReplayFileExtensionIco);
                Registry.ClassesRoot.CreateSubKey(ReplayFileExtension + "\\shell\\open\\command").SetValue("", OpenLprFileCommand, RegistryValueKind.ExpandString);
                SHChangeNotify(0x8000000, 0, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(String.Format("Add Lpr file supported failed: {0}", e.Message));
            }
        }

        #endregion
        public static void UpdateCreateShortCutOnDesktop()
        {
            try
            {
                string lnkPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\BaronReplays.lnk";
                if (Properties.Settings.Default.CreateShortCutOnDesktop)
                {
                    IWshRuntimeLibrary.WshShellClass shell = new IWshRuntimeLibrary.WshShellClass();
                    IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnkPath);
                    shortcut.TargetPath = Process.GetCurrentProcess().MainModule.FileName;
                    shortcut.Save();
                }
                else
                {
                    if (System.IO.File.Exists(lnkPath))
                    {
                        System.IO.File.Delete(lnkPath);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        private static void DeleteStartupRegistry()
        {
            try
            {
                RegistryKey startupRegKey;
                if (Environment.Is64BitOperatingSystem)
                {
                    startupRegKey = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Run", true);
                }
                else
                {
                    startupRegKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                }
                startupRegKey.DeleteValue("BaronReplays");
            }
            catch (Exception e)
            {
            }
        }

        private static Boolean IsStartupRegistryExists()
        {
            try
            {
                RegistryKey startupRegKey;
                if (Environment.Is64BitOperatingSystem)
                {
                    startupRegKey = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Run", true);
                }
                else
                {
                    startupRegKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                }
                String cmd = startupRegKey.GetValue("BaronReplays") as String;
                if (cmd == null)
                {
                    return false;
                }
                else
                {
                    if (cmd.CompareTo(StartupCommand) != 0)
                    {
                        DeleteStartupRegistry();
                        return false;
                    }
                }
            }
            catch (Exception e)
            {

            }
            return true;
        }


        public static void UpdateStartAtSystemStartup()
        {
            try
            {
                UpdateStartAtSystemStartupSchtasks();
            }
            catch (Exception e)
            {

            }
        }

        private static void UpdateStartAtSystemStartupSchtasks()
        {
            if (Properties.Settings.Default.StartWithSystem)
                Utilities.ExecuteCmd("schtasks /create /tn \"BaronReplays\" /tr \"" + StartupCommand + "\" /sc onlogon /rl HIGHEST /f");
            else
                Utilities.ExecuteCmd("schtasks /delete /tn \"BaronReplays\" /f");
        }



    }
}
