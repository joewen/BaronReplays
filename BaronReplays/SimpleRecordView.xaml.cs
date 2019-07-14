using BaronReplays.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// SimpleRecordView.xaml 的互動邏輯
    /// </summary>
    public partial class SimpleRecordView : UserControl
    {
        public MainWindow.RecordDelegate WatchClickEvent;

        public SimpleRecordView()
        {
            InitializeComponent();
        }

        private void WatchButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as SimpleLoLRecord).PlayRecord(); 
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            Nullable<bool> sure = YesNoPopupWindow.AskQuestion(MainWindow.Instance, String.Format(Utilities.GetString("SureDelete") as string, (this.DataContext as SimpleLoLRecord).FileNameShort));
            if (sure == true)
            {
                (DataContext as SimpleLoLRecord).DeleteRecord();
            }
        }

        private void HistoryIcon_Click(object sender, EventArgs e)
        {
            (DataContext as SimpleLoLRecord).OpenMatchHistory();
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            (DataContext as SimpleLoLRecord).ShowRenameDialog();
        }

        private void Menu_FileMeta_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as SimpleLoLRecord).ShowMeta();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as SimpleLoLRecord).ExportRecord();
        }

        private void FavoriteIcon_Clicked(object sender, EventArgs e)
        {
            SimpleLoLRecord slr = this.DataContext as SimpleLoLRecord;
            slr.Favorite = !slr.Favorite;
            FavoriteIcon.LockDown = slr.Favorite;
        }
    }

    public class GetListItemBackground : IMultiValueConverter
    {
        public static SolidColorBrush[] Backgrounds = new SolidColorBrush[] { new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)), new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) };
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ListBoxItem item = values[0] as ListBoxItem;
            ListBox listBox = values[1] as ListBox;
            int id = listBox.ItemContainerGenerator.IndexFromContainer(item);
            return Backgrounds[id % Backgrounds.Length];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class WinTeamColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if ((Boolean)value)
                return Constants.WinColor;
            return Constants.LoseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LeftColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = Constants.UnknowColor;
            try
            {
                PlayerDBDTO player = (PlayerDBDTO)values[0];
                GameDBDTO info = (GameDBDTO)values[1];
                if (player != null && info != null)
                {
                    if (player.Team == info.WinTeam)
                        return result = Constants.WinColor;
                    else
                        return result = Constants.LoseColor;
                }
            }
            catch (Exception e)
            {
            }
            return result;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class SimpleVersionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String shortver = String.Empty;
            try
            {
                char[] sep = { '.' };
                String[] vers = (value as String).Split(sep);
                if (vers.Length < 2)
                    return string.Empty;
                else
                    shortver = String.Format(Utilities.GetString("AboutVersion"), String.Format("{0}.{1}", vers[0], vers[1]));
            }
            catch (Exception e)
            {

            }

            return shortver;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
