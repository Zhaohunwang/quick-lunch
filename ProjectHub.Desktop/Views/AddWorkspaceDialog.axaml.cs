using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Core.Models;
using ProjectHub.Desktop.ViewModels;

namespace ProjectHub.Desktop.Views
{
    public partial class AddWorkspaceDialog : Window
    {
        public AddWorkspaceDialog()
        {
            InitializeComponent();
            var viewModel = App.Services.GetRequiredService<AddWorkspaceDialogViewModel>();
            viewModel.CloseRequested += (sender, result) => Close(result);
            DataContext = viewModel;
        }

        private void OnRemoveProjectClicked(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Project project)
            {
                var viewModel = DataContext as AddWorkspaceDialogViewModel;
                if (viewModel != null)
                {
                    viewModel.RemoveProjectCommand.Execute(project);
                }
            }
        }

        private void OnSaveClicked(object? sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as AddWorkspaceDialogViewModel;
            if (viewModel != null)
            {
                viewModel.SaveCommand.Execute(null);
            }
        }

        private void OnCancelClicked(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}