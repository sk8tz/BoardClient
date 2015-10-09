using System;
using System.Globalization;
using System.Windows.Data;

using C1.WPF.C1Chart;

namespace Board.Converters
{
    public class ChartTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ChartType) (int) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}