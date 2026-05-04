using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Desktop.ViewModels;

namespace ProjectHub.Desktop.Views
{
    public partial class IdeSettingsDialog : Window
    {
        public IdeSettingsDialog()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<IdeSettingsDialogViewModel>();
        }

        private void OnCloseClicked(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}