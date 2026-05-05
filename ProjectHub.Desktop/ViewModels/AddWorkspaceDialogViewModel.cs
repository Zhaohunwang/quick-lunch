using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;
using ProjectHub.Desktop.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.ViewModels
{
    public partial class AddWorkspaceDialogViewModel : ObservableObject
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly IProjectService _projectService;
        private readonly IIdeLauncherService _ideLauncherService;

        [ObservableProperty]
        private string _workspaceName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Project> _selectedProjects = new();

        [ObservableProperty]
        private ObservableCollection<Project> _availableProjects = new();

        [ObservableProperty]
        private ObservableCollection<IdeTemplate> _availableIdes = new();

        [ObservableProperty]
        private IdeTemplate? _selectedIde;

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private bool _isFavorite = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private long? _defaultIdeId;

        public string DialogTitle => IsEditMode ? "编辑工作区" : "添加工作区";

        public AddWorkspaceDialogViewModel() : this(new WorkspaceService(), new ProjectService(), new IdeLauncherService()) { }

        public AddWorkspaceDialogViewModel(IWorkspaceService workspaceService, IProjectService projectService, IIdeLauncherService ideLauncherService)
        {
            _workspaceService = workspaceService;
            _projectService = projectService;
            _ideLauncherService = ideLauncherService;
            _ = LoadIdesAsync();
        }

        private async Task LoadIdesAsync()
        {
            var ides = await _ideLauncherService.GetAvailableIdesAsync();
            AvailableIdes.Clear();
            foreach (var ide in ides)
            {
                AvailableIdes.Add(ide);
            }
        }

        public async Task LoadProjectsAsync()
        {
            var allProjects = await _projectService.GetAllProjectsAsync();
            AvailableProjects.Clear();
            
            foreach (var project in allProjects)
            {
                if (!SelectedProjects.Any(sp => sp.Id == project.Id))
                {
                    AvailableProjects.Add(project);
                }
            }
        }

        public void LoadWorkspace(Workspace workspace)
        {
            IsEditMode = true;
            OnPropertyChanged(nameof(DialogTitle));
            
            WorkspaceName = workspace.Name;
            Description = workspace.Description ?? string.Empty;
            IsFavorite = workspace.IsFavorite;
            _defaultIdeId = workspace.DefaultIdeId;
            
            if (workspace.DefaultIdeId.HasValue)
            {
                SelectedIde = AvailableIdes.FirstOrDefault(ide => ide.Id == workspace.DefaultIdeId.Value);
            }
            
            SelectedProjects.Clear();
            var allProjects = _projectService.GetAllProjectsAsync().Result;
            foreach (var projectId in workspace.ProjectIds)
            {
                var project = allProjects.FirstOrDefault(p => p.Id == projectId);
                if (project != null)
                {
                    SelectedProjects.Add(project);
                }
            }
        }

        [RelayCommand]
        private void AddProject(Project project)
        {
            if (project != null && !SelectedProjects.Any(sp => sp.Id == project.Id))
            {
                SelectedProjects.Add(project);
                AvailableProjects.Remove(project);
            }
        }

        [RelayCommand]
        private void RemoveProject(Project project)
        {
            if (project != null)
            {
                SelectedProjects.Remove(project);
                AvailableProjects.Add(project);
            }
        }

        [RelayCommand]
        private async Task SearchProjects()
        {
            var allProjects = await _projectService.GetAllProjectsAsync();
            
            AvailableProjects.Clear();
            
            var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                ? allProjects
                : allProjects.Where(p => 
                    p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (p.Alias != null && p.Alias.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));

            foreach (var project in filtered)
            {
                if (!SelectedProjects.Any(sp => sp.Id == project.Id))
                {
                    AvailableProjects.Add(project);
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!Validate())
            {
                return;
            }

            var workspace = new Workspace
            {
                Name = WorkspaceName.Trim(),
                Description = Description.Trim(),
                DefaultIdeId = SelectedIde?.Id,
                IsFavorite = IsFavorite
            };

            if (IsEditMode)
            {
                var existingWorkspace = await _workspaceService.GetWorkspaceByNameAsync(WorkspaceName.Trim());
                if (existingWorkspace != null)
                {
                    workspace.Id = existingWorkspace.Id;
                    workspace.CreatedAt = existingWorkspace.CreatedAt;
                }
            }

            workspace.ProjectIds.Clear();
            foreach (var project in SelectedProjects)
            {
                workspace.ProjectIds.Add(project.Id);
            }

            if (IsEditMode)
            {
                await _workspaceService.UpdateWorkspaceAsync(workspace);
            }
            else
            {
                await _workspaceService.AddWorkspaceAsync(workspace);
            }

            Close(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }

        public bool Validate()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(WorkspaceName))
            {
                ErrorMessage = "工作区名称不能为空";
                return false;
            }

            if (WorkspaceName.Length > 100)
            {
                ErrorMessage = "工作区名称不能超过100个字符";
                return false;
            }

            if (SelectedProjects.Count == 0)
            {
                ErrorMessage = "请至少选择一个项目";
                return false;
            }

            if (SelectedProjects.Count > 20)
            {
                ErrorMessage = "最多只能选择20个项目";
                return false;
            }

            return true;
        }

        public Workspace CreateWorkspace()
        {
            var workspace = new Workspace
            {
                Name = WorkspaceName.Trim(),
                Description = Description.Trim(),
                DefaultIdeId = SelectedIde?.Id,
                IsFavorite = IsFavorite
            };

            workspace.ProjectIds.Clear();
            foreach (var project in SelectedProjects)
            {
                workspace.ProjectIds.Add(project.Id);
            }

            return workspace;
        }

        public event EventHandler<bool>? CloseRequested;

        private void Close(bool result)
        {
            CloseRequested?.Invoke(this, result);
        }
    }
}
