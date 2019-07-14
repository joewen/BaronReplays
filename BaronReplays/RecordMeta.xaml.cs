using System;
using System.Collections.Generic;
using System.IO;
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
    /// RecordMeta.xaml 的互動邏輯
    /// </summary>
    public partial class RecordMeta : UserControl
    {
        public RecordMeta()
        {
            InitializeComponent();
        }

        private void FilePathTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoLRecord record = this.DataContext as LoLRecord;
            String path = record.RelatedFileName;
            if (!File.Exists(path))
            {
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", @"/select, " + path);
        }
    }
}
