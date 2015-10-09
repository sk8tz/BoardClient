using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Board.Common;
using Board.Enums;

namespace Board.Controls
{
    /// <summary>
    /// TrendTicker.xaml 的交互逻辑
    /// </summary>
    public partial class TrendTicker : UserControl
    {
        private string _format = "P1";
        private static readonly Brush _brNegative = new SolidColorBrush(Colors.Red);
        private static readonly Brush _brPositive = new SolidColorBrush(Colors.Green);

        public string BindingSource { get; set; }

        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
                _txtValue.Text = Value.ToString(_format);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(TrendTicker), new PropertyMetadata(0.0, ValueChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public TrendTicker()
        {
            InitializeComponent();
            _arrow.Fill = null;
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var ticker = d as TrendTicker;
                var value = (double)e.NewValue;
                var oldValue = (double)e.OldValue;

                // resetting
                if(double.IsNaN(value))
                {
                    ticker._root.Background = new SolidColorBrush(Colors.Transparent);
                    ticker._arrow.Fill = null;
                    return;
                }

                // update text
                ticker._txtValue.Text = value.ToString(ticker._format);

                if(value > 0)
                {
                    ticker._stArrow.ScaleY = +1;
                    ticker._arrow.Fill = _brPositive;
                    ticker._txtValue.Foreground = _brPositive;
                }
                else if(value < 0)
                {
                    ticker._stArrow.ScaleY = -1;
                    ticker._arrow.Fill = _brNegative;
                    ticker._txtValue.Foreground = _brNegative;
                }
                else
                {
                    ticker._arrow.Fill = null;
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("创建单元格内容出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to CreateCellContent", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "CreateCellContent", null);
                }
            }
        }
    }
}
