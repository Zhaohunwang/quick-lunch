using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Models
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string? Alias { get; set; }

        public string Path { get; set; } = string.Empty;

        public string? Description { get; set; }

        public List<string> Tags { get; set; } = new();

        public string? Color { get; set; }

        public string? Icon { get; set; }

        public Guid? DefaultIdeId { get; set; }

        public List<IdeConfiguration> IdeConfigurations { get; set; } = new();

        public DateTime LastOpenedAt { get; set; }

        public int OpenCount { get; set; }

        public bool IsFavorite { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string SearchText => $"{Name} {Alias} {string.Join(" ", Tags)}".ToLower();
    }
}
