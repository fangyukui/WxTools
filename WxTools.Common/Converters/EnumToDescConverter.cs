using System;
using System.Globalization;
using System.Windows.Data;
using WxTools.Common.Model;

namespace WxTools.Common.Converters
{
    public class EnumToDescConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                RunState state;
                Enum.TryParse(value.ToString(), out state);
                return typeof(RunState).GetEnumDesc((int)state);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
