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
    /// OneLineWindow.xaml 的互動邏輯
    /// </summary>
    public partial class OneLineWindow
    {
        public String DesireString;
        public Boolean OkayClick = false;

        public OneLineWindow()
        {
            InitializeComponent();
            
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            OkayClick = true;
            DesireString = OneLineTextBox.Text;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OneLineTextBox.Focus();
        }
    }
}
