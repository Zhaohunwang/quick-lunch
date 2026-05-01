using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class BoolToFavoriteIconConverter : IValueConverter
    {
        public static readonly BoolToFavoriteIconConverter Instance = new BoolToFavoriteIconConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isFavorite && isFavorite)
            {
                return "⭐";
            }
            return "☆";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
