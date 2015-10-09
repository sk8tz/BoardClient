using System;
using System.Globalization;
using System.Windows.Data;

namespace Board.Converters
{
    public class TypeToNameConverter : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var typeId = (int) value;
            switch (typeId)
            {
                case 1:
                    return "公有数据舱";
                case 2:
                    return "私有数据舱";
                default:
                    return "原始数据表";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
