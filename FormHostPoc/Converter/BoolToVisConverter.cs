using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FormHostPoc.Converter
{
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //invert
            if (parameter != null && parameter.ToString() == "1")
                return (Visibility)((bool)value ? 2 : 0);
            
            return (Visibility)((bool)value ? 0 : 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
