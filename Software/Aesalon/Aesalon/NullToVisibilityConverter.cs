using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Aesalon
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public Visibility NullVisibility { get; set; }
        public Visibility NotNullVisibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null ? NullVisibility : NotNullVisibility;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
