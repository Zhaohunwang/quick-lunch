using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectHub.Core.Models;
using ProjectHub.Desktop.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.ViewModels
{
    public partial class IdeSettingsDialogViewModel : ObservableObject
    {
        private readonly IdeDetectionService _detectionService;
        private readonly IdeLauncherService _launcherService;

        [ObservableProperty]
        private ObservableCollection<IdeTemplate> _availableIdes = new();

        [ObservableProperty]
        private IdeTemplate? _selectedIde;

        [ObservableProperty]
        private string _newIdeName = string.Empty;

        [ObservableProperty]
        private string _newIdePath = string.Empty;

        [ObservableProperty]
        private string _newIdeArgs = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public IdeSettingsDialogViewModel()
        {
            _detectionService = new IdeDetectionService();
            _launcherService = new IdeLauncherService();
            _ = LoadIdesAsync();
        }

        private async Task LoadIdesAsync()
        {
            var ides = await _launcherService.GetAvailableIdesAsync();
            AvailableIdes.Clear();
            foreach (var ide in ides)
            {
                AvailableIdes.Add(ide);
            }
        }

        [RelayCommand]
        private async Task DetectIdes()
        {
            var detectedIdes = _detectionService.DetectInstalledIdes();
            
            foreach (var detectedIde in detectedIdes)
            {
                var existingIde = AvailableIdes.FirstOrDefault(ide => ide.Name == detectedIde.Name);
                if (existingIde == null)
                {
                    await _launcherService.AddIdeTemplateAsync(detectedIde);
                    AvailableIdes.Add(detectedIde);
                }
            }
        }

        [RelayCommand]
        private async Task AddCustomIde()
        {
            if (!ValidateCustomIde())
            {
                return;
            }

            var newIde = new IdeTemplate
            {
                Name = NewIdeName.Trim(),
                ExecutablePath = NewIdePath.Trim(),
                DefaultArgs = NewIdeArgs.Trim(),
                Icon = "⚙️",
                Priority = AvailableIdes.Count + 1
            };

            await _launcherService.AddIdeTemplateAsync(newIde);
            AvailableIdes.Add(newIde);

            ClearCustomIdeFields();
        }

        [RelayCommand]
        private async Task UpdateSelectedIde()
        {
            if (SelectedIde == null)
            {
                ErrorMessage = "请先选择一个IDE";
                return;
            }

            await _launcherService.UpdateIdeTemplateAsync(SelectedIde);
            ErrorMessage = "更新成功";
        }

        [RelayCommand]
        private async Task DeleteSelectedIde()
        {
            if (SelectedIde == null)
            {
                ErrorMessage = "请先选择一个IDE";
                return;
            }

            await _launcherService.DeleteIdeTemplateAsync(SelectedIde.Id);
            AvailableIdes.Remove(SelectedIde);
            SelectedIde = null;
            ErrorMessage = "删除成功";
        }

        [RelayCommand]
        private void SelectIde(IdeTemplate? ide)
        {
            SelectedIde = ide;
        }

        [RelayCommand]
        private async Task BrowseIdePath()
        {
            var dialog = new Avalonia.Controls.OpenFileDialog
            {
                Title = "选择IDE可执行文件",
                Filters = new System.Collections.Generic.List<Avalonia.Controls.FileDialogFilter>
                {
                    new Avalonia.Controls.FileDialogFilter { Name = "可执行文件", Extensions = new System.Collections.Generic.List<string> { "exe" } },
                    new Avalonia.Controls.FileDialogFilter { Name = "所有文件", Extensions = new System.Collections.Generic.List<string> { "*" } }
                }
            };

            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow != null)
            {
                var result = await dialog.ShowAsync(mainWindow);
                if (result != null && result.Length > 0)
                {
                    NewIdePath = result[0];
                }
            }
        }

        private bool ValidateCustomIde()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewIdeName))
            {
                ErrorMessage = "IDE名称不能为空";
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewIdePath))
            {
                ErrorMessage = "IDE路径不能为空";
                return false;
            }

            if (!System.IO.File.Exists(NewIdePath))
            {
                ErrorMessage = "IDE路径不存在";
                return false;
            }

            if (AvailableIdes.Any(ide => ide.Name.Equals(NewIdeName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "IDE名称已存在";
                return false;
            }

            return true;
        }

        private void ClearCustomIdeFields()
        {
            NewIdeName = string.Empty;
            NewIdePath = string.Empty;
            NewIdeArgs = string.Empty;
            ErrorMessage = string.Empty;
        }

        public event EventHandler<bool>? CloseRequested;

        private void Close(bool result)
        {
            CloseRequested?.Invoke(this, result);
        }
    }
}