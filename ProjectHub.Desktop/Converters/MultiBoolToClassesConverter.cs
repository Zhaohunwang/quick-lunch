using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ProjectHub.Desktop.Converters
{
    public class MultiBoolToClassesConverter : IMultiValueConverter
    {
        public static readonly MultiBoolToClassesConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string paramStr)
                return "NavigationButton";

            var parts = paramStr.Split('|');
            var baseClass = parts[0];
            var activeClass = parts.Length > 1 ? parts[1] : "Active";

            // Check if any value is true
            bool isActive = values.Any(v => v is true);

            return isActive ? $"{baseClass} {activeClass}" : baseClass;
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
