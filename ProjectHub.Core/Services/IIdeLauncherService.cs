using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Services
{
    public interface IIdeLauncherService
    {
        Task LaunchProjectAsync(Project project, string? ideName = null);

        Task<List<IdeTemplate>> GetAvailableIdesAsync();

        Task AddIdeTemplateAsync(IdeTemplate template);

        Task UpdateIdeTemplateAsync(IdeTemplate template);

        Task DeleteIdeTemplateAsync(Guid id);
    }
}
