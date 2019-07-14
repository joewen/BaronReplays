using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BaronReplays
{
    /// <summary>
    /// RegionSelector.xaml 的互動邏輯
    /// </summary>
    public partial class RegionSelector : Window
    {
        public RegionSelector()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public List<String> regions = null;
        public List<String> Regions
        {
            get
            {
                if (regions == null)
                {
                    regions = new List<String>();
                    foreach (String s in Utilities.Regions)
                        regions.Add(Utilities.GetString(s));
                    regions.Add(Utilities.GetString("Others"));
                }
                return regions;
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wparam, int lparam);

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper h = new WindowInteropHelper(this);
            SendMessage(h.Handle, 0x00A1, 2, 0);
        }


        private void RegionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegionBox.SelectedIndex != RegionBox.Items.Count - 1)
            {
                Properties.Settings.Default.Platform = Utilities.Regions[RegionBox.SelectedIndex];
                Properties.Settings.Default.Save();
            }
            this.Close();
        }

    }
}
