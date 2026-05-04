using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Desktop.ViewModels;

namespace ProjectHub.Desktop.Views;

public partial class AddProjectDialog : Window
{
    public AddProjectDialog()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<AddProjectDialogViewModel>();
    }

    private async void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AddProjectDialogViewModel viewModel)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel != null)
            {
                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "选择项目文件夹",
                    AllowMultiple = false
                });

                if (folders.Count > 0)
                {
                    viewModel.ProjectPath = folders[0].Path.LocalPath;
                    viewModel.PathTypeIndex = 0;
                }
            }
        }
    }

    private async void BrowseFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AddProjectDialogViewModel viewModel)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel != null)
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择文件",
                    AllowMultiple = false
                });

                if (files.Count > 0)
                {
                    viewModel.ProjectPath = files[0].Path.LocalPath;
                    viewModel.PathTypeIndex = 1;
                }
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AddProjectDialogViewModel viewModel)
        {
            if (viewModel.Validate())
            {
                Close(true);
            }
        }
    }

    private void AddTagButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AddProjectDialogViewModel viewModel)
        {
            viewModel.AddTagCommand.Execute(null);
        }
    }

    private void RemoveTagButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            if (DataContext is AddProjectDialogViewModel viewModel)
            {
                viewModel.RemoveTagCommand.Execute(tag);
            }
        }
    }
}
