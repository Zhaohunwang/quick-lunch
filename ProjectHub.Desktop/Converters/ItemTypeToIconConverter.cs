using Avalonia.Data.Converters;
using Material.Icons;
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
                Project => MaterialIconKind.LightningBolt,
                Workspace => MaterialIconKind.BriefcaseOutline,
                _ => MaterialIconKind.Folder
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
