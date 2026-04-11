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
    public partial class AddTagDialogViewModel : ObservableObject
    {
        private readonly ITagService _tagService;

        [ObservableProperty]
        private string _tagName = string.Empty;

        [ObservableProperty]
        private string _tagColor = "#3498db";

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public ObservableCollection<string> AvailableColors { get; } = new()
        {
            "#e74c3c",
            "#f39c12",
            "#f1c40f",
            "#2ecc71",
            "#3498db",
            "#9b59b6",
            "#34495e",
            "#ecf0f1"
        };

        public string DialogTitle => IsEditMode ? "编辑标签" : "添加标签";

        public AddTagDialogViewModel()
        {
            _tagService = new TagService();
        }

        public AddTagDialogViewModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        public void LoadTag(Tag tag)
        {
            IsEditMode = true;
            OnPropertyChanged(nameof(DialogTitle));
            
            TagName = tag.Name;
            TagColor = tag.Color;
        }

        [RelayCommand]
        private void SelectColor(string color)
        {
            TagColor = color;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!Validate())
            {
                return;
            }

            var tag = new Tag
            {
                Name = TagName.Trim(),
                Color = TagColor
            };

            if (IsEditMode)
            {
                var existingTag = await _tagService.GetTagByNameAsync(TagName.Trim());
                if (existingTag != null)
                {
                    tag.Id = existingTag.Id;
                    tag.CreatedAt = existingTag.CreatedAt;
                    await _tagService.UpdateTagAsync(tag);
                }
            }
            else
            {
                await _tagService.AddTagAsync(tag);
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

            if (string.IsNullOrWhiteSpace(TagName))
            {
                ErrorMessage = "标签名称不能为空";
                return false;
            }

            if (TagName.Length > 50)
            {
                ErrorMessage = "标签名称不能超过50个字符";
                return false;
            }

            if (!IsEditMode)
            {
                var exists = _tagService.TagExistsAsync(TagName.Trim()).Result;
                if (exists)
                {
                    ErrorMessage = "标签名称已存在";
                    return false;
                }
            }

            return true;
        }

        public Tag CreateTag()
        {
            return new Tag
            {
                Name = TagName.Trim(),
                Color = TagColor
            };
        }

        public event EventHandler<bool>? CloseRequested;

        private void Close(bool result)
        {
            CloseRequested?.Invoke(this, result);
        }
    }
}
