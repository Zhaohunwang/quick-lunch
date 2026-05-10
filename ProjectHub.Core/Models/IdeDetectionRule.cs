using System.Collections.Generic;

namespace ProjectHub.Core.Models
{
    public class IdeDetectionRule
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public PlatformValue ExeName { get; set; } = new();

        public PlatformPaths SearchPaths { get; set; } = new();

        public string? Icon { get; set; }

        public int Priority { get; set; }

        public string? DefaultArgs { get; set; }

        public List<string> SupportedExtensions { get; set; } = new();
    }

    public class PlatformValue
    {
        public string? Windows { get; set; }

        public string? MacOS { get; set; }

        public string? Linux { get; set; }
    }

    public class PlatformPaths
    {
        public List<string> Windows { get; set; } = new();

        public List<string> MacOS { get; set; } = new();

        public List<string> Linux { get; set; } = new();
    }
}
