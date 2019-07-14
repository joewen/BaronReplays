using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace BaronReplays
{
    public class LoLLauncher
    {
        private LoLRecord _record;
        private String _exePath;
        private String _args;
        private bool _localPlay;
        private bool _checkVersion = false;
        private LoLRecordPlayer _recordPlayer;
        private Thread _playerThread;
        private LoLRecorder _recoder;
        public MainWindow.LoLLauncherDoneDelegate AnyEvent;
        public Window Parent;
        private DispatcherTimer LoLWatchTimer;
        private GameInfo _gameInfo;
        private bool _cameraToolInjected;
        private Process lol;

        private void InitLoLWatchTimer()
        {
            LoLWatchTimer = new DispatcherTimer();
            LoLWatchTimer.Interval = TimeSpan.FromMilliseconds(500);   //500ms
            LoLWatchTimer.Tick += new EventHandler(LoLWatchTimer_Tick);
            LoLWatchTimer.Start();
        }

        private void LoLWatchTimer_Tick(object sender, EventArgs args)
        {
            if (!LeagueOfLegendsAnalyzer.IsLoLExists())
            {
                LoLWatchTimer.Stop();
                if (_recordPlayer != null)
                    _recordPlayer.StopPlaying();
                if (AnyEvent != null)
                    AnyEvent(true, null);
            }
            else
            {   //
                //if (!_cameraToolInjected && Properties.Settings.Default.EnableCameraRotation)
                //{
                //    ProcessMemory processMemory = new ProcessMemory();
                //    processMemory.openProcess(lol);
                //    _cameraToolInjected = processMemory.injectDll(Constants.CameraToolDllName);
                //    processMemory.closeProcess();
                //}
            }
        }


        public LoLLauncher(LoLRecorder recordingRecorder)
        {
            _recoder = recordingRecorder;
            _record = recordingRecorder.record;
            _localPlay = true;
        }

        public LoLLauncher(SimpleLoLRecord simpleRecord)
        {
            if (simpleRecord.RecoringRecorder == null)
            {
                _record = new LoLRecord();
                _record.readFromFile(simpleRecord.FileName, false);
                _checkVersion = true;
            }
            else
                _record = simpleRecord.RecoringRecorder.record;
            _localPlay = true;
        }

        public LoLLauncher(String recordFilePath)
        {
            _record = new LoLRecord();
            _record.readFromFile(recordFilePath, false);
            _localPlay = true;
            _checkVersion = true;
        }


        public LoLLauncher(GameInfo info)
        {
            _gameInfo = info;
            _localPlay = false;
        }

        private void CreateLoLRecordPlayer()
        {
            _recordPlayer = new LoLRecordPlayer(_record);
            if (_recoder != null)
                _recordPlayer.useAdvanceReplay = true;
            ThreadStart playfunc = new ThreadStart(_recordPlayer.startPlaying);
            _playerThread = new Thread(playfunc);
            _playerThread.Name = "Player";
            _playerThread.IsBackground = true;
            _playerThread.Start();
        }

        private void MakeGameInfo()
        {
            Logger.Instance.WriteLog("Create game information.");
            if (_gameInfo == null)
            {
                if (_localPlay)
                {

                    _gameInfo = new GameInfo("127.0.0.1:" + _recordPlayer.Port, new String(_record.gamePlatform), _record.gameId, new string(_record.observerEncryptionKey));
                }
                else
                    _gameInfo = new GameInfo(_recoder.platformAddress, new String(_record.gamePlatform), _record.gameId, new string(_record.observerEncryptionKey));
            }
        }

        public void StartPlaying()
        {
            _exePath = Properties.Settings.Default.LoLGameExe;
            Logger.Instance.WriteLog(String.Format("LoL executing path is: {0}", _exePath));

            if (_localPlay)
            {
                if (_record.IsBroken)
                {
                    if (AnyEvent != null)
                        AnyEvent(false, "BrokenFile");
                    return;
                }
            }

            if (_checkVersion)
            {
                if (!GetAppropriateLoLExe())
                {
                    if (AnyEvent != null)
                        AnyEvent(false, "OldVersionReplay");
                    return;
                }
            }

            if (_localPlay)
                CreateLoLRecordPlayer();
            MakeGameInfo();
            SetArgs();
            ExecuteLoLExe();
            InitLoLWatchTimer();
        }

        private void SetArgs()
        {
            Logger.Instance.WriteLog("Create command line parameters.");
            _args = "60000 BR.exe Air\\LoLClient.exe \"spectator " + _gameInfo.ServerAddress + " " + _gameInfo.ObKey + " " + _gameInfo.GameId + " " + _gameInfo.PlatformId + "\"";
            Logger.Instance.WriteLog(_args);
            //if (_localPlay)
            //    _args = "60000 BR.exe Air\\BR.exe \"spectator 127.0.0.1:8080 " + new string(_record.observerEncryptionKey) + " " + _record.gameId + " " + new string(_record.gamePlatform) + "\"";
            //else
            //    _args = "60000 BR.exe Air\\BR.exe \"spectator " + _recoder.platformAddress+ " " + new string(_record.observerEncryptionKey) + " " + _record.gameId + " " + new string(_record.gamePlatform) + "\"";
        }

        private void ExecuteLoLExe()
        {
            Logger.Instance.WriteLog("Launch League of Legends.exe");
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.Arguments = _args;
            psi.FileName = _exePath;
            psi.WorkingDirectory = Properties.Settings.Default.LoLGameExe.Remove(Properties.Settings.Default.LoLGameExe.LastIndexOf('\\'));
            lol = Process.Start(psi);
            Logger.Instance.WriteLog("League of Legends.exe is started.");
        }

        private bool GetAppropriateLoLExe()
        {
            String nowSimpleVer = null;
            Logger.Instance.WriteLog("Get appropriate LoL version.");
            try
            {
                string nowExeVer = FileVersionInfo.GetVersionInfo(_exePath).ProductVersion;
                Logger.Instance.WriteLog(String.Format("LoL version is: {0}", nowExeVer));
                nowSimpleVer = Utilities.GetSimpleVersionNumber(nowExeVer);
                Logger.Instance.WriteLog(String.Format("LoL short version is: {0}", nowSimpleVer));
            }
            catch
            {
                Logger.Instance.WriteLog("Wrong League of Legends.exe");
                LeagueOfLegendsAnalyzer.ResetLoLExePath();
                return false;
            }

            String targetVer = null;
            //String spVer = null;
            try
            {
                if (_localPlay)
                {
                    targetVer = Utilities.GetSimpleVersionNumber(new String(_record.lolVersion));
                    if (String.Compare(targetVer, nowSimpleVer) != 0)
                    {
                        return false;
                    }
                }
                else
                    targetVer = nowSimpleVer;

            }
            catch
            {
                return false;
            }

            return true;
        }

    }
}
