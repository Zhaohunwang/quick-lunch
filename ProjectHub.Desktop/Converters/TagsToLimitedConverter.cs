using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ProjectHub.Desktop.Converters
{
    public class TagsToLimitedConverter : IValueConverter
    {
        public static readonly TagsToLimitedConverter Instance = new TagsToLimitedConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not List<string> tags) return null;
            
            int limit = 3; // 默认显示3个
            if (parameter is string paramStr && int.TryParse(paramStr, out int parsedLimit))
            {
                limit = parsedLimit;
            }
            
            return tags.Take(limit).ToList();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
