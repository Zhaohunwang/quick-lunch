using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Models
{
    public class IdeConfiguration
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string ExecutablePath { get; set; } = string.Empty;

        public string? CommandArgs { get; set; }

        public bool IsDefault { get; set; }

        public string? Icon { get; set; }

        public Guid ProjectId { get; set; }

        public Project Project { get; set; } = null!;
    }
}
