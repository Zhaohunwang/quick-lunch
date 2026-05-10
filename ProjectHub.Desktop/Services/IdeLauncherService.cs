using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Database;
using ProjectHub.Core.Models;
using ProjectHub.Core.Models.Entities;
using ProjectHub.Core.Services;

namespace ProjectHub.Desktop.Services;

public class IdeLauncherService : IIdeLauncherService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly List<IdeTemplate> _templates = new();

    public IdeLauncherService() : this(() => new AppDbContext()) { }

    public IdeLauncherService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        InitializeDefaultTemplates();
    }

    public async Task<List<IdeTemplate>> GetAvailableIdesAsync()
    {
        using var db = _contextFactory();
        var entities = await db.IdeTemplates.ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task LaunchProjectAsync(Project project, string? ideName = null)
    {
        var templates = await GetAvailableIdesAsync();
        var template = ideName != null
            ? templates.FirstOrDefault(t => t.Name == ideName)
            : templates.FirstOrDefault();

        if (template == null) return;

        await LaunchProcessAsync(template.ExecutablePath, template.DefaultArgs, project.Path);

        using var db = _contextFactory();
        var entity = await db.Projects.FindAsync(project.Id);
        if (entity != null)
        {
            entity.LastOpenedAt = DateTime.UtcNow;
            entity.OpenCount++;
            await db.SaveChangesAsync();
        }
    }

    public async Task LaunchProjectWithTemplateAsync(Project project, IdeTemplate ideTemplate)
    {
        await LaunchProcessAsync(ideTemplate.ExecutablePath, ideTemplate.DefaultArgs, project.Path);

        using var db = _contextFactory();
        var entity = await db.Projects.FindAsync(project.Id);
        if (entity != null)
        {
            entity.LastOpenedAt = DateTime.UtcNow;
            entity.OpenCount++;
            await db.SaveChangesAsync();
        }
    }

    public async Task AddIdeTemplateAsync(IdeTemplate template)
    {
        using var db = _contextFactory();
        var entity = new IdeTemplateEntity
        {
            Name = template.Name,
            ExecutablePath = template.ExecutablePath,
            DefaultArgs = template.DefaultArgs,
            Icon = template.Icon,
            Priority = template.Priority,
            SupportedExtensionsJson = System.Text.Json.JsonSerializer.Serialize(template.SupportedExtensions)
        };
        db.IdeTemplates.Add(entity);
        await db.SaveChangesAsync();
        template.Id = entity.Id;
    }

    public async Task UpdateIdeTemplateAsync(IdeTemplate template)
    {
        using var db = _contextFactory();
        var existingEntity = await db.IdeTemplates.FindAsync(template.Id);
        if (existingEntity == null)
            throw new InvalidOperationException($"IDE Template with Id '{template.Id}' not found");

        existingEntity.Name = template.Name;
        existingEntity.ExecutablePath = template.ExecutablePath;
        existingEntity.DefaultArgs = template.DefaultArgs;
        existingEntity.Icon = template.Icon;
        existingEntity.Priority = template.Priority;
        existingEntity.SupportedExtensionsJson = System.Text.Json.JsonSerializer.Serialize(template.SupportedExtensions);
        await db.SaveChangesAsync();
    }

    public async Task DeleteIdeTemplateAsync(long id)
    {
        using var db = _contextFactory();
        var entity = await db.IdeTemplates.FindAsync(id);
        if (entity != null)
        {
            db.IdeTemplates.Remove(entity);
            await db.SaveChangesAsync();
        }
    }

    private async Task LaunchProcessAsync(string executablePath, string? args, string? workingDirectory = null)
    {
        try
        {
            ProcessStartInfo processInfo;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (executablePath.EndsWith(".app") || Directory.Exists(executablePath))
                {
                    var arguments = string.IsNullOrEmpty(args) ? "" : $" {args}";
                    processInfo = new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"-a \"{executablePath}\"{(string.IsNullOrEmpty(workingDirectory) ? "" : $" \"{workingDirectory}\"")}{arguments}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else
                {
                    var launchArgs = args ?? string.Empty;
                    if (!string.IsNullOrEmpty(workingDirectory))
                        launchArgs = $"{launchArgs} \"{workingDirectory}\"".Trim();

                    processInfo = new ProcessStartInfo
                    {
                        FileName = executablePath,
                        Arguments = launchArgs,
                        UseShellExecute = false
                    };
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (!string.IsNullOrEmpty(workingDirectory) && File.Exists(executablePath))
                {
                    processInfo = new ProcessStartInfo
                    {
                        FileName = executablePath,
                        Arguments = $"{args ?? string.Empty} \"{workingDirectory}\"".Trim(),
                        UseShellExecute = false
                    };
                }
                else
                {
                    processInfo = new ProcessStartInfo
                    {
                        FileName = "gio",
                        Arguments = $"open \"{workingDirectory}\"",
                        UseShellExecute = false
                    };
                }
            }
            else
            {
                processInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = args ?? string.Empty,
                    WorkingDirectory = workingDirectory ?? string.Empty,
                    UseShellExecute = true
                };
            }

            await Task.Run(() => Process.Start(processInfo));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to launch IDE: {ex.Message}");
            throw;
        }
    }

    private void InitializeDefaultTemplates()
    {
        _templates.AddRange(new[]
        {
            new IdeTemplate
            {
                Id = 1,
                Name = "VS Code",
                ExecutablePath = "code",
                DefaultArgs = "",
                SupportedExtensions = new List<string> { "js", "ts", "py", "csharp", "json" },
                Priority = 1
            },
            new IdeTemplate
            {
                Id = 2,
                Name = "Visual Studio",
                ExecutablePath = "devenv",
                DefaultArgs = "",
                SupportedExtensions = new List<string> { "sln", "csproj", "cs" },
                Priority = 2
            },
            new IdeTemplate
            {
                Id = 3,
                Name = "Rider",
                ExecutablePath = "rider",
                DefaultArgs = "",
                SupportedExtensions = new List<string> { "sln", "csproj", "kt", "java" },
                Priority = 3
            },
            new IdeTemplate
            {
                Id = 4,
                Name = "IntelliJ IDEA",
                ExecutablePath = "idea",
                DefaultArgs = "",
                SupportedExtensions = new List<string> { "java", "kt", "xml" },
                Priority = 4
            }
        });
    }
}
