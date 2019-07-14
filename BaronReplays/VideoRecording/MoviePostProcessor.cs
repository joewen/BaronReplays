using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace BaronReplays.VideoRecording
{
    public class MoviePostProcessor:DispatcherObject
    {
        public delegate void PostProcessingDoneDelegate(MoviePostProcessor processor, Boolean isSuccess);
        public PostProcessingDoneDelegate PostProcessingDone;

        public event BaronReplays.MainWindow.DoCutMovieDelegate DoCut;

        public MovieRecordingController VideoRecorder
        {
            get;
            set;
        }

        public String FilePath
        {
            get;
            set;
        }

        public Window ParentWindow
        {
            get;
            set;
        }

        private MovieCutter cutter;

        public void OnRecordingDone(MovieRecordingController controller, Boolean isSuccess, String message)
        { 
            if (!Dispatcher.CheckAccess())
            {
                MovieRecordingController.DoneDelegate prd = new MovieRecordingController.DoneDelegate(OnRecordingDone);
                Dispatcher.Invoke(prd, controller, isSuccess, message);
            }
            else
            {
                if (isSuccess)
                {
                    CreateVideoCutter();
                }
                else
                {
                    PostProcessingDone(this, false);
                }
            }
        }

        private void CreateVideoCutter()
        {
            Logger.Instance.WriteLog("MoviePostProcessor: Create video cutter");
            cutter = new MovieCutter();
            cutter.Frames = VideoRecorder.MovieGetter.KeyFrames;
            cutter.FrameInterval = VideoRecorder.MovieGetter.KeyframeInterval;
            cutter.MovieLength = VideoRecorder.MovieGetter.VideoLength;
            cutter.PiciureFormat = VideoRecorder.MovieGetter.Format;
            DoCut(this, cutter);
        }

        public Boolean CombineVideoAndAudio()
        {
            Logger.Instance.WriteLog("MoviePostProcessor: Start combine audio and video");
            AVMixer mixer = new AVMixer();
            mixer.VideoInput = VideoRecorder.VideoPath;
            mixer.AudioInput = VideoRecorder.AudioPath;
            mixer.OutputFilePath = VideoRecorder.OutputFilePath;
            mixer.MixCompleted += new AVMixer.MixCompletedDelegate(OnMixCompleted);
            if (cutter.Cutted)
            {
                mixer.SetDuration(cutter.StartTime, cutter.EndTime);
            }
            mixer.CombineAsync();
            return true;
        }


        private void OnMixCompleted(AVMixer sender)
        {
            Logger.Instance.WriteLog("MoviePostProcessor: finished");
            if (File.Exists(VideoRecorder.OutputFilePath))
            {
                FilePath = VideoRecorder.OutputFilePath;
                PostProcessingDone(this, true);
            }
            else
            {
                PostProcessingDone(this, false);
            }
        }
    }
}
