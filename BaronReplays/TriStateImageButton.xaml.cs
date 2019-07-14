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
    /// TriStateImageButton.xaml 的互動邏輯
    /// </summary>
    public partial class TriStateImageButton : UserControl
    {
        private String imagePath;
        public String ImagePath
        {
            get
            {
                return imagePath;
            }
            set
            {
                imagePath = value;
            }
        }

        public static readonly DependencyProperty LockDownProperty = DependencyProperty.Register("LockDown", typeof(Boolean), typeof(TriStateImageButton), new PropertyMetadata(false));

        public Boolean LockDown
        {
            get { return (Boolean)this.GetValue(LockDownProperty); }
            set
            {
                this.SetValue(LockDownProperty, value);
                if (value)
                    MainImage.Source = Down;
                else
                    MainImage.Source = Normal;
            }
        }




        private CroppedBitmap Normal;
        private CroppedBitmap Hover;
        private CroppedBitmap Down;

        public event EventHandler Clicked;



        public TriStateImageButton()
        {
            InitializeComponent();
            
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {

            
        }


        private void CutImage()
        {
            BitmapImage image = Utilities.GetResourceBitmap(ImagePath);
            if (image == null)
                return;
            int width = Convert.ToInt32(image.PixelWidth) / 3;
            int height = Convert.ToInt32(image.PixelHeight);
            Normal = new CroppedBitmap(image, new Int32Rect(0, 0, width, height));
            Hover = new CroppedBitmap(image, new Int32Rect(width, 0, width, height));
            Down = new CroppedBitmap(image, new Int32Rect(width * 2, 0, width, height));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CutImage();
            MainImage.Source = Normal;
            LockDown = LockDown;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!LockDown)
            {
                MainImage.Source = Hover;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!LockDown)
            {
                MainImage.Source = Normal;
            }
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!LockDown)
            {
                MainImage.Source = Down;
            }
            e.Handled = true;
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!LockDown)
            {
                MainImage.Source = Hover;
            }
            e.Handled = true;
            if (Clicked != null)
            {
                Clicked(this, e);
            }
        }



    }
}
