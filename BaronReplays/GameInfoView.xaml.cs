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
    /// GameInfoView.xaml 的互動邏輯
    /// </summary>
    public partial class GameInfoView : UserControl
    {
        public GameInfoView()
        {
            InitializeComponent();
        }

        private void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(UrlTextBox.Text);
        }

        private void CopyCommand_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(CommandTextBox.Text);
        }
    }
}
