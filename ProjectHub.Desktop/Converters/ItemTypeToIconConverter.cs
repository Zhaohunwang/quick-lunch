using Avalonia.Data.Converters;
using ProjectHub.Core.Models;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class ItemTypeToIconConverter : IValueConverter
    {
        public static readonly ItemTypeToIconConverter Instance = new ItemTypeToIconConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                Project => "🚀",
                Workspace => "💼",
                _ => "📁"
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
