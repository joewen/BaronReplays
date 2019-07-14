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
using System.Windows.Shapes;

namespace BaronReplays
{
    /// <summary>
    /// PopupWindow.xaml 的互動邏輯
    /// </summary>
    public partial class PopupWindow : Window
    {


        private String message;
        public String Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }

        public PopupWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public static void ShowMessage(Window owner, String message)
        {
            PopupWindow window = new PopupWindow();
            window.Message = message;
            window.Owner = owner;
            window.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                Owner.Activate();
            }
        }

    }
}
