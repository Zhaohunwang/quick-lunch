using Avalonia.Data.Converters;
using System;

namespace ProjectHub.Desktop.Converters
{
    public class BooleanToClassesConverter : IValueConverter
    {
        public string TrueClasses { get; set; } = string.Empty;
        public string FalseClasses { get; set; } = string.Empty;

        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueClasses : FalseClasses;
            }
            return FalseClasses;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
