using ProjectHub.Core.Models;

namespace ProjectHub.Core.Services;

public interface IWorkspaceService
{
    Task<List<Workspace>> GetAllWorkspacesAsync();
    Task<Workspace?> GetWorkspaceByIdAsync(long id);
    Task<Workspace?> GetWorkspaceByNameAsync(string name);
    Task<Workspace> AddWorkspaceAsync(Workspace workspace);
    Task<Workspace> UpdateWorkspaceAsync(Workspace workspace);
    Task DeleteWorkspaceAsync(long id);
    Task<bool> WorkspaceExistsAsync(string name);
    Task<List<Workspace>> GetWorkspacesByProjectAsync(long projectId);
    Task RefreshInheritedTagsAsync(Workspace workspace);
    Task AddCustomTagAsync(long workspaceId, string tag);
    Task RemoveCustomTagAsync(long workspaceId, string tag);
    Task<List<string>> GetAllWorkspaceTagsAsync();
    Task<List<Workspace>> GetWorkspacesByTagAsync(string tag);
}
