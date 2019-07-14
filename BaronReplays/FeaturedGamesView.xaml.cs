using BaronReplays.BRapi.Service;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BaronReplays
{
    /// <summary>
    /// FeaturedGamesView.xaml 的互動邏輯
    /// </summary>
    public partial class FeaturedGamesView : UserControl, IFeaturedGamesQuery
    {
        private FeaturedGameJson[] _gameData;
        public event MainWindow.StartRecordFeaturedGameDelegate FeatureGameClicked;
        public event MainWindow.PlayGameDelegate FeatureGamePlayOnlyClicked;

        private DispatcherTimer _gameLengthTimer;

        public FeaturedGameJson NowDisplay
        {
            get
            {
                if (_gameData == null)
                    return null;
                if (_gameData.Length == 0)
                    return null;
                return _gameData[_nth];
            }
        }

        public FeaturedGamesView()
        {
            InitializeComponent();
        }

        private void _loadingStoryBoard_Completed(object sender, EventArgs e)
        {
            HideFeaturedGames();
        }

        private void LoadingTimesUp(object sender, EventArgs e)
        {
            CancelWaitForData();
        }

        private void CancelWaitForData()
        {
            gameManager.CancelAsync();
        }

        private void HideFeaturedGames()
        {
            Game.Visibility = Visibility.Hidden;
            SelectPoints.Children.Clear();
        }

        private void ShowFeaturedGames()
        {
            Game.Visibility = Visibility.Visible;
        }


        private Storyboard _loadingStoryBoard;
        private DoubleAnimation loadingLineWidthAnimation;
        private void InitLoadingAnimation()
        {
            _loadingStoryBoard = new Storyboard();
            _loadingStoryBoard.FillBehavior = FillBehavior.Stop;
            _loadingStoryBoard.Children.Add(loadingLineWidthAnimation);
            _loadingStoryBoard.Completed += LoadingTimesUp;
            this.RegisterName("Width", LoadingLine);
        }

        private void InitLoadingLineWidthControl()
        {
            loadingLineWidthAnimation = new DoubleAnimation();
            loadingLineWidthAnimation.From = 0;
            loadingLineWidthAnimation.To = this.ActualWidth;
            loadingLineWidthAnimation.DecelerationRatio = 1.0;
            loadingLineWidthAnimation.Duration = new Duration(TimeSpan.FromSeconds(10));

            Storyboard.SetTargetName(loadingLineWidthAnimation, "Width");
            Storyboard.SetTargetProperty(loadingLineWidthAnimation, new PropertyPath(Rectangle.WidthProperty));
        }


        private void StartLoadingLineAnimation()
        {
            InitLoadingLineWidthControl();
            InitLoadingAnimation();
            LoadingLine.Visibility = System.Windows.Visibility.Visible;
            _loadingStoryBoard.Begin(LoadingLine, true);
        }

        private void StopLoadingLineAnimation()
        {
            _loadingStoryBoard.Stop(LoadingLine);
            LoadingLine.Visibility = System.Windows.Visibility.Hidden;
        }

        public void RefreshDone(Boolean isSuccess)
        {
            if (!hasNewRequest)
            {
                StopLoadingLineAnimation();
                if (isSuccess)
                {
                    _gameData = gameManager.Games;
                    RefreshGameList();
                    ShowFeaturedGames();
                }
                else
                {
                    HideFeaturedGames();
                }
            }
            hasNewRequest = false;
        }

        private void RefreshGameList()
        {
            SelectPoints.Children.Clear();
            _nth = 0;
            for (int i = 0; i < _gameData.Length; i++)
            {
                Button btn = new Button();
                btn.Width = 32.0;
                btn.Height = 32.0;
                Style style = this.FindResource("TriButtonStyle") as Style;
                btn.Style = style;
                btn.Margin = new Thickness(5, 0, 5, 0);
                btn.Click += FeaturedGameNumber_Click;
                btn.Content = (i + 1).ToString();
                SelectPoints.Children.Add(btn);
            }
            if (_gameData.Length == 0)
                HideFeaturedGames();
            else
                ChangeGameContent(0);
        }

        private void FeaturedGameNumber_Click(object sender, RoutedEventArgs e)
        {
            ChangeGameContentTo(SelectPoints.Children.IndexOf(sender as UIElement));
        }


        private void RegionButton_Click(object sender, RoutedEventArgs e)
        {
            string p = (sender as Button).Tag as String;
            ChangePlatform(p);
        }

        private int _nth = 0;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //公用區域會有Bug//待修
            if (Utilities.Regions.Contains(Properties.Settings.Default.Platform))
            {
                PlatformBox.SelectedIndex = FeaturedGamesManager.FeaturedPlatform.IndexOf(Properties.Settings.Default.Platform);
            }
            else
            {
                PlatformBox.SelectedIndex = FeaturedGamesManager.FeaturedPlatform.IndexOf("NA1");
            }
            _gameLengthTimer = new DispatcherTimer();
            _gameLengthTimer.Interval = TimeSpan.FromSeconds(1);
            _gameLengthTimer.Tick += _gameLengthTimer_Tick;
            _gameLengthTimer.Start();
        }

        private void _gameLengthTimer_Tick(object sender, EventArgs e)
        {
            if (NowDisplay != null)
                NowDisplay.GameLength++;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (FeatureGameClicked != null)
            {
                GameInfo info = new GameInfo(Utilities.LoLObserveServersIpMapping[gameManager.Platform], NowDisplay.PlatformId, NowDisplay.gameId, NowDisplay.observers["encryptionKey"].ToString());
                Nullable<Boolean> result = YesNoPopupWindow.AskQuestion(MainWindow.Instance, Utilities.GetString("WatchNow") as string);
                if (result == true)
                    FeatureGameClicked(info, NowDisplay, true);
                else if (result == false)
                    FeatureGameClicked(info, NowDisplay, false);
            }
        }

        private void ChangeGameContentTo(int n)
        {
            if (SelectPoints.Children.Count <= 0)
                return;
            (SelectPoints.Children[_nth] as Button).IsEnabled = true;
            if (n >= _gameData.Length)
                n = 0;
            else if (n < 0)
                n = _gameData.Length - 1;
            _nth = n;
            Game.DataContext = NowDisplay;
            (SelectPoints.Children[_nth] as Button).IsEnabled = false;
        }

        private void ChangeGameContent(int n)
        {
            ChangeGameContentTo(_nth + n);
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeGameContent(-1);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeGameContent(1);
        }

        private FeaturedGamesManager gameManager;
        private Boolean hasNewRequest = false;
        private void ChangePlatform(String p)
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                PopupWindow.ShowMessage(MainWindow.Instance, Utilities.GetString("NetworkError") as String);
                return;
            }

            StartLoadingLineAnimation();
            if (gameManager == null)
            {
                gameManager = new FeaturedGamesManager(p, this);
            }
            else
            {
                if (gameManager.IsBusy)
                    hasNewRequest = true;
                CancelWaitForData();
                gameManager.Platform = p;
            }
        }

        private void PlayOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            if (FeatureGamePlayOnlyClicked != null)
                FeatureGamePlayOnlyClicked(new GameInfo(Utilities.LoLObserveServersIpMapping[gameManager.Platform], NowDisplay.PlatformId, NowDisplay.gameId, NowDisplay.observers["encryptionKey"].ToString()));
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                ChangeGameContent(1);
            else
                ChangeGameContent(-1);

        }

        private void PlatformBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangePlatform(FeaturedGamesManager.FeaturedPlatform[PlatformBox.SelectedIndex]);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            gameManager.RenewAsync();
        }


    }

}
