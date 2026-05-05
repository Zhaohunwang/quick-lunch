using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Database;
using ProjectHub.Core.Models;
using ProjectHub.Core.Models.Entities;
using ProjectHub.Core.Services;

namespace ProjectHub.Desktop.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly Func<AppDbContext> _contextFactory;

    public WorkspaceService() : this(() => new AppDbContext()) { }

    public WorkspaceService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Workspace>> GetAllWorkspacesAsync()
    {
        using var db = _contextFactory();
        var entities = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
                .ThenInclude(wp => wp.Project)
                    .ThenInclude(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
            .ToListAsync();
        var results = new List<Workspace>();
        foreach (var entity in entities)
        {
            var projectIds = entity.WorkspaceProjects.OrderBy(wp => wp.SortOrder).Select(wp => wp.ProjectId).ToList();
            var inheritedTags = await ComputeInheritedTagsAsync(projectIds);
            results.Add(entity.ToDomain(inheritedTags));
        }
        return results;
    }

    public async Task<Workspace?> GetWorkspaceByIdAsync(long id)
    {
        using var db = _contextFactory();
        var entity = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
                .ThenInclude(wp => wp.Project)
                    .ThenInclude(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (entity == null) return null;
        var projectIds = entity.WorkspaceProjects.OrderBy(wp => wp.SortOrder).Select(wp => wp.ProjectId).ToList();
        var inheritedTags = await ComputeInheritedTagsAsync(projectIds);
        return entity.ToDomain(inheritedTags);
    }

    public async Task<Workspace?> GetWorkspaceByNameAsync(string name)
    {
        using var db = _contextFactory();
        var entity = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
                .ThenInclude(wp => wp.Project)
                    .ThenInclude(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(w => w.Name == name);
        if (entity == null) return null;
        var projectIds = entity.WorkspaceProjects.OrderBy(wp => wp.SortOrder).Select(wp => wp.ProjectId).ToList();
        var inheritedTags = await ComputeInheritedTagsAsync(projectIds);
        return entity.ToDomain(inheritedTags);
    }

    public async Task<Workspace> AddWorkspaceAsync(Workspace workspace)
    {
        using var db = _contextFactory();
        var entity = workspace.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        foreach (var projectId in workspace.ProjectIds)
        {
            entity.WorkspaceProjects.Add(new WorkspaceProjectEntity
            {
                Workspace = entity,
                ProjectId = projectId,
                SortOrder = entity.WorkspaceProjects.Count
            });
        }

        db.Workspaces.Add(entity);
        await db.SaveChangesAsync();
        workspace.Id = entity.Id;
        return workspace;
    }

    public async Task<Workspace> UpdateWorkspaceAsync(Workspace workspace)
    {
        using var db = _contextFactory();
        var existingEntity = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
            .FirstOrDefaultAsync(w => w.Id == workspace.Id);
        if (existingEntity == null)
            throw new InvalidOperationException($"Workspace with Id '{workspace.Id}' not found");

        existingEntity.Name = workspace.Name;
        existingEntity.Description = workspace.Description;
        existingEntity.DefaultIdeId = workspace.DefaultIdeId;
        existingEntity.AutoInheritTags = workspace.AutoInheritTags;
        existingEntity.CustomTagsJson = System.Text.Json.JsonSerializer.Serialize(workspace.CustomTags);
        existingEntity.IsFavorite = workspace.IsFavorite;
        existingEntity.UpdatedAt = DateTime.UtcNow;

        var existingProjectLinks = existingEntity.WorkspaceProjects.ToList();
        foreach (var link in existingProjectLinks)
            db.WorkspaceProjects.Remove(link);

        for (int i = 0; i < workspace.ProjectIds.Count; i++)
        {
            existingEntity.WorkspaceProjects.Add(new WorkspaceProjectEntity
            {
                WorkspaceId = existingEntity.Id,
                ProjectId = workspace.ProjectIds[i],
                SortOrder = i
            });
        }

        await db.SaveChangesAsync();
        return workspace;
    }

    public async Task DeleteWorkspaceAsync(long id)
    {
        using var db = _contextFactory();
        var entity = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (entity != null)
        {
            foreach (var link in entity.WorkspaceProjects.ToList())
                db.WorkspaceProjects.Remove(link);
            db.Workspaces.Remove(entity);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> WorkspaceExistsAsync(string name)
    {
        using var db = _contextFactory();
        return await db.Workspaces.AnyAsync(w => w.Name == name);
    }

    public async Task<List<Workspace>> GetWorkspacesByProjectAsync(long projectId)
    {
        using var db = _contextFactory();
        var entities = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
                .ThenInclude(wp => wp.Project)
                    .ThenInclude(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
            .Where(w => w.WorkspaceProjects.Any(wp => wp.ProjectId == projectId))
            .ToListAsync();
        var results = new List<Workspace>();
        foreach (var entity in entities)
        {
            var projectIds = entity.WorkspaceProjects.OrderBy(wp => wp.SortOrder).Select(wp => wp.ProjectId).ToList();
            var inheritedTags = await ComputeInheritedTagsAsync(projectIds);
            results.Add(entity.ToDomain(inheritedTags));
        }
        return results;
    }

    public async Task RefreshInheritedTagsAsync(Workspace workspace)
    {
        using var db = _contextFactory();
        var entity = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
                .ThenInclude(wp => wp.Project)
                    .ThenInclude(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(w => w.Id == workspace.Id);
        if (entity != null)
        {
            var projectIds = entity.WorkspaceProjects.OrderBy(wp => wp.SortOrder).Select(wp => wp.ProjectId).ToList();
            workspace.InheritedTags = await ComputeInheritedTagsAsync(projectIds);
        }
    }

    public async Task AddCustomTagAsync(long workspaceId, string tag)
    {
        using var db = _contextFactory();
        var entity = await db.Workspaces.FindAsync(workspaceId);
        if (entity == null) return;

        var customTags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.CustomTagsJson) ?? new();
        if (!customTags.Contains(tag))
        {
            customTags.Add(tag);
            entity.CustomTagsJson = System.Text.Json.JsonSerializer.Serialize(customTags);
            entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task RemoveCustomTagAsync(long workspaceId, string tag)
    {
        using var db = _contextFactory();
        var entity = await db.Workspaces.FindAsync(workspaceId);
        if (entity == null) return;

        var customTags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.CustomTagsJson) ?? new();
        if (customTags.Remove(tag))
        {
            entity.CustomTagsJson = System.Text.Json.JsonSerializer.Serialize(customTags);
            entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetAllWorkspaceTagsAsync()
    {
        using var db = _contextFactory();
        var entities = await db.Workspaces.ToListAsync();
        var allTags = new HashSet<string>();
        foreach (var entity in entities)
        {
            var customTags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.CustomTagsJson) ?? new();
            foreach (var tag in customTags)
                allTags.Add(tag);
        }
        return allTags.ToList();
    }

    public async Task<List<Workspace>> GetWorkspacesByTagAsync(string tag)
    {
        using var db = _contextFactory();
        var entities = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
                .ThenInclude(wp => wp.Project)
                    .ThenInclude(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
            .ToListAsync();
        var results = new List<Workspace>();
        foreach (var entity in entities)
        {
            var projectIds = entity.WorkspaceProjects.OrderBy(wp => wp.SortOrder).Select(wp => wp.ProjectId).ToList();
            var inheritedTags = await ComputeInheritedTagsAsync(projectIds);
            var workspace = entity.ToDomain(inheritedTags);
            if (workspace.AllTags.Contains(tag))
                results.Add(workspace);
        }
        return results;
    }

    private async Task<List<string>> ComputeInheritedTagsAsync(List<long> projectIds)
    {
        if (projectIds.Count == 0) return new List<string>();

        using var db = _contextFactory();
        var tags = await db.ProjectTags
            .Where(pt => projectIds.Contains(pt.ProjectId))
            .Select(pt => pt.Tag.Name)
            .Distinct()
            .ToListAsync();
        return tags;
    }
}
