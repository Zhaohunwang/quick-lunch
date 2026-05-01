using Avalonia.Data.Converters;
using Avalonia.VisualTree;
using ProjectHub.Core.Models;
using ProjectHub.Desktop.ViewModels;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class DefaultIdeConverter : IValueConverter
    {
        public static readonly DefaultIdeConverter Instance = new DefaultIdeConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Project project)
                return "▶ 打开";

            var vm = GetViewModel();
            if (vm == null)
                return "▶ 打开";

            var defaultIde = vm.GetDefaultIdeTemplate(project);
            if (defaultIde != null)
            {
                var icon = !string.IsNullOrEmpty(defaultIde.Icon) ? defaultIde.Icon : "💻";
                return $"{icon} 用{defaultIde.Name}打开";
            }

            return "▶ 打开";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static MainWindowViewModel? GetViewModel()
        {
            if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow?.DataContext as MainWindowViewModel;
            }
            return null;
        }
    }
}
