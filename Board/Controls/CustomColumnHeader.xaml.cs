using System.Windows;
using System.Windows.Controls;

namespace Board.Controls
{
    /// <summary>
    /// ColumnHeader.xaml 的交互逻辑
    /// </summary>
    public partial class CustomColumnHeader : UserControl
    {
        public CustomColumnHeader()
        {
            InitializeComponent();
        }

        public string HeaderName
        {
            get { return (string)GetValue(HeaderNameProperty); }
            set { SetValue(HeaderNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderNameProperty =
            DependencyProperty.Register("HeaderName", typeof(string), typeof(CustomColumnHeader), new PropertyMetadata(string.Empty, HeaderNameChanged));

        private static void HeaderNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var header = d as CustomColumnHeader;
            var value = (string)e.NewValue;
            var oldValue = (string)e.OldValue;

            // resetting
            if(string.IsNullOrEmpty(value))
            {
                return;
            }

            // update text
            header._textBlock.Text = value;
            header._path.ToolTip = value;
        }


        public string HeaderToolTip
        {
            get { return (string)GetValue(HeaderTooltipProperty); }
            set { SetValue(HeaderTooltipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderTooltip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderTooltipProperty =
            DependencyProperty.Register("HeaderToolTip", typeof(string), typeof(CustomColumnHeader), new PropertyMetadata(string.Empty));
    }
}
