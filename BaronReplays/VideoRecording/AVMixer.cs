using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BaronReplays.VideoRecording
{
    public class AVMixer
    {
        public String VideoInput
        {
            get;
            set;
        }

        public String AudioInput
        {
            get;
            set;
        }

        public String OutputFilePath
        {
            get;
            set;
        }

        private int startInSecond;
        private int durationInSecond;
        private Boolean needToCut;


        public delegate void MixCompletedDelegate(AVMixer sender);
        public MixCompletedDelegate MixCompleted;

        private BackgroundWorker combineWorker;
        public void CombineAsync()
        {
            combineWorker = new BackgroundWorker();
            combineWorker.DoWork += CombineWorker_DoWork;
            combineWorker.RunWorkerCompleted += CombineWorker_RunWorkerCompleted;
            combineWorker.RunWorkerAsync();
        }

        private void CombineWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (MixCompleted != null)
                MixCompleted(this);
        }

        private void CombineWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Combine();
        }

        public void Combine()
        {
            String filePath = OutputFilePath;
            if (File.Exists(filePath))
                File.Delete(filePath);
            Logger.Instance.WriteLog("Starting combine audio and video");
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.FileName = "ffmpeg.exe";
            ffmpeg.StartInfo.Arguments = GetCmdLine();
            ffmpeg.StartInfo.CreateNoWindow = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            //ffmpeg.Exited += ffmpeg_Exited;
            ffmpeg.EnableRaisingEvents = true;
            ffmpeg.Start();
            ffmpeg.WaitForExit();
            Logger.Instance.WriteLog(ffmpeg.StandardError.ReadToEnd());
            Logger.Instance.WriteLog("combine done");
        }

        private String GetCmdLine()
        {
            string cmdLine = null;
            if(needToCut)
                cmdLine = String.Format("-i \"{0}\" -i \"{1}\" -ss {3} -t {4} -c:v copy -c:a libvo_aacenc -ar 44100 -b:a 384k \"{2}\"", VideoInput, AudioInput, OutputFilePath, startInSecond, durationInSecond);
            else
                cmdLine = String.Format("-i \"{0}\" -i \"{1}\" -c:v copy -c:a libvo_aacenc -ar 44100 -b:a 384k \"{2}\"", VideoInput, AudioInput, OutputFilePath);
            Logger.Instance.WriteLog("AVMixer command: " + cmdLine);
            return cmdLine;
        }

        public void SetDuration(int start, int end)
        {
            startInSecond = start;
            durationInSecond = end - start;
            needToCut = true;
        }


        private void ffmpeg_Exited(object sender, EventArgs e)
        {
            //Logger.Instance.WriteLog("ffmpeg finish excuting");
        }

    }
}
