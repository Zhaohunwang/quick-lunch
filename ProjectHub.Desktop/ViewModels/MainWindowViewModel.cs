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

        public MainWindowViewModel()
        {
            _projectService = new ProjectService();
            _ideLauncherService = new IdeLauncherService();
            _searchService = new SearchService();
            _tagService = new TagService();
            _workspaceService = new WorkspaceService(_projectService);
            
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
            AllProjectsCount = projects.Count;
            FavoriteProjectsCount = projects.Count(p => p.IsFavorite);
            RecentProjectsCount = projects.Count(p => p.LastOpenedAt > DateTime.UtcNow.AddDays(-7));
            Console.WriteLine($"Counts: All={AllProjectsCount}, Favorite={FavoriteProjectsCount}, Recent={RecentProjectsCount}");
            OnPropertyChanged(nameof(AllProjectsText));
            OnPropertyChanged(nameof(FavoriteProjectsText));
            OnPropertyChanged(nameof(RecentProjectsText));
            
            await LoadWorkspacesForList();
            
            ApplyFilter();
            Console.WriteLine("LoadProjects completed");
        }

        private async Task LoadWorkspacesForList()
        {
            Console.WriteLine("LoadWorkspacesForList started");
            var workspaces = await _workspaceService.GetAllWorkspacesAsync();
            Console.WriteLine($"Workspaces loaded: {workspaces.Count}");
            
            AllItems.Clear();
            
            foreach (var workspace in workspaces)
            {
                AllItems.Add(workspace);
                Console.WriteLine($"Added workspace: {workspace.Name}");
            }
            
            foreach (var project in Projects)
            {
                AllItems.Add(project);
                Console.WriteLine($"Added project: {project.Name}");
            }
            Console.WriteLine($"AllItems count: {AllItems.Count}");
            Console.WriteLine("LoadWorkspacesForList completed");
        }

        private void ApplyFilter()
        {
            IEnumerable<Project> filteredProjects = CurrentFilter switch
            {
                "favorite" => Projects.Where(p => p.IsFavorite),
                "recent" => Projects.Where(p => p.LastOpenedAt > DateTime.UtcNow.AddDays(-7)),
                "tag" => Projects.Where(p => p.Tags.Contains(SelectedTag.TrimStart('#'))),
                _ => Projects
            };

            FilteredItems.Clear();
            
            foreach (var project in filteredProjects)
            {
                FilteredItems.Add(project);
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
                var dialog = new Avalonia.Controls.Window
                {
                    Title = "确认删除",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                    Background = Avalonia.Media.Brushes.White,
                    BorderThickness = new Avalonia.Thickness(1),
                    BorderBrush = Avalonia.Media.Brushes.LightGray,
                    CornerRadius = new Avalonia.CornerRadius(8),
                    Padding = new Avalonia.Thickness(0)
                };
                
                var panel = new Avalonia.Controls.StackPanel { Margin = new Avalonia.Thickness(24) };
                
                var titleText = new Avalonia.Controls.TextBlock 
                {
                    Text = "确认删除",
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.SemiBold,
                    Margin = new Avalonia.Thickness(0, 0, 0, 16)
                };
                panel.Children.Add(titleText);
                
                var contentText = new Avalonia.Controls.TextBlock 
                {
                    Text = $"确定要删除项目 '{project.Name}' 吗？",
                    FontSize = 14,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 0, 0, 24)
                };
                panel.Children.Add(contentText);
                
                var buttonPanel = new Avalonia.Controls.StackPanel 
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Spacing = 12
                };
                
                var cancelButton = new Avalonia.Controls.Button 
                {
                    Content = "取消",
                    Width = 90,
                    Height = 36,
                    Background = Avalonia.Media.Brushes.Transparent,
                    BorderThickness = new Avalonia.Thickness(1),
                    BorderBrush = Avalonia.Media.Brushes.LightGray,
                    CornerRadius = new Avalonia.CornerRadius(4)
                };
                
                var deleteButton = new Avalonia.Controls.Button 
                {
                    Content = "删除",
                    Width = 90,
                    Height = 36,
                    Background = Avalonia.Media.Brushes.Red,
                    Foreground = Avalonia.Media.Brushes.White,
                    CornerRadius = new Avalonia.CornerRadius(4)
                };
                
                bool? dialogResult = null;
                cancelButton.Click += (s, e) => { dialogResult = false; dialog.Close(); };
                deleteButton.Click += (s, e) => { dialogResult = true; dialog.Close(); };
                
                buttonPanel.Children.Add(cancelButton);
                buttonPanel.Children.Add(deleteButton);
                panel.Children.Add(buttonPanel);
                
                dialog.Content = panel;
                await dialog.ShowDialog(mainWindow);
                
                if (dialogResult == true)
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
