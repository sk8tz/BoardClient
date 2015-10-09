using System.Windows;
using System.Windows.Controls;

namespace Board.Controls
{
    public class WidgetListView : ListView
    {
        // Using a DependencyProperty as the backing store for AcHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcHeightProperty =
            DependencyProperty.Register("AcHeight", typeof (double), typeof (WidgetListView),
                new PropertyMetadata(double.NaN));

        // Using a DependencyProperty as the backing store for AcWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcWidthProperty =
            DependencyProperty.Register("AcWidth", typeof (double), typeof (WidgetListView),
                new PropertyMetadata(double.NaN));

        public double AcHeight
        {
            get { return (double) GetValue(AcHeightProperty); }
            set { SetValue(AcHeightProperty, value); }
        }

        public double AcWidth
        {
            get { return (double) GetValue(AcWidthProperty); }
            set { SetValue(AcWidthProperty, value); }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            AcHeight = ActualHeight;
            AcWidth = ActualWidth;
        }
    }
}