using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetAllProjectsAsync();

        Task<Project?> GetProjectByIdAsync(Guid id);

        Task<Project> AddProjectAsync(Project project);

        Task<Project> UpdateProjectAsync(Project project);

        Task DeleteProjectAsync(Guid id);

        Task<List<Project>> GetRecentProjectsAsync(int limit = 10);

        Task<List<Project>> GetFavoriteProjectsAsync();

        Task<List<Project>> GetProjectsByTagAsync(string tag);

        Task<List<Project>> GetProjectsByIdsAsync(List<Guid> ids);
    }
}
