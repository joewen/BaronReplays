
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.VideoRecording
{
    public class AudioRecorder
    {
        private String fileName;
        private WasapiLoopbackCapture loopbackCapture;
        private WaveFileWriter writer;
        private bool isRecording = false;
        public bool IsRecording
        {
            get
            {
                return isRecording;
            }
        }


        public void Start(String path)
        {
            fileName = path;
            InitLoopbackCapture();
            InitWriter();
            loopbackCapture.StartRecording();
            isRecording = true;
        }

        private void InitWriter()
        {
            writer = new WaveFileWriter(fileName, loopbackCapture.WaveFormat);
        }

        private void InitLoopbackCapture()
        {
            loopbackCapture = new WasapiLoopbackCapture();
            loopbackCapture.DataAvailable += loopbackCapture_DataAvailable;
            loopbackCapture.ShareMode = NAudio.CoreAudioApi.AudioClientShareMode.Shared;
        }

        public void Stop()
        {
            if (isRecording)
            {
                isRecording = false;
                loopbackCapture.StopRecording();
                writer.Close();
            }
        }

        private void loopbackCapture_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (e.BytesRecorded > 0 && isRecording)
                    writer.Write(e.Buffer, 0, e.BytesRecorded);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
