using System.Collections.ObjectModel;
using System.Windows;

using C1.WPF.C1Chart;

namespace Board.Common
{
    public static class ChartExtensions
    {
        public static ObservableCollection<Axis> GetBindingAxis(DependencyObject obj)
        {
            return (ObservableCollection<Axis>)obj.GetValue(BindingAxisProperty);
        }

        public static void SetBindingAxis(DependencyObject obj, ObservableCollection<Axis> value)
        {
            obj.SetValue(BindingAxisProperty, value);
        }

        private static void BindingAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bindingAxis = e.NewValue as ObservableCollection<Axis>;
            if(bindingAxis != null)
            {
                var c1ChartView = d as ChartView;
                if(c1ChartView != null)
                {
                    c1ChartView.Axes.Clear();

                    foreach(var axis in bindingAxis)
                    {
                        c1ChartView.Axes.Add(axis);
                    }
                }
            }
        }

        // Using a DependencyProperty as the backing store for BindingColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindingAxisProperty =
            DependencyProperty.RegisterAttached("BindingAxis", typeof(ObservableCollection<Axis>),
                typeof(ChartExtensions), new PropertyMetadata(null, BindingAxisChanged));
    }
}
