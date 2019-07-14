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
    /// YesNoPopupWindow.xaml 的互動邏輯
    /// </summary>
    public partial class YesNoPopupWindow : Window
    {
        public static Nullable<Boolean> AskQuestion(Window owner, String question)
        {
            YesNoPopupWindow askWindow = new YesNoPopupWindow();
            askWindow.Question = question;
            askWindow.Owner = owner;
            askWindow.ShowDialog();
            return askWindow.Agreed;
        }

        public YesNoPopupWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private String question;
        public String Question
        {
            get
            {
                return question;
            }

            set
            {
                question = value;
            }
        }

        private Nullable<Boolean> agreed = null;
        public Nullable<Boolean> Agreed
        {
            get
            {
                return agreed;
            }
            set
            {
                agreed = value;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            agreed = Boolean.Parse(btn.Tag as String);
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
