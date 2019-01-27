using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Aesalon
{
    public class SevenSegmentDigitFillConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool? segmentOn = values[0] as bool?;
            Brush fillOn = (Brush)values[1];
            Brush fillOff = (Brush)values[2];
            return segmentOn == true ? fillOn : fillOff;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
