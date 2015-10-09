using System;
using System.Globalization;
using System.Windows.Data;

namespace Board.Converters
{
    public class ValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                if((double)value > 1000)
                {
                    return ((double)value - 300);
                }
                else
                {
                    return ((double)value - 210);
                }
            }

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}