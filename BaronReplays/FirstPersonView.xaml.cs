using BaronReplays.Database;
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
    /// FirstPersonView.xaml 的互動邏輯
    /// </summary>
    public partial class FirstPersonView : UserControl
    {
        public FirstPersonView()
        {
            InitializeComponent();
        }



    }
    
    public class GoldToKGold : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Double d = System.Convert.ToDouble(value);
            d /= 1000;
            return String.Format("{0:.0}K", d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class KDACalculator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PlayerDBDTO player = (PlayerDBDTO)(value);
            Double result = 0;
            if (player != null)
            {
                Double k = System.Convert.ToDouble(player.K);
                Double d = System.Convert.ToDouble(player.D);
                Double a = System.Convert.ToDouble(player.A);
                result = (k + a) / d;
                if (Double.IsPositiveInfinity(result))
                    return "∞";
            }
            return String.Format("{0:0.0}", result);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
