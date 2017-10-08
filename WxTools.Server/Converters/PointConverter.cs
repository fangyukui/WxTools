using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace WxTools.Server.Converters
{
    public class PointToStrConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var p =(Point)value;
                return p.X + "x" + p.Y;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
