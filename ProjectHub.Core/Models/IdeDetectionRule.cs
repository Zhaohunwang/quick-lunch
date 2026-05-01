using System.Collections.Generic;

namespace ProjectHub.Core.Models
{
    public class IdeDetectionRule
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string ExeName { get; set; } = string.Empty;

        public List<string> SearchPaths { get; set; } = new();

        public string? Icon { get; set; }

        public int Priority { get; set; }

        public string? DefaultArgs { get; set; }

        public List<string> SupportedExtensions { get; set; } = new();
    }
}
