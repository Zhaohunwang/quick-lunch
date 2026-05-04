using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;
using ProjectHub.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.ViewModels
{
    public partial class AddProjectDialogViewModel : ObservableObject
    {
        private readonly IProjectService _projectService;
        private readonly IIdeLauncherService _ideLauncherService;

        [ObservableProperty]
        private string _projectName = string.Empty;

        [ObservableProperty]
        private string _projectPath = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFile))]
        [NotifyPropertyChangedFor(nameof(IsIdeEnabled))]
        private int _pathTypeIndex = 0;

        public bool IsFile => PathTypeIndex == 1;
        public bool IsIdeEnabled => PathTypeIndex == 0;

        [ObservableProperty]
        private string _alias = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _newTagText = string.Empty;

        [ObservableProperty]
        private string _selectedAvailableTag = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _selectedTags = new();

        [ObservableProperty]
        private ObservableCollection<string> _availableTags = new();

        [ObservableProperty]
        private ObservableCollection<string> _availableIdes = new();

        [ObservableProperty]
        private string _selectedIde = string.Empty;

        [ObservableProperty]
        private bool _isFavorite = false;

        [ObservableProperty]
        private bool _isEditMode = false;

        public string DialogTitle => IsEditMode ? "编辑项目" : "添加项目";

        [RelayCommand]
        private void AddTag()
        {
            if (!string.IsNullOrWhiteSpace(NewTagText))
            {
                var trimmedTag = NewTagText.Trim();
                if (!SelectedTags.Contains(trimmedTag))
                {
                    SelectedTags.Add(trimmedTag);
                }
                NewTagText = string.Empty;
            }
        }

        [RelayCommand]
        private void RemoveTag(string tag)
        {
            SelectedTags.Remove(tag);
        }

        public void LoadProject(Project project)
        {
            IsEditMode = true;
            OnPropertyChanged(nameof(DialogTitle));
            
            ProjectName = project.Name;
            ProjectPath = project.Path;
            Alias = project.Alias ?? string.Empty;
            Description = project.Description ?? string.Empty;
            IsFavorite = project.IsFavorite;
            
            SelectedTags.Clear();
            foreach (var tag in project.Tags)
            {
                SelectedTags.Add(tag);
            }

            var defaultIde = project.IdeConfigurations?.FirstOrDefault(ic => ic.IsDefault);
            if (defaultIde != null)
            {
                SelectedIde = defaultIde.Name;
            }
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(ProjectPath))
            {
                return false;
            }

            return true;
        }

        public AddProjectDialogViewModel() : this(new ProjectService(), new IdeLauncherService()) { }

        public AddProjectDialogViewModel(IProjectService projectService, IIdeLauncherService ideLauncherService)
        {
            _projectService = projectService;
            _ideLauncherService = ideLauncherService;
            _ = LoadAvailableTags();
            LoadAvailableIdes();
        }

        private async Task LoadAvailableTags()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var tags = projects.SelectMany(p => p.Tags).Distinct().ToList();
            AvailableTags.Clear();
            foreach (var tag in tags)
            {
                AvailableTags.Add(tag);
            }
        }

        private void LoadAvailableIdes()
        {
            AvailableIdes.Clear();
            AvailableIdes.Add("VS Code");
            AvailableIdes.Add("Trae");
            AvailableIdes.Add("WebStorm");
            AvailableIdes.Add("IntelliJ IDEA");
            AvailableIdes.Add("Visual Studio");
        }

        public Project CreateProject()
        {
            var ideConfigurations = new List<IdeConfiguration>();
            if (IsIdeEnabled && !string.IsNullOrWhiteSpace(SelectedIde))
            {
                ideConfigurations.Add(new IdeConfiguration
                {
                    Name = SelectedIde,
                    ExecutablePath = GetIdeExecutablePath(SelectedIde),
                    IsDefault = true
                });
            }

            return new Project
            {
                Name = ProjectName,
                Path = ProjectPath,
                Alias = string.IsNullOrWhiteSpace(Alias) ? null : Alias,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                Tags = SelectedTags.ToList(),
                IdeConfigurations = ideConfigurations,
                IsFavorite = IsFavorite,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private string GetIdeExecutablePath(string ideName)
        {
            return ideName switch
            {
                "VS Code" => "code.exe",
                "Trae" => "trae.exe",
                "WebStorm" => "webstorm64.exe",
                "IntelliJ IDEA" => "idea64.exe",
                "Visual Studio" => "devenv.exe",
                _ => "code.exe"
            };
        }
    }
}
