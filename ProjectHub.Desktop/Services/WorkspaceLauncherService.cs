using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;

namespace ProjectHub.Desktop.Services;

public class WorkspaceLauncherService : IWorkspaceLauncherService
{
    private readonly IProjectService _projectService;

    public WorkspaceLauncherService(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task LaunchWorkspaceAsync(Workspace workspace, IdeTemplate? ideTemplate = null)
    {
        if (ideTemplate == null)
            throw new InvalidOperationException("No IDE template provided for workspace launch");

        var projects = await _projectService.GetAllProjectsAsync();
        var workspaceProjects = workspace.ProjectIds
            .Select(id => projects.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .ToList();

        if (workspaceProjects.Count == 0)
            throw new InvalidOperationException("Workspace has no projects with valid paths");

        var folders = workspaceProjects
            .Where(p => p!.Path.Contains(Path.DirectorySeparatorChar) || p!.Path.Contains(Path.AltDirectorySeparatorChar))
            .Select(p => new { path = p!.Path })
            .ToList();

        if (folders.Count == 0)
            throw new InvalidOperationException("No valid folder paths found in workspace projects");

        var workspaceData = new
        {
            folders = folders
        };

        var tempDir = Path.Combine(Path.GetTempPath(), "ProjectHub", "Workspaces");
        Directory.CreateDirectory(tempDir);

        var safeName = string.Join("_", workspace.Name.Split(Path.GetInvalidFileNameChars()));
        var workspaceFilePath = Path.Combine(tempDir, $"{safeName}.code-workspace");

        var json = JsonSerializer.Serialize(workspaceData, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(workspaceFilePath, json);

        await LaunchProcessAsync(ideTemplate.ExecutablePath, ideTemplate.DefaultArgs, workspaceFilePath);
    }

    private static async Task LaunchProcessAsync(string executablePath, string? args, string? fileToOpen = null)
    {
        try
        {
            var arguments = fileToOpen != null
                ? $"\"{fileToOpen}\" {args ?? string.Empty}".Trim()
                : args ?? string.Empty;

            var processInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                UseShellExecute = true
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                processInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    UseShellExecute = false
                };
            }

            await Task.Run(() => Process.Start(processInfo));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to launch IDE for workspace: {ex.Message}");
            throw;
        }
    }
}
