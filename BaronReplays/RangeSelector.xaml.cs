using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BaronReplays
{
    /// <summary>
    /// RangeSelector.xaml 的互動邏輯
    /// </summary>
    public partial class RangeSelector : UserControl
    {
        private double minValue;
        public double MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = value;
            }
        }

        private double maxValue;
        public double MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = value;
            }
        }

        private double TotalRange
        {
            get
            {
                return maxValue - minValue;
            }
        }

        private double LowerCurrentPosition
        {
            get
            {
                return LeftSlider.Margin.Left - 5;
            }
        }

        public double LowerCurrentValue
        {
            get
            {
                return (LowerCurrentPosition / Line.ActualWidth) * TotalRange + minValue;
            }
        }

        public double UpperCurrentPosition
        {
            get
            {
                return Line.ActualWidth - (RightSlider.Margin.Right - 5);
            }
        }

        public double UpperCurrentValue
        {
            get
            {
                return (UpperCurrentPosition / Line.ActualWidth) * TotalRange + minValue;
            }
        }


        public RangeSelector()
        {
            minValue = 0;
            maxValue = 100;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private Boolean mouseDown;
        private Point mouseDownPosition;
        private Image dragingRect;
        private Thickness oriMargin;
        public delegate void SlideEventDelegate(RangeSelector sender, double LowerCurrentValue, double UpperCurrentValue);
        public SlideEventDelegate SlideEvent;
        public int DraggingBar
        {
            get
            {
                if (dragingRect == LeftSlider)
                    return -1;
                else if (dragingRect == RightSlider)
                    return 0;
                else return 1;
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.MouseMove += RangeSelector_MouseMove;
            mouseDown = true;
            mouseDownPosition = e.GetPosition(this);
            dragingRect = sender as Image;
            oriMargin = dragingRect.Margin;
            Mouse.Capture(this, CaptureMode.Element);
        }

        private void RangeSelector_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                double diff = e.GetPosition(this).X - mouseDownPosition.X;
                double targetMargin = 5;
                Thickness newMargin;
                bool inRange = false;
                double barMinDist = LeftSlider.ActualWidth + RightSlider.ActualWidth;
                if (dragingRect == LeftSlider)
                {

                    if (oriMargin.Left + diff >= 0)
                        targetMargin = oriMargin.Left + diff;

                    newMargin = new Thickness(targetMargin, oriMargin.Top, oriMargin.Right, oriMargin.Bottom);
                    if (newMargin.Left - 5 < UpperCurrentPosition - 10 && (targetMargin + RightSlider.Margin.Right < this.ActualWidth - barMinDist))
                        inRange = true;
                }
                else
                {
                    diff *= -1;
                    if (oriMargin.Right + diff >= 0)
                        targetMargin = oriMargin.Right + diff;

                    newMargin = new Thickness(oriMargin.Left, oriMargin.Top, targetMargin, oriMargin.Bottom);
                    if (Line.ActualWidth - (newMargin.Right - 5) > LowerCurrentPosition + 10 && (targetMargin + LeftSlider.Margin.Left < this.ActualWidth - barMinDist))
                        inRange = true;

                }

                if (inRange)
                {
                    dragingRect.Margin = newMargin;
                    if (SlideEvent != null)
                        SlideEvent(this, LowerCurrentValue, UpperCurrentValue);
                }
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this, CaptureMode.None);
            mouseDown = false;
        }


    }
}
