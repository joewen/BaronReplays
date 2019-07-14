using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// BugReport.xaml 的互動邏輯
    /// </summary>
    public partial class BugReport : Window
    {
        public string Message { get; set; } = string.Empty;
        private bool OKClicked { get; set; } = false;

        public BugReport()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            OKClicked = true;
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!OKClicked)
                Message = string.Empty;
        }
    }
}
