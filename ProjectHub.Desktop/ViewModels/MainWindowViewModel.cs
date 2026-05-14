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
using Avalonia;

namespace ProjectHub.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;
        private readonly IIdeLauncherService _ideLauncherService;
        private readonly ISearchService _searchService;
        private readonly ITagService _tagService;
        private readonly IWorkspaceService _workspaceService;
        private readonly IWorkspaceLauncherService _workspaceLauncherService;

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

        [ObservableProperty]
        private ObservableCollection<IdeTemplate> _availableIdes = new();

        public string AllProjectsText => $"全部({AllProjectsCount})";
        public string FavoriteProjectsText => $"收藏({FavoriteProjectsCount})";
        public string RecentProjectsText => $"最近({RecentProjectsCount})";

        [ObservableProperty]
        private bool _isAllSelected = true;
        
        [ObservableProperty]
        private bool _isFavoriteSelected = false;
        
        [ObservableProperty]
        private bool _isRecentSelected = false;

        [ObservableProperty]
        private string _currentViewMode = "card"; // card, list, compact

        [ObservableProperty]
        private bool _isLightTheme = App.ThemeService.CurrentTheme == Services.ThemeMode.Light;

        [ObservableProperty]
        private bool _isDarkTheme = App.ThemeService.CurrentTheme == Services.ThemeMode.Dark;

        public bool HasMoreTags => Tags.Count > 6;

        partial void OnIsLightThemeChanged(bool value)
        {
            if (value)
            {
                IsDarkTheme = false;
                App.ThemeService.ApplyTheme(App.Current!, ThemeMode.Light);
            }
            else if (!IsDarkTheme)
            {
                App.ThemeService.ApplyTheme(App.Current!, ThemeMode.Default);
            }
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            if (value)
            {
                IsLightTheme = false;
                App.ThemeService.ApplyTheme(App.Current!, ThemeMode.Dark);
            }
            else if (!IsLightTheme)
            {
                App.ThemeService.ApplyTheme(App.Current!, ThemeMode.Default);
            }
        }
        
        [RelayCommand]
        private void ChangeViewMode(string mode)
        {
            CurrentViewMode = mode;
        }

        public MainWindowViewModel() : this(
            new ProjectService(), new IdeLauncherService(),
            new SearchService(), new TagService(), new WorkspaceService(),
            new WorkspaceLauncherService(new ProjectService())) { }

        public MainWindowViewModel(
            IProjectService projectService,
            IIdeLauncherService ideLauncherService,
            ISearchService searchService,
            ITagService tagService,
            IWorkspaceService workspaceService,
            IWorkspaceLauncherService workspaceLauncherService)
        {
            _projectService = projectService;
            _ideLauncherService = ideLauncherService;
            _searchService = searchService;
            _tagService = tagService;
            _workspaceService = workspaceService;
            _workspaceLauncherService = workspaceLauncherService;

            LoadTagsSync();
            _ = LoadDataAsync();
            _ = LoadAvailableIdesAsync();
        }
        
        private async Task LoadDataAsync()
        {
            await LoadProjectsAndWorkspacesAsync();
            ApplyFilter();
        }

        private async Task LoadAvailableIdesAsync()
        {
            var ides = await _ideLauncherService.GetAvailableIdesAsync();
            AvailableIdes.Clear();
            foreach (var ide in ides)
            {
                AvailableIdes.Add(ide);
            }
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
            Console.WriteLine("LoadProjects started");
            var projects = await _projectService.GetAllProjectsAsync();
            Console.WriteLine($"Projects loaded: {projects.Count}");
            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }

            // 仅更新项目相关的计数
            FavoriteProjectsCount = projects.Count(p => p.IsFavorite);
            RecentProjectsCount = projects.Count(p => p.LastOpenedAt > DateTime.UtcNow.AddDays(-7));
            Console.WriteLine($"Counts: Favorite={FavoriteProjectsCount}, Recent={RecentProjectsCount}");
            OnPropertyChanged(nameof(FavoriteProjectsText));
            OnPropertyChanged(nameof(RecentProjectsText));

            // AllProjectsCount 将在 LoadWorkspacesForList() 中更新

            await LoadWorkspacesForList();

            Console.WriteLine("LoadProjects completed");
        }

        private async Task LoadWorkspacesForList()
        {
            Console.WriteLine("LoadWorkspacesForList started");
            var workspaces = await _workspaceService.GetAllWorkspacesAsync();
            Console.WriteLine($"Workspaces loaded: {workspaces.Count}");

            AllItems.Clear();

            // 添加工作区（排在前面）
            foreach (var workspace in workspaces)
            {
                AllItems.Add(workspace);
                Console.WriteLine($"Added workspace: {workspace.Name}");
            }

            // 添加项目（排在后面）
            foreach (var project in Projects)
            {
                AllItems.Add(project);
                Console.WriteLine($"Added project: {project.Name}");
            }

            // 更新计数：全部数量 = 工作区数 + 项目数
            AllProjectsCount = workspaces.Count + Projects.Count;
            Console.WriteLine($"AllItems count: {AllItems.Count}, Total count: {AllProjectsCount}");

            OnPropertyChanged(nameof(AllProjectsText));
            Console.WriteLine("LoadWorkspacesForList completed");
        }

        private void ApplyFilter()
        {
            FilteredItems.Clear();

            if (CurrentFilter == "all")
            {
                foreach (var item in AllItems)
                {
                    FilteredItems.Add(item);
                }
            }
            else if (CurrentFilter == "favorite")
            {
                foreach (var item in AllItems)
                {
                    if (item is Workspace ws && ws.IsFavorite)
                        FilteredItems.Add(item);
                    else if (item is Project p && p.IsFavorite)
                        FilteredItems.Add(item);
                }
            }
            else
            {
                IEnumerable<object> filtered = CurrentFilter switch
                {
                    "recent" => Projects.Where(p => p.LastOpenedAt > DateTime.UtcNow.AddDays(-7)).Cast<object>(),
                    "tag" => AllItems.Where(item =>
                        (item is Project p && p.Tags.Contains(SelectedTag.TrimStart('#'))) ||
                        (item is Workspace ws && ws.AllTags.Contains(SelectedTag.TrimStart('#')))),
                    _ => AllItems
                };

                foreach (var item in filtered)
                {
                    FilteredItems.Add(item);
                }
            }

            OnPropertyChanged(nameof(FilteredItems));
        }

        public IdeTemplate? GetDefaultIdeTemplate(Project project)
        {
            if (project.DefaultIdeId.HasValue)
            {
                return AvailableIdes.FirstOrDefault(ide => ide.Id == project.DefaultIdeId.Value);
            }
            return AvailableIdes.FirstOrDefault();
        }

        [RelayCommand]
        private async Task LaunchWithIde((Project Project, IdeTemplate Ide) param)
        {
            if (param.Project == null || param.Ide == null) return;
            await _ideLauncherService.LaunchProjectWithTemplateAsync(param.Project, param.Ide);
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
                        ApplyFilter();
                        await LoadTags();
                    }
                }
            }
        }

        [RelayCommand]
        private async Task DeleteProject(Project project)
        {
            if (project == null) return;
            
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;
            
            if (mainWindow != null)
            {
                var dialog = new ConfirmDialog(
                    "确认删除",
                    $"确定要删除项目 '{project.Name}' 吗？\n\n此操作将从项目列表中移除该项目，但保留磁盘上的文件。",
                    "删除");
                var result = await dialog.ShowDialog<bool>(mainWindow);
                
                if (result)
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
            }
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
                    ApplyFilter();
                }
            }
        }

        [RelayCommand]
        private async Task LaunchProject(Project project)
        {
            await _ideLauncherService.LaunchProjectAsync(project);
        }

        [RelayCommand]
        private async Task ToggleFavorite(Project project)
        {
            if (project == null) return;

            project.IsFavorite = !project.IsFavorite;
            await _projectService.UpdateProjectAsync(project);

            FavoriteProjectsCount = Projects.Count(p => p.IsFavorite);
            OnPropertyChanged(nameof(FavoriteProjectsText));
            ApplyFilter();
        }

        [RelayCommand]
        private async Task ToggleFavoriteWorkspace(Workspace workspace)
        {
            if (workspace == null) return;

            workspace.IsFavorite = !workspace.IsFavorite;
            await _workspaceService.UpdateWorkspaceAsync(workspace);

            ApplyFilter();
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
            ApplyFilter();
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
                    await LoadWorkspacesForList();
                    ApplyFilter();
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
                await LoadAvailableIdesAsync();
            }
        }

        [RelayCommand]
        private async Task EditWorkspace(Workspace workspace)
        {
            if (workspace == null) return;

            var dialog = new AddWorkspaceDialog();
            var viewModel = dialog.DataContext as AddWorkspaceDialogViewModel;
            if (viewModel != null)
            {
                viewModel.LoadWorkspace(workspace);
            }

            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;

            if (mainWindow != null)
            {
                var result = await dialog.ShowDialog<bool>(mainWindow);
                if (result == true && viewModel != null)
                {
                    var updatedWorkspace = viewModel.CreateWorkspace();
                    updatedWorkspace.Id = workspace.Id;
                    updatedWorkspace.CreatedAt = workspace.CreatedAt;
                    await _workspaceService.UpdateWorkspaceAsync(updatedWorkspace);
                    await LoadWorkspacesForList();
                    ApplyFilter();
                }
            }
        }

        [RelayCommand]
        private async Task DeleteWorkspace(Workspace workspace)
        {
            if (workspace == null) return;

            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;

            if (mainWindow != null)
            {
                var dialog = new ConfirmDialog(
                    "确认删除工作区",
                    $"确定要删除工作区「{workspace.Name}」吗？\n\n此操作不可撤销，工作区的项目关联将被移除。",
                    "确认删除");
                var result = await dialog.ShowDialog<bool>(mainWindow);

                if (result)
                {
                    await _workspaceService.DeleteWorkspaceAsync(workspace.Id);
                    await LoadWorkspacesForList();
                }
            }
        }

        [RelayCommand]
        private async Task RefreshData()
        {
            await LoadTags();
            await LoadDataAsync();
            await LoadAvailableIdesAsync();
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

        [RelayCommand]
        private async Task LaunchWorkspace(Workspace workspace)
        {
            if (workspace == null) return;
            var ide = GetDefaultIdeTemplateForWorkspace(workspace);
            if (ide == null)
            {
                FileLogger.Warn($"No IDE available to launch workspace '{workspace.Name}'. Please configure an IDE first.");
                return;
            }
            try
            {
                await _workspaceLauncherService.LaunchWorkspaceAsync(workspace, ide);
            }
            catch (Exception ex)
            {
                FileLogger.Error($"Failed to launch workspace '{workspace.Name}' with IDE '{ide.Name}'", ex);
            }
        }

        [RelayCommand]
        private async Task LaunchWorkspaceWithIde((Workspace Workspace, IdeTemplate Ide) param)
        {
            if (param.Workspace == null || param.Ide == null) return;
            try
            {
                await _workspaceLauncherService.LaunchWorkspaceAsync(param.Workspace, param.Ide);
            }
            catch (Exception ex)
            {
                FileLogger.Error($"Failed to launch workspace '{param.Workspace.Name}' with IDE '{param.Ide.Name}'", ex);
            }
        }

        public IdeTemplate? GetDefaultIdeTemplateForWorkspace(Workspace workspace)
        {
            if (workspace.DefaultIdeId.HasValue)
            {
                return AvailableIdes.FirstOrDefault(ide => ide.Id == workspace.DefaultIdeId.Value);
            }
            return AvailableIdes.FirstOrDefault();
        }
    }
}
