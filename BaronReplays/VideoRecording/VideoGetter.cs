using AForge.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BaronReplays.VideoRecording
{
    public class VideoGetter
    {
        private VideoFormat format;
        public VideoFormat Format
        {
            get
            {
                return format;
            }
        }
        private ProcessMemory process;
        private String fileName;
        private VideoFileWriter writer;
        private DateTime firstFrameTime;
        private Boolean stopFlag;
        private int fps;
        public int KeyframeInterval
        {
            get;
            set;

        }
        private TimeSpan videoLength;
        public TimeSpan VideoLength
        {
            get
            {
                return videoLength;
            }
        }
        private List<BitmapSource> keyFrames;
        public List<BitmapSource> KeyFrames
        {
            get
            {
                return keyFrames;
            }
        }


        public int Fps
        {
            get
            {
                return fps;
            }
        }

        public double FrameDuration
        {
            get
            {
                return 1000 / fps;
            }
        }

        private int bitRate;
        public int BitRate
        {
            get
            {
                return bitRate;
            }
            set
            {
                if (value > 400000)
                    bitRate = value;
            }
        }


        public VideoGetter()
        {
            stopFlag = false;
            fps = 30;
            bitRate = 8192000;
            KeyframeInterval = 10;
        }

        public void Start(VideoFormat f, String fileName)
        {
            this.fileName = fileName;
            format = f;
            process = new ProcessMemory();
            process.openProcess(Constants.LoLExeName);
            InitVideoFile();
            StartRecordingLooping();
        }

        public void Stop()
        {
            stopFlag = true;
            process.closeProcess();
        }

        private void StartRecordingLooping()
        {
            Logger.Instance.WriteLog("VideoGetter: Start recording looping");
            keyFrames = new List<BitmapSource>();
            firstFrameTime = DateTime.Now;
            int imageSize = format.Width * format.Height * 4;
            do
            {
                DateTime timeStamp = DateTime.Now;
                TimeSpan elapsed = timeStamp - firstFrameTime;
                byte[] bits = process.readMemory((uint)format.Bits, (uint)imageSize);
                if (bits != null)
                {
                    Bitmap frame = AddFrame(bits, elapsed);
                    if (elapsed.TotalSeconds > keyFrames.Count * KeyframeInterval)
                    {
                        AddKeyFrame(bits);
                    }
                }
                TimeSpan processTime = DateTime.Now - timeStamp;
                double sleepMsec = FrameDuration - processTime.TotalMilliseconds; //FrameDuration 扣掉處理時間
                if (sleepMsec > 0)
                {
                    SpinWait.SpinUntil(() => false, (int)sleepMsec);
                }
            } 
            while (!stopFlag);

            writer.Close();

            videoLength = DateTime.Now - firstFrameTime;
            keyFrames.Add(keyFrames[keyFrames.Count - 1]);
            Logger.Instance.WriteLog("VideoGetter: end recording looping");
        }

        private void AddKeyFrame(byte[] frameData)
        {
            BitmapSource bitmap = BitmapSource.Create(format.Width, format.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr32, null, frameData, format.Width * 4);

            ScaleTransform scale = new ScaleTransform(0.5, 0.5);
            TransformedBitmap scaledBitMap = new TransformedBitmap(bitmap, scale);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(scaledBitMap));

            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);

            ms.Seek(0, SeekOrigin.Begin);
            JpegBitmapDecoder jpg = new JpegBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            ms.Dispose();

            keyFrames.Add(jpg.Frames[0]);
        }


        private void InitVideoFile()
        {
            writer = new VideoFileWriter();
            writer.Open(fileName, format.Width, format.Height, fps, VideoCodec.MPEG4, bitRate);
        }

        private GCHandle frameBuffer;
        private Bitmap AddFrame(byte[] frameData, TimeSpan frameTime)
        {
            frameBuffer = GCHandle.Alloc(frameData, GCHandleType.Pinned);
            IntPtr pointer = frameBuffer.AddrOfPinnedObject();
            Bitmap bitmap = new Bitmap(format.Width, format.Height, format.Width * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, pointer);
            writer.WriteVideoFrame(bitmap, frameTime);
            frameBuffer.Free();
            return bitmap;
        }


    }
}
