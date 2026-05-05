using ProjectHub.Core.Models;

namespace ProjectHub.Core.Services;

public interface IWorkspaceLauncherService
{
    Task LaunchWorkspaceAsync(Workspace workspace, IdeTemplate? ideTemplate = null);
}
