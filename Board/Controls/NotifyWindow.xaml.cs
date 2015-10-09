using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;

namespace Board.Controls
{
    /// <summary>
    /// NotifyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NotifyWindow : Window
    {
        private bool _requireClose;
        private Timer _timer;

        public NotifyWindow()
        {
            InitializeComponent();
        }

        #region 属性
        
        public string WindowTitle { get; set; }

        public string MessageText { get; set; }

        private int _showupTime = 2;

        public int ShowupTime
        {
            get { return _showupTime; }
            set { _showupTime = value; }
        }

        private int _holdingTime = 2;

        public int HoldingTime
        {
            get { return _holdingTime; }
            set { _holdingTime = value; }
        }

        private int _closeTime = 2;

        public int CloseTime
        {
            get { return _closeTime; }
            set { _closeTime = value; }
        }

        #endregion

        #region 方法

        private void OnNotifyWindowLoaded(object sender, RoutedEventArgs e)
        {
            Window.Title = Title;
            textBlock.Text = MessageText;
            _timer = new Timer(TimerCallback, null, (ShowupTime + HoldingTime) * 1000, Timeout.Infinite);
        }

        private void TimerCallback(object obj)
        {
            Dispatcher.Invoke(() =>
            {
                if(!_requireClose)
                {
                    BeginStoryboard((Storyboard)FindResource("OnUnloaded"));

                    _timer.Change(CloseTime * 1000, Timeout.Infinite);

                    _requireClose = true;
                }
                else
                {
                    _timer.Dispose();
                    Close();
                }
            });
        }
        
        private void OnButtonBaseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

    }
}