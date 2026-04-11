using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace ProjectHub.Desktop.ViewModels
{
    public partial class TagListDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<string> _tags = new();

        public event EventHandler<string>? TagSelected;

        [RelayCommand]
        private void SelectTag(string tag)
        {
            TagSelected?.Invoke(this, tag);
        }
    }
}
