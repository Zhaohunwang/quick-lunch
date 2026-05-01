using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Models
{
    public class Project : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _name = string.Empty;
        public string Name 
        { 
            get => _name; 
            set { _name = value; OnPropertyChanged(); }
        }

        private string? _alias;
        public string? Alias 
        { 
            get => _alias; 
            set { _alias = value; OnPropertyChanged(); }
        }

        public string Path { get; set; } = string.Empty;

        private string? _description;
        public string? Description 
        { 
            get => _description; 
            set { _description = value; OnPropertyChanged(); }
        }

        public List<string> Tags { get; set; } = new();

        public string? Color { get; set; }

        public string? Icon { get; set; }

        public Guid? DefaultIdeId { get; set; }

        public List<IdeConfiguration> IdeConfigurations { get; set; } = new();

        public DateTime LastOpenedAt { get; set; }

        public int OpenCount { get; set; }

        private bool _isFavorite;
        public bool IsFavorite 
        { 
            get => _isFavorite; 
            set { _isFavorite = value; OnPropertyChanged(); }
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string SearchText => $"{Name} {Alias} {string.Join(" ", Tags)}".ToLower();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}