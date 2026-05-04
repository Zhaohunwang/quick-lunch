using Avalonia.Data.Converters;
using ProjectHub.Core.Models;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class IsWorkspaceConverter : IValueConverter
    {
        public static readonly IsWorkspaceConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isWorkspace = value is Workspace;

            // 支持反转参数
            if (parameter?.ToString() == "Invert")
            {
                return !isWorkspace;
            }

            return isWorkspace;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
