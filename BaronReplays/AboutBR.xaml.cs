using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// AboutBR.xaml 的互動邏輯
    /// </summary>
    public partial class AboutBR : UserControl
    {
        private StringBuilder cheat = new StringBuilder();


        public String VersionString
        {
            get
            {
                return String.Format(Utilities.GetString("AboutVersion") as string, (Utilities.Version / 100) + "." + (Utilities.Version % 100).ToString().PadLeft(2, '0'));
            }
        }
        public AboutBR()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserControl_TextInput(object sender, TextCompositionEventArgs e)
        {
            cheat.Append(e.Text);
            if (cheat.Length > 10)
                cheat.Remove(0, cheat.Length - (cheat.Length - 10));

            string curcmd = cheat.ToString();
            bool hasCmd = true;
            if (curcmd.Contains("crash"))
            {
                throw new Exception("爆炸了");
            }
            else if (curcmd.Contains("update"))
            {
                Utilities.UpdateSoftware();
            }
            else
            {
                hasCmd = false;
            }
            if (hasCmd)
                cheat.Clear();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new WebClient();
                AboutText.Text = Encoding.UTF8.GetString(await client.DownloadDataTaskAsync(new Uri($"{Constants.Website}BaronReplays/About.html")));
            }
            catch (Exception)
            {

            }
            AboutText.Focus();
        }

        private void BugReportButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var reportWindow = new BugReport();
            reportWindow.Owner = MainWindow.Instance;
            reportWindow.ShowDialog();
            Utilities.BugReportString = reportWindow.Message;
        }
    }
}
