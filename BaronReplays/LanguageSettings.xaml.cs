using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// LanguageSettings.xaml 的互動邏輯
    /// </summary>
    public partial class LanguageSettings : UserControl
    {
        public LanguageSettings()
        {
            InitializeComponent();
        }


        private void Language_Click(object sender, RoutedEventArgs e)
        {
            LanguageControl.ChangeLanguage((sender as BRButton).Tag as String);
            LanguageList.Items.Refresh();
        }

    }


    public class LanguageCodeToName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String lang = value as String;
            CultureInfo cultureInfo = new CultureInfo(lang);
            return cultureInfo.NativeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsCurrentLanguage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String lang = value as String;
            if (String.Compare(lang, Properties.Settings.Default.Language) == 0)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
