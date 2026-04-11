using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return value;

            var format = parameter.ToString();
            if (string.IsNullOrEmpty(format))
                return value;

            return string.Format(format, value);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
