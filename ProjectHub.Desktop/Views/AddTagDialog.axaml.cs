using Avalonia.Controls;
using Avalonia.Interactivity;
using ProjectHub.Desktop.ViewModels;
using System;
using System.Linq;

namespace ProjectHub.Desktop.Views
{
    public partial class AddTagDialog : Window
    {
        public AddTagDialog()
        {
            InitializeComponent();
            var viewModel = new AddTagDialogViewModel();
            viewModel.CloseRequested += (sender, result) => Close(result);
            DataContext = viewModel;
        }

        private void OnSaveClicked(object? sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as AddTagDialogViewModel;
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
