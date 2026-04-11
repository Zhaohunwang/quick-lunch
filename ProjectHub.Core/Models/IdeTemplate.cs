using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Models
{
    public class IdeTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string ExecutablePath { get; set; } = string.Empty;

        public string? DefaultArgs { get; set; }

        public string? Icon { get; set; }

        public List<string> SupportedExtensions { get; set; } = new();

        public int Priority { get; set; }
    }
}
