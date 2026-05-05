using Avalonia.Data.Converters;
using ProjectHub.Core.Models;
using ProjectHub.Desktop.ViewModels;
using System;
using System.Globalization;

namespace ProjectHub.Desktop.Converters
{
    public class WorkspaceDefaultIdeConverter : IValueConverter
    {
        public static readonly WorkspaceDefaultIdeConverter Instance = new WorkspaceDefaultIdeConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Workspace workspace)
                return "打开";

            var vm = GetViewModel();
            if (vm == null)
                return "打开";

            var defaultIde = vm.GetDefaultIdeTemplateForWorkspace(workspace);
            if (defaultIde != null)
            {
                return $"用 {defaultIde.Name} 打开";
            }

            return "打开";
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
