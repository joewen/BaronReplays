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
    /// MovieConfiguration.xaml 的互動邏輯
    /// </summary>
    public partial class MovieConfiguration : UserControl
    {
        public event BaronReplays.MainWindow.MovieConfigurationDoneDelegate Done;

        public List<QualityData> qualities = new List<QualityData>   
        { 
            new QualityData(Utilities.GetString("Lowest"), Utilities.GetString("Space30"), false,4000000),
            new QualityData(Utilities.GetString("Low"), Utilities.GetString("Space60"), true,8000000) ,
            new QualityData(Utilities.GetString("Medium"), Utilities.GetString("Space90"), false,12000000) ,
            new QualityData(Utilities.GetString("High"), Utilities.GetString("Space120"), false,16000000) 
        };
        public List<QualityData> Qualities
        {
            get
            {
                return qualities;
            }
        }

        public int Quality
        {
            get
            {
                return (QualityListBox.SelectedItem as QualityData).BitRate;
            }
        }

        public String FileName
        {
            get
            {
                return FileNameBox.Text + ".mp4"; ;
            }
        }

        public Boolean Finish
        {
            get
            {
                if (QualityListBox.SelectedIndex == -1)
                    return false;
                if (FileNameBox.Text.Length == 0)
                    return false;
                return true;
            }
        }

        public MovieConfiguration()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            QualityListBox.SelectedIndex = 1;
        }

        private void FileName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (Char c in e.Text)
            {
                if (Utilities.invaildFileNameChar.Contains(c))
                    e.Handled = true;
            }
        }

        private void Okay_Click(object sender, RoutedEventArgs e)
        {
            if (FileNameBox.Text.Length == 0)
                return;
            if (Done != null)
            {
                Done(this);
            }
        }

        private void FileNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Char c in FileNameBox.Text)
            {
                if (!Utilities.invaildFileNameChar.Contains(c))
                {
                    sb.Append(c);
                }
            }
            FileNameBox.Text = sb.ToString();
        }

    }
}
