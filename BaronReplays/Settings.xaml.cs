using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Collections;
using System.Runtime.InteropServices;

namespace BaronReplays
{
    /// <summary>
    /// Settings.xaml 的互動邏輯
    /// </summary>
    public partial class Settings : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string LoLGameExe
        {
            get
            {
                if (Properties.Settings.Default.LoLGameExe.Length == 0)
                    return Utilities.GetString("PleasePlayFirst") as String;
                return Properties.Settings.Default.LoLGameExe;
            }
        }

        public event ReplayManager.LoadRecordsAsyncDelegate ReplayPathChanged;

        public string ReplaySavePath
        {
            get
            {
                return Properties.Settings.Default.ReplayDir;
            }
            set
            {
                Properties.Settings.Default.ReplayDir = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("ReplaySavePath");
                if (ReplayPathChanged != null)
                    ReplayPathChanged(value);

            }
        }

        public string MovieSavePath
        {
            get
            {
                return Properties.Settings.Default.MovieDir;
            }
            set
            {
                Properties.Settings.Default.MovieDir = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("MovieSavePath");
            }
        }

        public string FileNameFormat
        {
            get
            {
                return Properties.Settings.Default.FileNameFormat;
            }
            set
            {
                Properties.Settings.Default.FileNameFormat = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("FileNameFormat");
            }
        }

        public List<String> regions = null;
        public List<String> Regions
        {
            get
            {
                if(regions == null)
                {
                    regions = new List<String>();
                    foreach(String s in Utilities.Regions)
                        regions.Add(Utilities.GetString(s));
                }
                return regions;
            }
        }

        public bool KillProcess
        {
            get
            {
                return Properties.Settings.Default.KillProcess;
            }
            set
            {
                Properties.Settings.Default.KillProcess = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("KillProcess");
            }
        }

        public bool AlwaysTaskbar
        {
            get
            {
                return Properties.Settings.Default.AlwaysTaskbar;
            }
            set
            {
                Properties.Settings.Default.AlwaysTaskbar = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("AlwaysTaskbar");
            }
        }

        public bool StartWithSystem
        {
            get
            {
                return Properties.Settings.Default.StartWithSystem;
            }
            set
            {
                Properties.Settings.Default.StartWithSystem = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("StartWithSystem");
                ExternalLinkControl.UpdateStartAtSystemStartup();
            }
        }

        public bool CreateShortCutOnDesktop
        {
            get
            {
                return Properties.Settings.Default.CreateShortCutOnDesktop;
            }
            set
            {
                Properties.Settings.Default.CreateShortCutOnDesktop = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("CreateShortCutOnDesktop");
                ExternalLinkControl.UpdateCreateShortCutOnDesktop();
            }
        }

        public bool EnableCameraRotation
        {
            get
            {
                return Properties.Settings.Default.EnableCameraRotation;
            }
            set
            {
                Properties.Settings.Default.EnableCameraRotation = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("EnableCameraRotation");
            }
        }

        public bool AdvanceReplay
        {
            get
            {
                return Properties.Settings.Default.AdvanceReplay;
            }
            set
            {
                Properties.Settings.Default.AdvanceReplay = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool HideBorder
        {
            get
            {
                return Properties.Settings.Default.HideBorder;
            }
            set
            {
                Properties.Settings.Default.HideBorder = value;
                Properties.Settings.Default.Save();
                MainWindow.Instance.UpdateLeftBorderState();
            }
        }


        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public Settings()
        {
            InitializeComponent();
            DataContext = this;
            ExternalLinkControl.UpdateCreateShortCutOnDesktop();

        }



        private void SelectMovieSavePath(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog replayFolderSelector = new System.Windows.Forms.FolderBrowserDialog())
            {
                replayFolderSelector.ShowNewFolderButton = true;
                replayFolderSelector.Description = "選擇影片儲存路徑";
                if (replayFolderSelector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    MovieSavePath = replayFolderSelector.SelectedPath;
                }
            }
        }


        private void SelectSavePath(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog replayFolderSelector = new System.Windows.Forms.FolderBrowserDialog())
            {
                replayFolderSelector.ShowNewFolderButton = true;
                replayFolderSelector.Description = Utilities.GetString("PickReplaySaveFolder") as string;
                if (replayFolderSelector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ReplaySavePath = replayFolderSelector.SelectedPath;
                }
            }
        }


        private void MovieSavePathOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(MovieSavePath))
                Process.Start(MovieSavePath);
            else
            {
                if (true == YesNoPopupWindow.AskQuestion(MainWindow.Instance, Utilities.GetString("DirectoryNotExist") as string))
                {
                    Directory.CreateDirectory(MovieSavePath);
                    Process.Start(MovieSavePath);
                }
            }
        }

        private void OpenFolderPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(ReplaySavePath))
                Process.Start(ReplaySavePath);
            else
            {
                if (true == YesNoPopupWindow.AskQuestion(MainWindow.Instance, Utilities.GetString("DirectoryNotExist") as string))
                {
                    Directory.CreateDirectory(ReplaySavePath);
                    Process.Start(ReplaySavePath);
                }
            }
        }

        private void SelectLoLGameExeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Utilities.SelectLoLExePath())
                NotifyPropertyChanged("LoLGameExe");
        }

        private void FileNameFormatChangeButton_Click(object sender, RoutedEventArgs e)
        {
            FileNameFormat fnf = new BaronReplays.FileNameFormat();
            MainWindow.Instance.SwitchMainContent(fnf);
        }

        private void FileNameFormatTextbox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (Char c in e.Text)
            {
                if (Utilities.invaildFileNameChar.Contains(c))
                {
                    e.Handled = true;
                }
            }
        }

        private void SettingView_Loaded(object sender, RoutedEventArgs e)
        {
            int index = Utilities.Regions.IndexOf(Properties.Settings.Default.Platform);
            if(index < 0)
                RegionSelector.Text = Properties.Settings.Default.Platform;
            RegionSelector.SelectedIndex = index;
        }

        private void RegionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (String.Compare(Properties.Settings.Default.Platform, Utilities.Regions[RegionSelector.SelectedIndex]) != 0)
            {
                Properties.Settings.Default.Platform = Utilities.Regions[RegionSelector.SelectedIndex];
                Properties.Settings.Default.Save();
            }
        }

    }
}
