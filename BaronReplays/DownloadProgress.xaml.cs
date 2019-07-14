using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
    /// DownloadProgress.xaml 的互動邏輯
    /// </summary>
    public partial class DownloadProgress : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private String _url;
        private String _saveLocation;
        private WebClient _downloader;
        private int _percentage;
        public int Percentage
        {
            get
            {
                return _percentage;
            }
            set
            {
                _percentage = value;
                NotifyPropertyChanged("Percentage");
                NotifyPropertyChanged("PercentageString");
            }
        }

        public String PercentageString
        {
            get
            {
                return String.Format("{0}%", Percentage);
            }
        }

        private Nullable<Boolean> _isSuccess = null;
        public Nullable< Boolean> IsSuccess
        {
            get
            {
                return _isSuccess;
            }
        }

        private int _tempPercentage;

        public DownloadProgress(String url, String location)
        {
            InitializeComponent();
            _url = url;
            _saveLocation = location;
        }

        private void StartDownload()
        {
            _downloader = new WebClient();
            _downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
            _downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
            _downloader.DownloadFileAsync(new Uri(_url), _saveLocation);

        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
                _isSuccess = false;
            else
                _isSuccess = true;
            if(!e.Cancelled)
             Close();
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _tempPercentage = e.ProgressPercentage;
            ChangeProgress();
        }

        private void ChangeProgress()
        {
            Percentage = _tempPercentage;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            StartDownload();

        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_isSuccess.HasValue)
            {
                _downloader.CancelAsync();
            }
        }
    }
}
