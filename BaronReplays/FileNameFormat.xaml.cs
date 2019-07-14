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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace BaronReplays
{
    /// <summary>
    /// FileNameFormat.xaml 的互動邏輯
    /// </summary>
    public partial class FileNameFormat : UserControl
    {
        private static List<String> TagTypes = new List<string>
        { 
            "SummonerName",
            "Champion",
            "Year",
            "Month",
            "Day",
            "Hour",
            "Minute",
            "Second",
            "Kill",
            "Death",
            "Assist",
            "KDA",
            "WinLose",
            "Map",
        };

        private static List<String> SymbolTypes = new List<string>
        { 
            " ",
            "-",
            "~"
        };

        private Dictionary<Button, String> _tagToType;
        private Dictionary<String, String> _namesToExample;

        public FileNameFormat()
        {
            InitializeComponent();
            _clickDragTimer = new DispatcherTimer();
            _clickDragTimer.Interval = TimeSpan.FromMilliseconds(200);
            _clickDragTimer.Tick += _clickDragTimer_Tick;
            CreateTags();
            CreateExamples();

        }

        private void CreateTags()
        {
            _tagToType = new Dictionary<Button, string>();
            foreach (String s in TagTypes)
            {
                CreateButton(s, false);
            }

            foreach (String s in SymbolTypes)
            {
                CreateButton(s, true);
            }

        }

        private Button CreateButton(String s, bool isSymbol)
        {
            Button tsi = new Button();
            if (!isSymbol)
            {
                tsi.Content = Utilities.GetString(s) as String;
                tsi.PreviewMouseRightButtonUp += CancelFromNames;
                _tagToType.Add(tsi, s);
            }
            else
            {
                tsi.Content = s;
                tsi.PreviewMouseRightButtonUp += CancelSymbol;
            }
            tsi.Style = (Style)FindResource("PropertyButtonStyle");
            SourcePanel.Children.Add(tsi);
            return tsi;
        }

        private DateTime _mouseDownTime;
        private DispatcherTimer _clickDragTimer;
        private Button _draggingItem;
        private void tsi_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDownTime = DateTime.Now;
            _draggingItem = sender as Button;
            _clickDragTimer.Start();
        }

        private void tsi_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TimeSpan passTime = DateTime.Now - _mouseDownTime;
            if (passTime.TotalMilliseconds < 200)
            {
                AddToNamePanel(sender as Button);
                _clickDragTimer.Stop();
            }

        }

        private void _clickDragTimer_Tick(object sender, EventArgs e)
        {
            _clickDragTimer.Stop();
            DragStart(_draggingItem, null);
        }


        private void AddToNamePanel(Button button)
        {
            if (NamePanel.Children.Contains(button))
            {
                NamePanel.Children.Remove(button);
                if (!SymbolTypes.Contains(button.Content))
                {
                    SourcePanel.Children.Add(button);
                }
                
            }
            else
            {
                SourcePanel.Children.Remove(button);
                NamePanel.Children.Add(button);
                if (SymbolTypes.Contains(button.Content))
                {
                    CreateButton(button.Content as String, true);
                }
            }
            RefreshExample();

        }

        private void DragStart(object sender, MouseButtonEventArgs e)
        {
            DataObject data = new DataObject();
            data.SetData("Object", sender);
            DragDrop.DoDragDrop(this, data, DragDropEffects.All);
        }

        private void CreateExamples()
        {
            DateTime dt = DateTime.Now;
            _namesToExample = new Dictionary<string, string>();
            _namesToExample.Add("SummonerName", Utilities.GetString("SummonerName") as String);
            _namesToExample.Add("Champion", "Ahri");
            _namesToExample.Add("Year", dt.Year.ToString());
            _namesToExample.Add("Month", dt.Month.ToString());
            _namesToExample.Add("Day", dt.Day.ToString());
            _namesToExample.Add("Hour", dt.Hour.ToString());
            _namesToExample.Add("Minute", dt.Minute.ToString());
            _namesToExample.Add("Second", dt.Second.ToString());
            _namesToExample.Add("Kill", "8");
            _namesToExample.Add("Death", "3");
            _namesToExample.Add("Assist", "2");
            _namesToExample.Add("KDA", "3.3");
            _namesToExample.Add("WinLose", Utilities.GetString("Win") as String);
            _namesToExample.Add("Map", Utilities.GetString("CLASSIC") as String);
        }








        private void RefreshExample()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Object obj in NamePanel.Children)
            {
                Button tag = obj as Button;
                //sb.Append(SplitSymbol.Text);
                if (_tagToType.ContainsKey(tag))
                {
                    String type = _tagToType[tag];
                    sb.Append(_namesToExample[type]);
                }
                else

                    sb.Append(tag.Content);
            }
            sb.Append(".lpr");
            //if (NamePanel.Children.Count != 0)
            //{
            //    sb.Remove(0, SplitSymbol.Text.Length);
            //}
            if (FileNameExample != null)
                FileNameExample.Text = sb.ToString();
        }

        private void SaveFormat()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Object obj in NamePanel.Children)
            {
                Button tag = obj as Button;
                //sb.Append(SplitSymbol.Text);
                if (_tagToType.ContainsKey(tag))
                    sb.AppendFormat("<{0}>", _tagToType[tag]);
                else
                    sb.Append(tag.Content);
            }
            sb.Append(".lpr");
            //if (NamePanel.Children.Count != 0)
            //{
            //    sb.Remove(0, SplitSymbol.Text.Length);
            //}
            Properties.Settings.Default.FileNameFormat = sb.ToString();
        }


        private void ControlDropped(object sender, DragEventArgs e)
        {
            Button item = e.Data.GetData("Object") as Button;
            double x = e.GetPosition(NamePanel).X;
            double len = 0;
            int insertTo = 0;
            if (NamePanel.Children.Count > 0)
            {
                for (; insertTo < NamePanel.Children.Count; insertTo++)
                {
                    len += ((NamePanel.Children[insertTo] as Button).ActualWidth + 10);
                    if (len - (NamePanel.Children[insertTo] as Button).ActualWidth / 2 - 5 > x)
                    {

                        break;
                    }
                }
            }
            if (SourcePanel.Children.Contains(item))
            {

                SourcePanel.Children.Remove(item);
                NamePanel.Children.Insert(insertTo, item);
                if (SymbolTypes.Contains(item.Content as String))
                {
                    CreateButton(item.Content as String, true);
                }
            }
            else
            {
                if (insertTo > NamePanel.Children.IndexOf(item))
                    insertTo--;
                NamePanel.Children.Remove(item);
                NamePanel.Children.Insert(insertTo, item);
            }

            RefreshExample();
        }

        private void RemoveFromNamePanel(Button button)
        {
            if (NamePanel.Children.Contains(button))
            {
                NamePanel.Children.Remove(button);
                SourcePanel.Children.Add(button);
                RefreshExample();
            }
        }



        private void CancelSymbol(object sender, MouseButtonEventArgs e)
        {
            Button tsb = sender as Button;
            if (NamePanel.Children.Contains(tsb))
            {
                NamePanel.Children.Remove(tsb);
                RefreshExample();
            }
        }

        private void CancelFromNames(object sender, MouseButtonEventArgs e)
        {
            RemoveFromNamePanel(sender as Button);
        }

        private void SplitSymbol_TextChanged(object sender, TextChangedEventArgs e)
        {

            RefreshExample();
        }

        private void ToSettingsPage()
        {
            MainWindow.Instance.SwitchMainContent(new Settings());
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFormat();
            ToSettingsPage();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ToSettingsPage();
        }

        private static Char[] _unAcceptChars = new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };
        private void SplitSymbol_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (Char c in _unAcceptChars)
            {
                if (e.Text.Contains(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }



    }
}
