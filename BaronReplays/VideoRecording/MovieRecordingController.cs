using BaronReplays;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaronReplays.VideoRecording
{
    public class MovieRecordingController: ProcedureTask
    {
        private Thread recordingThread;
        public Thread RecordingThread
        {
            get
            {
                return recordingThread;
            }
        }

        private VideoFormat videoFormat;
        private VideoGetter videoGetter;
        public VideoGetter MovieGetter
        {
            get
            {
                return videoGetter;
            }
        }
        private Boolean cancelRequested;
        public delegate void DoneDelegate(MovieRecordingController controller, Boolean isSuccess, String message);
        public DoneDelegate DoneEvent;
        private AudioRecorder audioRecoder;
        private String pathPrefix;
        private String videoPath;
        public String VideoPath
        { 
            get
            {
                return videoPath;
            }
        }
        private String audioPath;
        public String AudioPath
        {
            get
            {
                return audioPath;
            }
        }
       
        public int Quality
        {
            get;
            set;
        }

        private String outputFileName;
        public String OutputFileName
        {
            get
            {
                return outputFileName;
            }
            set 
            {
                outputFileName = value;
                OutputFilePath = BaronReplays.Properties.Settings.Default.MovieDir + "\\" + outputFileName;
            }
        }

        public String OutputFilePath
        {
            get;
            set;
        }

        private bool lolStarted;
        public bool LoLStarted
        {
            get
            {
                return lolStarted;
            }
        }

        private void OnError()
        {
            throw new NotImplementedException();
        }

        public MovieRecordingController()
            : base()
        {
            String tempFilePath = "BRtemp-" + DateTime.Now.ToString("yyyy-MM-ddThh-mm-ss");
            String tempPath = Path.GetTempPath();
            pathPrefix = tempPath + tempFilePath;
            videoPath = pathPrefix + ".mp4";
            audioPath = pathPrefix + ".wav";
            OutputFileName = "default.mp4";
            cancelRequested = false;
            AddTasks();
        }

        public void AddTasks()
        {
            AddTask(new Task(WaitForLoLStart));
            AddTask(new Task(WaitForLoLLoadingFinish));
            AddTask(new Task(InjectVideoSendingDll));
            AddTask(new Task(GetVideoFormat));
            AddTask(new Task(GetAudio));
            AddTask(new Task(GetVideo));
            //AddTask(new Task(CombineVideoAndAudio));
            TaskDoneEvent += new TaskResult(OnTaskDone);
        }

        ~MovieRecordingController()
        {
            try
            {
                audioRecoder.Stop();
                videoGetter.Stop();
                File.Delete(videoPath);
                File.Delete(audioPath);
            }
            catch (Exception e)
            {

            }
        }

        private void OnTaskDone(bool isSuccess, String msg)
        {
            DoneEvent(this,isSuccess,msg);
        }

        private Boolean GetAudio()
        {
            audioRecoder = new AudioRecorder();
            audioRecoder.Start(audioPath);
            return true;
        }

        private Boolean InjectVideoSendingDll()
        {
            Logger.Instance.WriteLog("MovieRecordingController: Inject capture dll.");
            ProcessMemory processMemory = new ProcessMemory();
            processMemory.openProcess(Constants.LoLExeName);
            Boolean dllInjectSuccess = processMemory.injectDll(Constants.VideoCapturerDllName);
            processMemory.closeProcess();
            return dllInjectSuccess;
        }

        private bool WaitForLoLStart()
        {
            Logger.Instance.WriteLog("MovieRecordingController: Wait for LoL starts.");
            while (!LeagueOfLegendsAnalyzer.IsLoLExists() && !cancelRequested)
            {
                SpinWait.SpinUntil(() => false, 1000);
            }
            return !cancelRequested;
        }

        private bool WaitForLoLLoadingFinish()
        {
            lolStarted = true;
            Logger.Instance.WriteLog("MovieRecordingController: Wait for LoL loading finish.");
            SpinWait.SpinUntil(() => false, 5000);
            String logContent = null;
            do
            {
                logContent = LoLWatcher.Instance.ExecutionData.LogContent;
                SpinWait.SpinUntil(() => false, 1000);
            }
            while (!LoLWatcher.Instance.ExecutionData.LogContent.Contains("Start Main Loop") && !cancelRequested);
            return !cancelRequested;
        }

        private Boolean GetVideo()
        {
            Logger.Instance.WriteLog("MovieRecordingController: Start to get video");
            videoGetter = new VideoGetter();
            videoGetter.BitRate = Quality;
            videoGetter.Start(videoFormat, videoPath);
            return true;
        }


        public void StartRecording()
        {
            Logger.Instance.WriteLog("MovieRecordingController: Start");
            recordingThread = Thread.CurrentThread;
            RunTasks();
        }

        public void StopRecording()
        {
            Logger.Instance.WriteLog("MovieRecordingController: Stop requested");
            if (videoGetter != null)
                videoGetter.Stop();
            if (audioRecoder != null)
                audioRecoder.Stop();
            cancelRequested = true;
        }

        public Boolean GetVideoFormat()
        {
            Logger.Instance.WriteLog("MovieRecordingController: Get video format");
            VideoFormatGetter formatGetter = new VideoFormatGetter();
            videoFormat = formatGetter.GetVideoFormat();
            return (videoFormat != null);
        }
    }
}
