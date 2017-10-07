using System;
using System.Globalization;
using System.Windows.Data;

namespace WxTools.Theme.Converter
{
    public class OffsetConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + (double)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
