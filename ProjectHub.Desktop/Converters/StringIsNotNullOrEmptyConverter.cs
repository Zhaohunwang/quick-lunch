using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class StringIsNotNullOrEmptyConverter : IValueConverter
    {
        public static readonly StringIsNotNullOrEmptyConverter Instance = new StringIsNotNullOrEmptyConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
