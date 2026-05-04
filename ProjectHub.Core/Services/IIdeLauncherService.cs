using ProjectHub.Core.Models;

namespace ProjectHub.Core.Services;

public interface IIdeLauncherService
{
    Task LaunchProjectAsync(Project project, string? ideName = null);
    Task LaunchProjectWithTemplateAsync(Project project, IdeTemplate ideTemplate);
    Task<List<IdeTemplate>> GetAvailableIdesAsync();
    Task AddIdeTemplateAsync(IdeTemplate template);
    Task UpdateIdeTemplateAsync(IdeTemplate template);
    Task DeleteIdeTemplateAsync(long id);
}
