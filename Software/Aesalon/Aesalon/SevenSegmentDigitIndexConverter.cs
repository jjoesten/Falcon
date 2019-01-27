using System;
using System.Globalization;
using System.Windows.Data;

namespace Aesalon
{
    public class SevenSegmentDigitIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte? index = (byte?)value;
            if (index.HasValue)
                return index.ToString();
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            if (text == string.Empty)
                return (byte?)null;
            else
                return byte.Parse(text);
        }
    }
}
