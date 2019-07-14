using BaronReplays.VideoRecording;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BaronReplays
{
    /// <summary>
    /// MovieCutter.xaml 的互動邏輯
    /// </summary>
    public partial class MovieCutter : UserControl
    {
        public event BaronReplays.MainWindow.CuttingDownDelegate CuttingDown;
        public List<BitmapSource> Frames
        {
            get;
            set;
        }

        public int FrameInterval
        {
            get;
            set;
        }

        public TimeSpan MovieLength
        {
            get;
            set;
        }

        public VideoFormat PiciureFormat
        {
            get;
            set;
        }

        private Boolean cutted;
        public Boolean Cutted
        {
            get { return cutted; }
        }

        private Boolean finished;
        public Boolean Finished
        {
            get { return finished; }
        }

        public MovieCutter()
        {
            finished = false;
            cutted = false;
            InitializeComponent();
        }

        public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("Bitmap is null");

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private double start;
        public int StartTime
        {
            get
            {
                return (int)(start);
            }
        }

        private double end;
        public int EndTime
        {
            get
            {
                return (int)(end);
            }
        }


        private int pictureWidth;
        private int pictureHeight;

        private void InitSelectcor()
        {
            Selectcor.MaxValue = MovieLength.TotalSeconds;
            Selectcor.MinValue = 0;
            Selectcor.SlideEvent += new RangeSelector.SlideEventDelegate(OnSelectorSlidedRangeSelector);
        }

        private void InitPictureSize()
        {
            pictureWidth = 640;
            pictureHeight = pictureWidth / 16 * (int)(PiciureFormat.GetResolutionRatio);
        }
        private void InitTimeValue()
        {
            start = 0;
            startFrame = 0;
            end = MovieLength.TotalSeconds;
            endFrame = (int)Math.Round((MovieLength.TotalSeconds / FrameInterval));
            StartTimeTextBlock.Text = StartTimeString;
            EndTimeTextBlock.Text = EndTimeString;
        }

        private void InitImageUI()
        {
            LeftImage.Source = Frames[0];
            RightImage.Source = Frames[Frames.Count - 1];
            ChangeDragging(LeftImage);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Frames == null)
                return;
            if (FrameInterval == 0)
                return;
            LeftImage = new System.Windows.Controls.Image();
            RightImage = new System.Windows.Controls.Image();

            InitPictureSize();
            InitSelectcor();
            InitTimeValue();
            InitImageUI();
        }

        private void ChangeDragging(System.Windows.Controls.Image current)
        {
            ImageContainer.Children.Clear();
            ImageContainer.Children.Add(current);
        }


        private int startFrame;
        private int endFrame;

        public String StartTimeString
        {
            get
            {
                return String.Format("{0:00}:{1:00}", StartTime / 60, StartTime % 60);
            }
        }

        public String EndTimeString
        {
            get
            {
                return String.Format("{0:00}:{1:00}", EndTime / 60, EndTime % 60);
            }
        }

        private System.Windows.Controls.Image LeftImage;
        private System.Windows.Controls.Image RightImage;

        private void OnSelectorSlidedRangeSelector(RangeSelector sender, double LowerCurrentValue, double UpperCurrentValue)
        {
            if ( start != LowerCurrentValue)
            {
                ChangeDragging(LeftImage);
                //OnDraggingStart();
            }
            if ( end != UpperCurrentValue)
            {
                ChangeDragging(RightImage);
                //OnDraggingEnd();
            }
            start = LowerCurrentValue;
            end = UpperCurrentValue;
            StartTimeTextBlock.Text = StartTimeString;
            EndTimeTextBlock.Text = EndTimeString;
            cutted = true;

            try
            {
                int newStart = (int)Math.Round((LowerCurrentValue / FrameInterval));
                int newEnd = (int)Math.Round((UpperCurrentValue / FrameInterval));
                if (newStart != startFrame)
                {
                    LeftImage.Source = Frames[newStart];
                    startFrame = newStart;
                }
                if (newEnd != endFrame)
                {
                    RightImage.Source = Frames[newEnd];
                    endFrame = newEnd;
                }
            }
            catch (Exception e)
            {

            }
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            CuttingDown();
        }


    }
}
