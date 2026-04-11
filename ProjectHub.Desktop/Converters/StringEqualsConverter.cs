using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class StringEqualsConverter : IValueConverter
    {
        public static readonly StringEqualsConverter Instance = new StringEqualsConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if ((bool)value!)
            {
                return parameter?.ToString();
            }
            return AvaloniaProperty.UnsetValue;
        }
    }
}