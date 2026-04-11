using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;
using ProjectHub.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.ViewModels
{
    public partial class QuickLauncherViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;
        private readonly IIdeLauncherService _ideLauncherService;
        private readonly ISearchService _searchService;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private Project? _selectedProject;

        public QuickLauncherViewModel()
        {
            _projectService = new ProjectService();
            _ideLauncherService = new IdeLauncherService();
            _searchService = new SearchService();
            _ = LoadProjects();
        }

        private async Task LoadProjects()
        {
            var projects = await _projectService.GetRecentProjectsAsync(20);
            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }
        }

        [RelayCommand]
        private async Task Search()
        {
            var results = await _searchService.SearchProjectsAsync(SearchQuery);
            Projects.Clear();
            foreach (var project in results)
            {
                Projects.Add(project);
            }
        }

        [RelayCommand]
        private async Task LaunchProject()
        {
            if (SelectedProject != null)
            {
                await _ideLauncherService.LaunchProjectAsync(SelectedProject);
            }
        }

        [RelayCommand]
        private void OpenManager()
        {
            // 打开管理界面
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}