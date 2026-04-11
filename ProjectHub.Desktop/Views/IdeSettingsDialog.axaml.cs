using Avalonia.Controls;
using Avalonia.Interactivity;
using ProjectHub.Desktop.ViewModels;

namespace ProjectHub.Desktop.Views
{
    public partial class IdeSettingsDialog : Window
    {
        public IdeSettingsDialog()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}