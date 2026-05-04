using ProjectHub.Core.Models;

namespace ProjectHub.Core.Services;

public interface IProjectService
{
    Task<List<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(long id);
    Task<Project> AddProjectAsync(Project project);
    Task<Project> UpdateProjectAsync(Project project);
    Task DeleteProjectAsync(long id);
    Task<List<Project>> GetRecentProjectsAsync(int limit = 10);
    Task<List<Project>> GetFavoriteProjectsAsync();
    Task<List<Project>> GetProjectsByTagAsync(string tag);
    Task<List<Project>> GetProjectsByIdsAsync(List<long> ids);
}
