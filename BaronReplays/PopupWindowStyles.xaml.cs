using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace BaronReplays
{
    public partial class PopupWindowStyles
    {

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wparam, int lparam);
        private void CloseButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((sender as ContentControl).Tag as Window).Close();
        }

        private void Image_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void Image_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void WindowMove(object sender, MouseButtonEventArgs e)
        {

            WindowInteropHelper h = new WindowInteropHelper(sender as Window);
            SendMessage(h.Handle, 0x00A1, 2, 0);
        }
    }
}
