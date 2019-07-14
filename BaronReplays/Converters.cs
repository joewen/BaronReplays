using BaronReplays.LoLStaticData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BaronReplays
{

    public class StringToChampionIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            if ((value as string).Length == 0)
                return null;
            if ((value as string).Contains("Unknow Champion"))
                return null;
            return Champion.Instance.GetImageByKey(value as String);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemNumberToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Item.Instance.GetImageById(System.Convert.ToInt32(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //public class ItemNumberToIcon : IValueConverter
    //{
    //    public static String ItemsDir = @"Items\";
    //    public static String ItemsDirFullPath = AppDomain.CurrentDomain.BaseDirectory + ItemsDir;
    //    public static Object Lock = new Object();
    //    public static Dictionary<String, ImageSource> ItemNumberToBitmapImage;

    //    public static void CheckItemDir()
    //    {
    //        Utilities.IfDirectoryNotExitThenCreate(ItemsDirFullPath);
    //        string[] l = Directory.GetFiles(ItemsDirFullPath, "*.png");
    //        ItemNumberToBitmapImage = new Dictionary<string, ImageSource>();
    //        foreach (string s in l)
    //        {
    //            try
    //            {
    //                ItemNumberToBitmapImage.Add(s.Substring(s.LastIndexOf('\\') + 1), Utilities.GetBitmapImage(s));
    //            }
    //            catch
    //            {
    //                if (File.Exists(s))
    //                    File.Delete(s);
    //            }
    //        }
    //    }

    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        UInt32 number = (UInt32)System.Convert.ChangeType(value, typeof(UInt32));
    //        if (number == 0)
    //            return null;
    //        string imageName = number + ".png";
    //        string prefix = String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/item/", Properties.Settings.Default.NAVersion);
    //        lock (Lock)
    //        {
    //            if (!ItemNumberToBitmapImage.ContainsKey(imageName))
    //            {
    //                try
    //                {
    //                    using (WebClient wc = new WebClient())
    //                    {
    //                        //wc.DownloadFile("http://quickfind.kassad.in/assets/legendary/img/item/" + imageName, ItemsDir + imageName);
    //                        //wc.DownloadFile("https://sites.google.com/site/ahritw/" + imageName, ItemsDir + imageName);
    //                        wc.DownloadFile(prefix + imageName, ItemsDir + imageName);
    //                    }
    //                }
    //                catch
    //                {
    //                    return null;
    //                }
    //                ItemNumberToBitmapImage.Add(imageName, Utilities.GetBitmapImage(ItemsDirFullPath + imageName));
    //            }
    //        }
    //        return ItemNumberToBitmapImage[imageName];
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


    public class SpellNumberToSpellIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return SummonerSpell.Instance.GetImageById((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class TeamToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            UInt32 team = (UInt32)value;
            SolidColorBrush brush = Constants.UnknowColor;
            if (team == 100)
                brush = Constants.Blue;
            else if (team == 200)
                brush = Constants.Purple;
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MapToImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String map = value as String;
            return @"Maps\" + map + ".jpg";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ChampionIdToChampion : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int i = (int)(value);
            return Champion.Instance.GetNameByKey(Champion.Instance.GetKeyById(i));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ChampionIdToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Champion.Instance.GetImageByKey(Champion.Instance.GetKeyById((int)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RegionToName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return String.Empty;
            List<string> ls = value as List<string>;
            List<String> newlist = new List<string>();
            foreach (String s in ls)
            {
                newlist.Add(Utilities.GetString(s) as string);
            }

            return newlist;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PlatformIdToName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            return Utilities.GetString(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityMutex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            Visibility v = (Visibility)value;
            if (v == Visibility.Visible)
                return Visibility.Hidden;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ArrayLengthToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Object[] objArray = (Object[])value;
            if (objArray.Length != 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if ((Boolean)value)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BorderClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double && values[1] is double && values[2] is CornerRadius)
            {
                var width = (double)values[0];
                var height = (double)values[1];

                if (width < Double.Epsilon || height < Double.Epsilon)
                {
                    return Geometry.Empty;
                }

                var radius = (CornerRadius)values[2];

                // Actually we need more complex geometry, when CornerRadius has different values.
                // But let me not to take this into account, and simplify example for a common value.
                var clip = new RectangleGeometry(new Rect(0, 0, width, height), radius.TopLeft, radius.TopLeft);
                clip.Freeze();

                return clip;
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    public class MsecToStr : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            UInt32 sec = (UInt32)value / 1000;
            return String.Format("{0:D}:{1:D2}", sec / 60, sec % 60);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
