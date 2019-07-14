using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Resources;
using System.Windows.Controls;
using BaronReplays.VideoRecording;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Threading.Tasks;

namespace BaronReplays
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<LoLRecorder, Thread> Recorders;
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private LoLLauncher _launcher;
        private MovieRecordingController videoRecordingController;
        private MoviePostProcessor movieProcessor;
        private List<String> changedSettings = new List<string>();

        private System.Timers.Timer adTimer;
        public bool LockMainView
        {
            get;
            set;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wparam, int lparam);

        private static MainWindow instance;
        public static MainWindow Instance
        {
            get
            {
                return instance;
            }
            set
            {
                if (instance != null)
                    throw new Exception("呼叫了兩次MainWindow的建構子");
                instance = value;
            }
        }

        public MainWindow()
        {
            Instance = this;
            Recorders = new Dictionary<LoLRecorder, Thread>();
            CheckRegionSelected();
            InitializeComponent();
            InitNotifyIcon();

            InitReplayManager();

            InitLoLWatchTimer();
            InitPlayFileListener();

            Properties.Settings.Default.SettingChanging += Default_SettingChanging;
            Properties.Settings.Default.SettingsSaving += Default_SettingsSaving;

            SetStaticDataUpdatedCallback();
            LoLWatcher.Instance.StartWatcher();
            LoLClientWatcher.Instance.StartWatcher();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitWindowVisible();

            UpdateLeftBorderState();
            UpdateStatusBar();

            UpdateRecordList();
            ChechUpdateAsync();
            AnalyzeCommandArgs();
        }

        public void UpdateLeftBorderState()
        {
            if (Properties.Settings.Default.HideBorder)
                HideLeftBorder();
            else
                ShowLeftBorder();
        }

        private void HideLeftBorder()
        {
            LeftBorder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowLeftBorder()
        {
            LeftBorder.Visibility = System.Windows.Visibility.Visible;
        }

        private void SetStaticDataUpdatedCallback()
        {               // 目前列表只有英雄圖片需要更新
            BaronReplays.LoLStaticData.Champion.Instance.DataUpdated += StaticData_DataUpdated;
            //BaronReplays.LoLStaticData.Item.Instance.DataUpdated += StaticData_DataUpdated;   
            //BaronReplays.LoLStaticData.SummonerSpell.Instance.DataUpdated += StaticData_DataUpdated;
        }

        private void StaticData_DataUpdated(LoLStaticData.Base sender)
        {
            if (!Dispatcher.CheckAccess())
            {
                BaronReplays.LoLStaticData.Base.DataUpdatedDelegate dele = new LoLStaticData.Base.DataUpdatedDelegate(StaticData_DataUpdated);
                Dispatcher.Invoke(dele, sender);
                return;
            }
            //TODO: 更新UI Binding的圖片
            RecordsListBox.Items.Refresh();
        }

        private object settingsLock = new object();
        private void Default_SettingsSaving(object sender, CancelEventArgs e)
        {
            lock (settingsLock)
            {
                foreach (String settingsName in changedSettings)
                {
                    if (String.Compare(settingsName, "SummonerName") == 0)
                        replayManager.ChangeSummonerName(Properties.Settings.Default.SummonerName as String);
                    else if (String.Compare(settingsName, "Language") == 0)
                    {
                        Task.Run(() => Utilities.ReDownloadAllStaticData());
                        UpdateStatusBar();
                    }
                }
                changedSettings.Clear();
            }
        }


        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            lock (settingsLock)
            {
                if (!changedSettings.Contains(e.SettingName))
                    changedSettings.Add(e.SettingName);
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
                    if (String.Compare(cmdLineArgs[i], "-replay") == 0)
                    {
                        PlayFile(cmdLineArgs[i + 1]);
                        i++;
                    }
                    else if (String.Compare(cmdLineArgs[i], "-cmd") == 0)
                    {
                        String wholecmd = String.Empty;
                        for (int j = i + 1; j < argsCount; j++)
                        {
                            wholecmd = wholecmd + cmdLineArgs[j] + " ";
                        }
                        TryToPlayCommand(wholecmd);
                        break;
                    }
                }
            }
        }

        public delegate void PlayFileDelegate(String path);
        public void PlayFile(String path)
        {
            if (!Dispatcher.CheckAccess())
            {
                PlayFileDelegate act = new PlayFileDelegate(PlayFile);
                Dispatcher.Invoke(act, path);
                return;
            }
            StartLoLLauncher(new LoLLauncher(path));
        }

        private void CheckRegionSelected()
        {
            if (Properties.Settings.Default.Platform.Length == 0)
            {
                RegionSelector rs = new RegionSelector();
                rs.ShowDialog();
            }
        }

        private ReplayManager replayManager;
        private void InitReplayManager()
        {
            replayManager = new ReplayManager();
            replayManager.PlayRecordEvent += PlayRecord;
            replayManager.ExportRecordEvent += ExportRecord;
            replayManager.StopRecordingEvent += RemoveRecoder;
            replayManager.SearchCompleted += replayManager_SearchCompleted;
            replayManager.LoadRecordsAsync(Properties.Settings.Default.ReplayDir);
            RecordsListBox.ItemsSource = replayManager.Records;
        }

        private void InitWindowVisible()
        {
            if (Utilities.StartupMinimized)
                HideWindow();
            else
            {
                ShowInTaskbar = true;
            }
        }

        private void InitNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            StreamResourceInfo sri = Application.GetResourceStream(new Uri("Icon.ico", UriKind.Relative));
            if (sri != null)
            {
                using (Stream s = sri.Stream)
                {
                    _notifyIcon.Icon = new System.Drawing.Icon(s);
                    // Do something with the stream...
                }
            }
            //_notifyIcon.Icon = new System.Drawing.Icon(AppDomain.CurrentDomain.BaseDirectory + "\\Icon.ico");

            _notifyIcon.DoubleClick += _notifyIcon_DoubleClick;
            _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add(Utilities.GetString("Exit") as string);
            _notifyIcon.ContextMenuStrip.Items[0].Click += ExitFromIcon;
            _notifyIcon.Visible = true;
        }

        private async void ChechUpdateAsync()
        {
            if (await Task.Run(() => Utilities.IsUpdateAvailable()))
            {
                if (Properties.Settings.Default.NotifyUpdate)
                    Menu_Function_Update_Click(this, null);
            }
        }


        private PlayFileListener _playFileListener;
        private void InitPlayFileListener()
        {
            _playFileListener = new PlayFileListener();
            _playFileListener.PlayEvent += PlayRecord;
            Thread t = new Thread(new ThreadStart(_playFileListener.StartListening));
            t.IsBackground = true;
            t.Name = "BR Listener";
            t.Start();
        }

        private void ExitFromIcon(object sender, EventArgs e)
        {
            _closeFromIcon = true;
            Close();
        }


        private void ShowWindow(Boolean topMost = false)
        {
            this.Visibility = Visibility.Visible;
            if (topMost)
            {
                this.Activate();
            }
        }

        private void HideWindow()
        {
            this.Visibility = Visibility.Hidden;
        }

        private void _notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowWindow(true);
            UpdateStatusBar();
        }

        private String GetStatusString()
        {
            String reuslt = null;
            if (videoRecordingController != null)
                reuslt = Utilities.GetString("MovieRecording");
            else if (Recorders.Count == 0)
                reuslt = Utilities.GetString("AutoStart");
            if (Recorders.Count > 0)
            {
                if (videoRecordingController != null)
                    reuslt += " & ";
                reuslt += String.Format(Utilities.GetString("RemainGameStatus"), Recorders.Count);
            }
            return reuslt;
        }

        private void UpdateStatusBar()
        {
            String statusStr = GetStatusString();
            if (IsLoaded)
            {
                StatusText.Text = statusStr;
            }
            _notifyIcon.Text = statusStr;
        }

        private void InitLoLWatchTimer()
        {
            LoLWatcher.Instance.LoLStartedEvent += LoLNewExecution;
            LoLWatcher.Instance.LoLFinishedEvent += LoLFinishExecution;
        }

        public delegate void RecordDelegate(SimpleLoLRecord slr);
        public void PlayRecord(SimpleLoLRecord slr)
        {
            if (!Dispatcher.CheckAccess())
            {
                RecordDelegate prd = new RecordDelegate(PlayRecord);
                Dispatcher.Invoke(prd, slr);
                return;
            }
            Logger.Instance.WriteLog(String.Format("Play record: {0}", slr.FileName));
            if (_launcher == null)
            {
                if (slr.RecoringRecorder != null)
                    StartLoLLauncher(new LoLLauncher(slr.RecoringRecorder));
                else
                    StartLoLLauncher(new LoLLauncher(slr));
            }

            else
                PopupWindow.ShowMessage(this, Utilities.GetString("AlreadyPlaying") as string);
        }

        private void ShowExportFailedMessage()
        {
            ShowNotifyUserMessage(Utilities.GetString("MovieRecordingFailed"));
            UpdateStatusBar();
        }

        public SimpleLoLRecord toExport;
        public void ExportRecord(SimpleLoLRecord slr)
        {
            if (!Dispatcher.CheckAccess())
            {
                RecordDelegate prd = new RecordDelegate(ExportRecord);
                Dispatcher.Invoke(prd, slr);
                return;
            }
            if (videoRecordingController != null)
            {
                if (videoRecordingController.LoLStarted)
                {
                    bool? answer = YesNoPopupWindow.AskQuestion(this, Utilities.GetString("VideoRecordingExitCheck"));
                    if (answer.HasValue)
                    {
                        if (answer.Value)
                        {
                            EndVideoRecordingMode();
                        }
                    }
                }
                else
                {
                    EndVideoRecordingMode();
                }
                return;
            }
            toExport = slr;
            MovieConfiguration configurater = new MovieConfiguration();
            if (toExport != null)
                configurater.FileNameBox.Text = slr.FileNameShort.Remove(slr.FileNameShort.Length - 4);
            configurater.Done += MovieConfigurationDone;
            SwitchMainContent(configurater);
        }

        public delegate void MovieConfigurationDoneDelegate(MovieConfiguration sender);
        public void MovieConfigurationDone(MovieConfiguration sender)
        {
            Menu_ReplayList(null, null);
            if (!sender.Finish)
                return;
            videoRecordingController = new MovieRecordingController();
            videoRecordingController.Quality = sender.Quality;
            videoRecordingController.OutputFileName = sender.FileName;

            MoviePostProcessor mpp = new MoviePostProcessor();
            mpp.PostProcessingDone += OnMoviePostProcessingDone;
            mpp.VideoRecorder = videoRecordingController;
            mpp.DoCut += DoCutMovie;
            videoRecordingController.DoneEvent += mpp.OnRecordingDone;


            UpdateStatusBar();
            if (toExport != null)
                PlayRecord(toExport);
            Thread t = new Thread(new ThreadStart(videoRecordingController.StartRecording));
            t.Name = "Movie";
            t.IsBackground = true;
            t.Start();
            StartVideoRecordingMode();
        }

        public delegate void DoCutMovieDelegate(MoviePostProcessor sender, MovieCutter cutter);
        public void DoCutMovie(MoviePostProcessor sender, MovieCutter cutter)
        {
            movieProcessor = sender;
            cutter.CuttingDown += CuttingDown;
            SwitchMainContent(cutter);
            LockMainView = true;
            this.Activate();
        }

        private void OnMoviePostProcessingDone(MoviePostProcessor processor, Boolean isSuccess)
        {
            LockMainView = false;
            if (isSuccess)
            {
                ShowNotifyUserMessage(Utilities.GetString("MovieRecordingSuccessed"));
                Nullable<Boolean> result = YesNoPopupWindow.AskQuestion(this, String.Format(Utilities.GetString("MovieRecordingPlayNow"), processor.FilePath));
                if (result == true)
                {
                    Process.Start(videoRecordingController.OutputFilePath);
                }
            }
            else
            {
                ShowExportFailedMessage();
            }
            EndVideoRecordingMode();
        }

        private void StartVideoRecordingMode()
        {
            UpdateStatusBar();
            Menu_VideoRecordingModeButton.LockDown = true;
        }
        private void EndVideoRecordingMode()
        {
            if (videoRecordingController.RecordingThread.IsAlive)
            {
                videoRecordingController.RecordingThread.Abort();
            }
            videoRecordingController = null;
            movieProcessor = null;
            UpdateStatusBar();
            Menu_VideoRecordingModeButton.LockDown = false;
            LockMainView = false;
            Menu_ReplayList(null, null);
        }

        public delegate void CuttingDownDelegate();
        private void CuttingDown()
        {
            LockMainView = false;
            Menu_ReplayList(null, null);
            ShowNotifyUserMessage(Utilities.GetString("MovieRecordingBackground"));
            movieProcessor.CombineVideoAndAudio();
        }

        public delegate void LoLLauncherDoneDelegate(bool isSuccess, string reason);
        public void LoLLauncherDone(bool isSuccess, string reason)
        {
            if (!isSuccess)
            {
                if (reason.CompareTo("OldVersionReplay") == 0)
                {
                    Nullable<Boolean> result = YesNoPopupWindow.AskQuestion(this, String.Format(Utilities.GetString("OldVersionReplay")));
                    if (result == true)
                    {
                        GoToOldVersionReplayGuide();
                    }
                }
                else
                    PopupWindow.ShowMessage(this, Utilities.GetString(reason) as String);
            }
            _launcher = null;
        }

        private void GoToOldVersionReplayGuide()
        {
            if (Utilities.NeedGoToEnglishSites())
            {
                Process.Start("https://ahri.tw/en/Events/20140321/");
            }
            else
            {
                Process.Start("https://ahri.tw/Events/20140321/");
            }
        }

        private void StartLoLLauncher(LoLLauncher launcher)
        {
            if (!LeagueOfLegendsAnalyzer.CheckLoLExeSelected())
            {
                return;
            }
            _launcher = launcher;
            _launcher.Parent = this;
            _launcher.AnyEvent += LoLLauncherDone;
            _launcher.StartPlaying();
        }

        public delegate void RemoveRecoderDelegate(LoLRecorder recoder, Boolean force = false);
        private void RemoveRecoder(LoLRecorder recoder, Boolean force = false)
        {
            if (!Dispatcher.CheckAccess())
            {
                RemoveRecoderDelegate refresh = new RemoveRecoderDelegate(RemoveRecoder);
                Dispatcher.BeginInvoke(refresh, recoder, force);
            }
            else
            {
                if (Recorders.ContainsKey(recoder))
                {
                    Thread t = this.Recorders[recoder];
                    if (t.IsAlive && force)
                    {
                        t.Abort();
                        recoder.removeFromList();
                    }
                    this.Recorders.Remove(recoder);
                    SimpleLoLRecord slr = replayManager.GetRecordFromRecorder(recoder);
                    if (slr != null)
                    {
                        this.replayManager.RemoveRecord(slr);
                    }
                    UpdateStatusBar();
                }
            }
        }

        public delegate void RecordDoneDelegate(LoLRecorder sender, bool isSuccess, string reason);
        public void RecordDone(LoLRecorder sender, bool isSuccess, string reason)
        {
            RemoveRecoder(sender as LoLRecorder);
            if (isSuccess)
            {
                String sb = FileNameGenerater.GenerateFilename(sender);
                string path = Properties.Settings.Default.ReplayDir + @"\" + sb;
                Utilities.IfDirectoryNotExitThenCreate(Properties.Settings.Default.ReplayDir);
                if (System.IO.File.Exists(path))
                    path = path.Insert(path.Length - 4, (new Random().Next(9999)).ToString());
                sender.record.writeToFile(path);

                Dispatcher.Invoke(() => ShowRecordSucceedMessage());

                if (sender.selfGame)
                {

                    Properties.Settings.Default.SummonerName = sender.selfPlayerInfo.PlayerName;
                    Properties.Settings.Default.Save();

                    BRapi.Service.RecordingService.ReportSuccess(sender.GameInfo.PlatformId);

                }
                if (sender.record.HasResult)
                {
                    BRapi.Service.GameService.SendEndOfGameStats(sender.record.GamePlatform, Newtonsoft.Json.JsonConvert.SerializeObject(sender.record.gameStats.OriginalAMFObject), sender.record.GameId);
                }

            }
            else
            {
                Dispatcher.Invoke(() => ShowRecordFailedMessageBox(reason));
                if (sender.selfGame)
                {
                    BRapi.Service.RecordingService.ReportFail(sender.record.GamePlatform);
                }

            }

            Dispatcher.Invoke(() => UpdateStatusBar());
        }

        private void ShowRecordSucceedMessage()
        {
            Menu_ReplayList(null, null);
            ShowNotifyUserMessage(Utilities.GetString("RecordSucceed") as string);
        }

        private void ShowRecordFailedMessageBox(string reason)
        {
            ShowNotifyUserMessage(String.Format(Utilities.GetString("RecordFailed") as string, Utilities.GetString(reason)));
        }

        public void RecordInfoDone(LoLRecorder recoder)
        {
            replayManager.UpdateRecordingRecord(recoder);
            UpdateRecordList();
            if (!recoder.selfGame && recoder.exeData != null)       //要是觀戰且必須有LoL可以關
            {
                if (Properties.Settings.Default.KillProcess)
                {
                    if (!recoder.exeData.LoLProcess.HasExited)
                        recoder.exeData.LoLProcess.CloseMainWindow();
                }
            }
        }

        private void LoLNewExecution(LoLWatcher sender, LoLExeData executiondata, bool isSpectateMode)
        {
            Logger.Instance.WriteLog("Detected LoL launched.");
            if (_launcher != null)
                return;
            StartNewRecoding(executiondata.MatchInfo, false, true);
        }

        private void LoLFinishExecution()
        {
            Logger.Instance.WriteLog("Detected LoL finished.");
            _launcher = null;
            if (videoRecordingController != null)
            {
                videoRecordingController.StopRecording();
            }
        }


        public LoLRecorder StartNewRecoding(GameInfo info = null, Boolean playNow = false, Boolean externalStart = false, PlayerInfo[] players = null)
        {
            if (!LeagueOfLegendsAnalyzer.IsLoLExists())     //觀戰用，LoL沒在執行時要先確定有LoL路徑
            {
                if (!LeagueOfLegendsAnalyzer.CheckLoLExeSelected())
                    return null;
            }
            LoLRecorder r = new LoLRecorder(info, externalStart);
            r.doneEvent += new RecordDoneDelegate(RecordDone);
            r.infoDoneEvent += new BaronReplays.LoLRecorder.RecordEventDelegate(RecordInfoDone);

            if (players != null)
                r.record.players = players;


            if (playNow)
            {
                if (!LeagueOfLegendsAnalyzer.CheckLoLExeSelected())
                {
                    return r;
                }
                r.prepareContentDoneEvent += Recorder_prepareContentDoneEvent;
            }

            ThreadStart recoderfunc = new ThreadStart(r.startRecording);
            Thread t = new Thread(recoderfunc);
            t.IsBackground = true;
            t.Name = "Recorder";
            this.Recorders.Add(r, t);
            t.Start();
            Logger.Instance.WriteLog(String.Format("Recorder thread created {0}({1})", t.ManagedThreadId, t.Name));

            UpdateStatusBar();
            return r;
        }

        private delegate void Recorder_prepareContentDoneEventDelegate(LoLRecorder recoder);
        private void Recorder_prepareContentDoneEvent(LoLRecorder recoder)
        {
            if (!Dispatcher.CheckAccess())
            {
                Recorder_prepareContentDoneEventDelegate dele = new Recorder_prepareContentDoneEventDelegate(Recorder_prepareContentDoneEvent);
                Dispatcher.Invoke(dele, recoder);
            }
            else
            {
                StartLoLLauncher(new LoLLauncher(recoder));
            }
        }

        public delegate void StartRecordFeaturedGameDelegate(GameInfo info, FeaturedGameJson gameData, Boolean playNow);
        public void StartRecordFeaturedGame(GameInfo info, FeaturedGameJson gameData, Boolean playNow)
        {
            PlayerInfo[] playerInfo = new PlayerInfo[gameData.participants.Length];
            for (int i = 0; i < gameData.BlueTeamParticipants.Length; i++)
            {
                playerInfo[i] = gameData.BlueTeamParticipants[i].ToPlayerInfo();
                playerInfo[i].clientID = i;
            }

            for (int i = 0; i < gameData.PurpleTeamParticipants.Length; i++)
            {
                int num = gameData.PurpleTeamParticipants.Length + i;
                playerInfo[num] = gameData.PurpleTeamParticipants[i].ToPlayerInfo();
                playerInfo[num].clientID = num;
            }
            LoLRecorder recorder = StartNewRecoding(info, playNow, false, playerInfo);
        }

        public delegate void PlayGameDelegate(GameInfo info);
        public void PlayGame(GameInfo info)
        {
            if (!LeagueOfLegendsAnalyzer.IsLoLExists())
            {
                if (!LeagueOfLegendsAnalyzer.CheckLoLExeSelected())
                    return;
            }
            StartLoLLauncher(new LoLLauncher(info));
        }

        public void UpdateRecordList()
        {
            if (!Dispatcher.CheckAccess())
            {
                Action refresh = new Action(UpdateRecordList);
                Dispatcher.Invoke(refresh);
            }
            else
            {
                if (IsLoaded)
                {
                    RecordsListBox.Items.Refresh();
                }
            }
        }

        private bool _closeFromIcon = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsLoaded)
                return;
            if (!_closeFromIcon)
            {
                int taskbar = ShringToTaskbar();
                if (taskbar == 0)
                {
                    e.Cancel = true;
                    return;
                }
                else if (taskbar == 1)
                {
                    e.Cancel = true;
                    HideWindow();
                    return;
                }
            }
            if (!CheckRamainRecorder())
                e.Cancel = true;
            if (!e.Cancel)
            {
                PrepareToCloseWindow();
            }
            _closeFromIcon = false;
        }

        private void PrepareToCloseWindow()
        {
            _notifyIcon.Visible = false;
            if (_playFileListener != null)
                _playFileListener.StopListening();
            ClearMainContent();
        }

        private int ShringToTaskbar()
        {
            if (Properties.Settings.Default.AlwaysTaskbar)
                return 1;
            Nullable<Boolean> result = YesNoPopupWindow.AskQuestion(this, Utilities.GetString("ShrinkToTaskBarQuestion"));
            if (result == true)
                return 1;
            else if (result == false)
                return 2;
            return 0;
        }


        public bool CheckRamainRecorder()
        {
            if (Recorders == null)
                return true;
            if (Recorders.Count == 0)
                return true;
            if (YesNoPopupWindow.AskQuestion(this, String.Format(Utilities.GetString("RemainGame") as string, Recorders.Count)) == true)
            {
                foreach (KeyValuePair<LoLRecorder, Thread> pair in Recorders)
                    pair.Value.Abort();
                return true;
            }
            else
            {
                return false;
            }
        }


        public delegate void SwitchMainContentDelegate(UIElement page);
        public void SwitchMainContent(UIElement page)
        {
            if (LockMainView)
                return;
            if (!Dispatcher.CheckAccess())
            {
                SwitchMainContentDelegate dele = new SwitchMainContentDelegate(SwitchMainContent);
                Dispatcher.BeginInvoke(dele, page);
            }
            else
            {
                ClearMainContent();
                MainContent.Children.Add(page);
            }
        }

        private void ClearMainContent()
        {
            if (LockMainView)
                return;
            while (MainContent.Children.Count > 1)
            {
                UIElement ui = MainContent.Children[1];
                MainContent.Children.Remove(ui);
            }
        }


        private void RecordsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (RecordsListBox.SelectedItem == null)
                return;
            SimpleLoLRecord slr = RecordsListBox.SelectedItem as SimpleLoLRecord;
            if (slr.RecoringRecorder != null)
            {
                GameInfoView giv = new GameInfoView();
                giv.DataContext = slr.RecoringRecorder.GameInfo;
                SwitchMainContent(giv);
                return;
            }

            LoLRecord lr = new LoLRecord();
            lr.readFromFile(slr.FileName, false);
            if (lr.HasResult)
                lr.gameStats.DecodeData();
            if (lr.gameStats == null)
            {
                ShowNoData();
            }
            else
            {
                UserControl ui = new RecordDetail();
                ui.DataContext = lr.gameStats;
                SwitchMainContent(ui);
            }
        }

        private void ReadRecordFile(object sender, DoWorkEventArgs e)
        {
            SimpleLoLRecord slr = e.Argument as SimpleLoLRecord;
            LoLRecord lr = new LoLRecord();
            lr.readFromFile((e.Argument as SimpleLoLRecord).FileName, false);
            if (lr.HasResult)
                lr.gameStats.DecodeData();
            e.Result = lr;
        }

        private void ShowNoData()
        {
            SwitchMainContent(new NoData());
        }

        private void Menu_ReplayList(object sender, EventArgs e)
        {
            ClearMainContent();
            RecordsListBox.ItemsSource = replayManager.Records;
            if (RecordsListBox.SelectedIndex != -1)
            {
                RecordsListBox.SelectedIndex = -1;
            }
        }

        private void Menu_Function_Settings_Click(object sender, EventArgs e)
        {
            Settings set = new Settings();
            set.ReplayPathChanged += replayManager.LoadRecordsAsync;
            SwitchMainContent(set);
        }

        private void Menu_Function_FeaturedGames_Click(object sender, EventArgs e)
        {
            FeaturedGamesView fgv = new FeaturedGamesView();
            fgv.FeatureGameClicked += StartRecordFeaturedGame;
            fgv.FeatureGamePlayOnlyClicked += PlayGame;
            SwitchMainContent(fgv);
        }

        private void Menu_Help_Website_Click(object sender, RoutedEventArgs e)
        {
            if (Utilities.NeedGoToEnglishSites())
                System.Diagnostics.Process.Start("https://ahri.tw/en");
            else
                System.Diagnostics.Process.Start("https://ahri.tw/");
        }

        private void Menu_File_Open_Click(object sender, EventArgs e)
        {
            if (LockMainView)
                return;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.DefaultExt = ".lpr";
            dlg.Filter = "BaronReplays Files (*.lpr)|*.lpr";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                StartLoLLauncher(new LoLLauncher(dlg.FileName));
            }
        }

        private async void Menu_Function_Update_Click(object sender, EventArgs e)
        {
            if (LockMainView)
                return;
            if (await Task.Run(() => Utilities.IsUpdateAvailable()))
            {
                UpdateNotifyWindow updateWindow = new UpdateNotifyWindow();
                updateWindow.Owner = this;
                updateWindow.ShowDialog();
                if (updateWindow.IsUpdateAccepted)
                {
                    Utilities.UpdateSoftware();
                }
            }
            else
                PopupWindow.ShowMessage(this, Utilities.GetString("NewestVersion") as string);
        }

        private void ShowNotifyUserMessage(String message)
        {
            if (_notifyIcon.Visible == true) //永遠顯示提示
            {
                _notifyIcon.ShowBalloonTip(10000, Utilities.GetString("BR") as String, message, System.Windows.Forms.ToolTipIcon.None);
            }
        }

        private void Menu_File_AnalyzeCommand_Click(object sender, EventArgs e)
        {
            if (LockMainView)
                return;
            OneLineWindow olw = new OneLineWindow();
            olw.Title = Utilities.GetString("InputCommand") as string;
            olw.Owner = this;
            olw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

            olw.ShowDialog();
            if (olw.DesireString != null)
            {
                TryToPlayCommand(olw.DesireString);
            }
        }

        private void Menu_File_OpenCommandFile_Click(object sender, RoutedEventArgs e)
        {
            if (LockMainView)
                return;
            OpenFileDialog lolFolderSelector = new OpenFileDialog();
            lolFolderSelector.Filter = "All files (*.*)|*.*";
            lolFolderSelector.Title = Utilities.GetString("SelectFile") as string;
            lolFolderSelector.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            lolFolderSelector.ShowDialog();
            if (lolFolderSelector.FileName != string.Empty)
            {
                StreamReader sr = new StreamReader(lolFolderSelector.FileName);
                TryToPlayCommand(sr.ReadToEnd());
            }
        }

        public delegate void TryToPlayCommandDelegate(String command);
        public void TryToPlayCommand(String command)
        {
            if (!Dispatcher.CheckAccess())
            {
                TryToPlayCommandDelegate act = new TryToPlayCommandDelegate(TryToPlayCommand);
                Dispatcher.Invoke(act, command);
                return;
            }
            Logger.Instance.WriteLog(String.Format("Play command line: {0}", command));
            LoLCommandAnalyzer lca = new LoLCommandAnalyzer(command);
            if (lca.IsSuccess)
            {
                Nullable<Boolean> result = YesNoPopupWindow.AskQuestion(this, Utilities.GetString("WatchNow") as string);
                if (result == true)
                    StartNewRecoding(lca.GetGameInfo(), true);
                else if (result == false)
                    StartNewRecoding(lca.GetGameInfo(), false);
            }
            else
            {
                PopupWindow.ShowMessage(this, Utilities.GetString("UselessCommand") as string);
            }
        }

        private void RecordsListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Menu_Function_VideoRecordingMode_Click(object sender, EventArgs e)
        {
            ExportRecord(null);
        }

        private void replayManager_SearchCompleted(ReplayManager rpmanager)
        {
            RecordsListBox.ItemsSource = replayManager.SearchResult;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text.Length == 0)
            {
                RecordsListBox.ItemsSource = replayManager.Records;
            }
            else
            {
                replayManager.SearchAsync(SearchTextBox.Text);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecordsListBox.ItemsSource = replayManager.Records;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (SearchTextBox.IsFocused)
            {
                if (e.Key == Key.Enter)
                    SearchButton_Click(SearchButton, null);
            }
        }

        private void WindowMinimum_Clicked(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void WindowMaximum_Clicked(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
                this.WindowState = System.Windows.WindowState.Normal;
            else
                this.WindowState = System.Windows.WindowState.Maximized;
        }

        private void WindowClose_Clicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FacebookButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://facebook.com/baronrp");
        }

        private void WindowControlBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper h = new WindowInteropHelper(this);
            SendMessage(h.Handle, 0x00A1, 2, 0);
        }

        private void HelpButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Utilities.NeedGoToEnglishSites())
                System.Diagnostics.Process.Start("http://ahri.tw/en/HelpUS");
            else
                System.Diagnostics.Process.Start("http://ahri.tw/HelpUS");
        }

        private void Menu_Language(object sender, EventArgs e)
        {
            SwitchMainContent(new LanguageSettings());
        }

        private Point startPosition;
        private Boolean isSizing;
        private Size originalSize;
        private void Sizer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            originalSize = new Size(this.ActualWidth, this.ActualHeight);
            isSizing = true;
            startPosition = Mouse.GetPosition(null);
            Mouse.Capture(sender as IInputElement, CaptureMode.Element);
        }

        private void Sizer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isSizing = false;
            Mouse.Capture(sender as IInputElement, CaptureMode.None);
        }

        private void Sizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSizing)
            {
                Point endPosition = Mouse.GetPosition(null);
                double x = endPosition.X - startPosition.X;
                double y = endPosition.Y - startPosition.Y;
                this.Width = originalSize.Width + x;
                this.Height = originalSize.Height + y;
            }
        }


        private void Menu_FavoriteReplayList_Clicked(object sender, EventArgs e)
        {
            Menu_ReplayList(null, null);
            replayManager.SearchFavorite();
        }

        private void Menu_Help_About_Click(object sender, EventArgs e)
        {
            SwitchMainContent(new AboutBR());
        }

        private void Menu_Help_FAQ_Click(object sender, EventArgs e)
        {
            if (Utilities.NeedGoToEnglishSites())
                Process.Start("https://ahri.tw/en/FAQ");
            else
                Process.Start("https://ahri.tw/FAQ");
        }

        private void SearchButton_MouseLeftButtonDown(object sender, EventArgs e)
        {
            SearchButton_Click(null, null);
        }

        private void HomeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Utilities.NeedGoToEnglishSites())
                Process.Start("https://ahri.tw/en/");
            else
                Process.Start("https://ahri.tw/");
        }
    }


}
