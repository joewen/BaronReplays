using System;
using System.Windows;

namespace BaronReplays
{
    /// <summary>
    /// UpdateNotifyWindow.xaml 的互動邏輯
    /// </summary>
    public partial class UpdateNotifyWindow : Window
    {
        public UpdateNotifyWindow()
        {
            InitializeComponent();
        }

        public Boolean IsUpdateAccepted
        {
            get;
            set;
        }

        private void DontShowAgain_Click(object sender, RoutedEventArgs e)
        {
            IsUpdateAccepted = false;
            Properties.Settings.Default.NotifyUpdate = false;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void NotNow_Click(object sender, RoutedEventArgs e)
        {
            IsUpdateAccepted = false;
            this.Close();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            IsUpdateAccepted = true;
            this.Close();
        }
    }
}
