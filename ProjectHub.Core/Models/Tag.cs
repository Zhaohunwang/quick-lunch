using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Models
{
    public class Tag
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Color { get; set; } = "#3498db";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
