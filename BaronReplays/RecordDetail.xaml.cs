using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// RecordDetail.xaml 的互動邏輯
    /// </summary>
    public partial class RecordDetail:UserControl
    {
        public RecordDetail()
        {
            InitializeComponent();
        }

    }

    public class GameModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string mode = value as String;
            return Utilities.GetString(mode);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class GameTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value as String;
            return Utilities.GetString(type);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class GameLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UInt32 seconds = (UInt32)value;
            return String.Format(Utilities.GetString("GameLengthFormat").ToString(), (seconds / 60), (seconds % 60));
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TeamToGridImagePath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            UInt32 team = (UInt32)value;
            String imagePath = String.Empty;
            if (team == 100)
                imagePath = "UI/RecordDetail/ScordBoard_Blue.png";
            else if (team == 200)
                imagePath = "UI/RecordDetail/ScordBoard_Purple.png";
            return imagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
