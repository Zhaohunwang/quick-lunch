using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;
using ProjectHub.Desktop.Services;
using ProjectHub.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;
        private readonly IIdeLauncherService _ideLauncherService;
        private readonly ISearchService _searchService;
        private readonly ITagService _tagService;
        private readonly IWorkspaceService _workspaceService;

        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private ObservableCollection<object> _allItems = new();

        [ObservableProperty]
        private ObservableCollection<object> _filteredItems = new();

        [ObservableProperty]
        private ObservableCollection<string> _tags = new();

        [ObservableProperty]
        private ObservableCollection<string> _workspaces = new();

        [ObservableProperty]
        private ObservableCollection<string> _displayTags = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private int _allProjectsCount;

        [ObservableProperty]
        private int _favoriteProjectsCount;

        [ObservableProperty]
        private int _recentProjectsCount;

        [ObservableProperty]
        private string _currentFilter = "all";

        [ObservableProperty]
        private string _selectedTag = string.Empty;

        public string AllProjectsText => $"📁 全部({AllProjectsCount})";
        public string FavoriteProjectsText => $"⭐ 收藏({FavoriteProjectsCount})";
        public string RecentProjectsText => $"🕐 最近({RecentProjectsCount})";

        [ObservableProperty]
        private bool _isAllSelected = true;
        
        [ObservableProperty]
        private bool _isFavoriteSelected = false;
        
        [ObservableProperty]
        private bool _isRecentSelected = false;

        [ObservableProperty]
        private string _currentViewMode = "card"; // card, list, compact

        public bool HasMoreTags => Tags.Count > 6;
        
        [RelayCommand]
        private void ChangeViewMode(string mode)
        {
            CurrentViewMode = mode;
        }

        public MainWindowViewModel()
        {
            _projectService = new ProjectService();
            _ideLauncherService = new IdeLauncherService();
            _searchService = new SearchService();
            _tagService = new TagService();
            _workspaceService = new WorkspaceService(_projectService);
            
            // 同步加载标签数据
            LoadTagsSync();
            
            // 异步加载其他数据
            _ = InitializeAsync();
        }
        
        private async Task InitializeAsync()
        {
            await LoadProjectsAndWorkspacesAsync();
            // 确保数据加载完成后再应用筛选
            ApplyFilter();
        }

        private void LoadTagsSync()
        {
            try
            {
                var tags = _tagService.GetAllTagsAsync().Result;
                Tags.Clear();
                foreach (var tag in tags)
                {
                    Tags.Add($"#{tag.Name}");
                }
                UpdateDisplayTags();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载标签失败: {ex.Message}");
            }
        }

        private async Task LoadProjectsAndWorkspacesAsync()
        {
            await LoadProjects();
        }

        private async Task LoadProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }
            AllProjectsCount = projects.Count;
            FavoriteProjectsCount = projects.Count(p => p.IsFavorite);
            RecentProjectsCount = projects.Count(p => p.LastOpenedAt > DateTime.UtcNow.AddDays(-7));
            OnPropertyChanged(nameof(AllProjectsText));
            OnPropertyChanged(nameof(FavoriteProjectsText));
            OnPropertyChanged(nameof(RecentProjectsText));
            
            // 加载工作区并添加到AllItems
            await LoadWorkspacesForList();
            
            // 确保在所有数据加载完成后再应用筛选
            ApplyFilter();
        }

        private async Task LoadWorkspacesForList()
        {
            var workspaces = await _workspaceService.GetAllWorkspacesAsync();
            
            // 清空并重新构建 AllItems
            AllItems.Clear();
            
            // 添加工作区
            foreach (var workspace in workspaces)
            {
                AllItems.Add(workspace);
            }
            
            // 添加项目
            foreach (var project in Projects)
            {
                AllItems.Add(project);
            }
        }

        private void ApplyFilter()
        {
            // 筛选项目
            IEnumerable<Project> filteredProjects = CurrentFilter switch
            {
                "favorite" => Projects.Where(p => p.IsFavorite),
                "recent" => Projects.Where(p => p.LastOpenedAt > DateTime.UtcNow.AddDays(-7)),
                "tag" => Projects.Where(p => p.Tags.Contains(SelectedTag.TrimStart('#'))),
                _ => Projects
            };

            // 构建混合列表：工作区 + 筛选后的项目
            FilteredItems.Clear();
            
            // 添加所有工作区
            foreach (var item in AllItems)
            {
                if (item is Core.Models.Workspace)
                {
                    FilteredItems.Add(item);
                }
            }
            
            // 添加筛选后的项目
            foreach (var project in filteredProjects)
            {
                FilteredItems.Add(project);
            }
            
            // 手动触发通知，确保 UI 更新
            OnPropertyChanged(nameof(FilteredItems));
        }

        [RelayCommand]
        private void ShowAllProjects()
        {
            CurrentFilter = "all";
            SelectedTag = string.Empty;
            ApplyFilter();
            IsAllSelected = true;
            IsFavoriteSelected = false;
            IsRecentSelected = false;
        }

        [RelayCommand]
        private void ShowFavoriteProjects()
        {
            CurrentFilter = "favorite";
            SelectedTag = string.Empty;
            ApplyFilter();
            IsAllSelected = false;
            IsFavoriteSelected = true;
            IsRecentSelected = false;
        }

        [RelayCommand]
        private void ShowRecentProjects()
        {
            CurrentFilter = "recent";
            SelectedTag = string.Empty;
            ApplyFilter();
            IsAllSelected = false;
            IsFavoriteSelected = false;
            IsRecentSelected = true;
        }

        [RelayCommand]
        private void FilterByTag(string tag)
        {
            CurrentFilter = "tag";
            SelectedTag = tag;
            ApplyFilter();
            IsAllSelected = false;
            IsFavoriteSelected = false;
            IsRecentSelected = false;
        }

        private async Task LoadTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync();
                Tags.Clear();
                foreach (var tag in tags)
                {
                    Tags.Add($"#{tag.Name}");
                }
                UpdateDisplayTags();
                OnPropertyChanged(nameof(Tags));
                OnPropertyChanged(nameof(DisplayTags));
            }
            catch (Exception ex)
            {
                // 记录错误但不影响程序运行
                Console.WriteLine($"加载标签失败: {ex.Message}");
            }
        }

        private void UpdateDisplayTags()
        {
            DisplayTags.Clear();
            if (Tags.Count <= 6)
            {
                foreach (var tag in Tags)
                {
                    DisplayTags.Add(tag);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    DisplayTags.Add(Tags[i]);
                }
                DisplayTags.Add("...");
            }
            OnPropertyChanged(nameof(HasMoreTags));
            OnPropertyChanged(nameof(DisplayTags));
        }

        private async Task LoadWorkspaces()
        {
            var workspaces = await _workspaceService.GetAllWorkspacesAsync();
            Workspaces.Clear();
            foreach (var workspace in workspaces)
            {
                Workspaces.Add(workspace.Name);
            }
        }

        [RelayCommand]
        private async Task AddProject()
        {
            var dialog = new AddProjectDialog();
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;

            if (mainWindow != null)
            {
                var result = await dialog.ShowDialog<bool>(mainWindow);
                if (result == true)
                {
                    var viewModel = dialog.DataContext as AddProjectDialogViewModel;
                    if (viewModel != null)
                    {
                        var project = viewModel.CreateProject();
                        await _projectService.AddProjectAsync(project);
                        await LoadProjects();
                        await LoadTags();
                    }
                }
            }
        }

        [RelayCommand]
        private async Task DeleteProject(Project project)
        {
            await _projectService.DeleteProjectAsync(project.Id);
            Projects.Remove(project);
            ApplyFilter();
            AllProjectsCount = Projects.Count;
            FavoriteProjectsCount = Projects.Count(p => p.IsFavorite);
            RecentProjectsCount = Projects.Count(p => p.LastOpenedAt > DateTime.UtcNow.AddDays(-7));
            OnPropertyChanged(nameof(AllProjectsText));
            OnPropertyChanged(nameof(FavoriteProjectsText));
            OnPropertyChanged(nameof(RecentProjectsText));
        }

        [RelayCommand]
        private async Task EditProject(Project project)
        {
            var dialog = new AddProjectDialog();
            var viewModel = dialog.DataContext as AddProjectDialogViewModel;
            if (viewModel != null)
            {
                viewModel.LoadProject(project);
            }

            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;

            if (mainWindow != null)
            {
                var result = await dialog.ShowDialog<bool>(mainWindow);
                if (result == true && viewModel != null)
                {
                    var updatedProject = viewModel.CreateProject();
                    updatedProject.Id = project.Id;
                    updatedProject.CreatedAt = project.CreatedAt;
                    await _projectService.UpdateProjectAsync(updatedProject);
                    await LoadProjects();
                }
            }
        }

        [RelayCommand]
        private async Task LaunchProject(Project project)
        {
            await _ideLauncherService.LaunchProjectAsync(project);
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
        private async Task AddTag()
        {
            var dialog = new AddTagDialog();
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;

            if (mainWindow != null)
            {
                var result = await dialog.ShowDialog<bool>(mainWindow);
                if (result == true)
                {
                    var viewModel = dialog.DataContext as AddTagDialogViewModel;
                    if (viewModel != null)
                    {
                        await LoadTags();
                    }
                }
            }
        }

        [RelayCommand]
        private async Task AddWorkspace()
        {
            var dialog = new AddWorkspaceDialog();
            var viewModel = dialog.DataContext as AddWorkspaceDialogViewModel;
            if (viewModel != null)
            {
                await viewModel.LoadProjectsAsync();
            }

            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;

            if (mainWindow != null)
            {
                var result = await dialog.ShowDialog<bool>(mainWindow);
                if (result == true)
                {
                    await LoadWorkspaces();
                }
            }
        }

        [RelayCommand]
        private async Task ShowAllTags()
        {
            var dialog = new TagListDialog();
            var viewModel = new TagListDialogViewModel();
            foreach (var tag in Tags)
            {
                viewModel.Tags.Add(tag);
            }
            viewModel.TagSelected += (sender, tag) =>
            {
                CurrentFilter = "tag";
                SelectedTag = tag;
                ApplyFilter();
                OnPropertyChanged(nameof(IsAllSelected));
                OnPropertyChanged(nameof(IsFavoriteSelected));
                OnPropertyChanged(nameof(IsRecentSelected));
                dialog.Close();
            };
            dialog.DataContext = viewModel;

            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;

            if (mainWindow != null)
            {
                await dialog.ShowDialog(mainWindow);
            }
        }

        [RelayCommand]
        private async Task ShowIdeSettings()
        {
            var dialog = new IdeSettingsDialog();
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;

            if (mainWindow != null)
            {
                await dialog.ShowDialog(mainWindow);
            }
        }

        [RelayCommand]
        private async Task QuickLaunch()
        {
            var recentProject = FilteredItems.OfType<Project>()
                .OrderByDescending(p => p.LastOpenedAt)
                .FirstOrDefault();

            if (recentProject != null)
            {
                await LaunchProject(recentProject);
            }
        }
    }
}
