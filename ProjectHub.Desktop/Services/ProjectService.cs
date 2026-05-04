using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Database;
using ProjectHub.Core.Models;
using ProjectHub.Core.Models.Entities;
using ProjectHub.Core.Services;

namespace ProjectHub.Desktop.Services;

public class ProjectService : IProjectService
{
    private readonly Func<AppDbContext> _contextFactory;

    public ProjectService() : this(() => new AppDbContext()) { }

    public ProjectService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Project>> GetAllProjectsAsync()
    {
        using var db = _contextFactory();
        var entities = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<Project?> GetProjectByIdAsync(long id)
    {
        using var db = _contextFactory();
        var entity = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
        return entity?.ToDomain();
    }

    public async Task<Project> AddProjectAsync(Project project)
    {
        using var db = _contextFactory();
        var entity = project.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        if (project.Tags.Count > 0)
        {
            foreach (var tagName in project.Tags)
            {
                var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (tag == null)
                {
                    tag = new TagEntity { Name = tagName, CreatedAt = DateTime.UtcNow };
                    db.Tags.Add(tag);
                    await db.SaveChangesAsync();
                }
                entity.ProjectTags.Add(new ProjectTagEntity
                {
                    Project = entity,
                    Tag = tag
                });
            }
        }

        if (project.IdeConfigurations.Count > 0)
        {
            foreach (var config in project.IdeConfigurations)
            {
                var configEntity = config.ToEntity();
                configEntity.Project = entity;
                entity.IdeConfigurations.Add(configEntity);
            }
        }

        db.Projects.Add(entity);
        await db.SaveChangesAsync();
        project.Id = entity.Id;
        return project;
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        using var db = _contextFactory();
        var existingEntity = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == project.Id);

        if (existingEntity == null)
            throw new InvalidOperationException($"Project with Id '{project.Id}' not found");

        existingEntity.Name = project.Name;
        existingEntity.Alias = project.Alias;
        existingEntity.Path = project.Path;
        existingEntity.Description = project.Description;
        existingEntity.Color = project.Color;
        existingEntity.Icon = project.Icon;
        existingEntity.DefaultIdeId = project.DefaultIdeId;
        existingEntity.LastOpenedAt = project.LastOpenedAt;
        existingEntity.OpenCount = project.OpenCount;
        existingEntity.IsFavorite = project.IsFavorite;
        existingEntity.UpdatedAt = DateTime.UtcNow;

        var currentTagIds = existingEntity.ProjectTags.Select(pt => pt.TagId).ToList();
        var newTagNames = project.Tags;

        var removeTagLinks = existingEntity.ProjectTags
            .Where(pt => !newTagNames.Contains(pt.Tag.Name))
            .ToList();
        foreach (var link in removeTagLinks)
        {
            existingEntity.ProjectTags.Remove(link);
        }

        foreach (var tagName in newTagNames)
        {
            if (existingEntity.ProjectTags.Any(pt => pt.Tag.Name == tagName))
                continue;

            var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new TagEntity { Name = tagName, CreatedAt = DateTime.UtcNow };
                db.Tags.Add(tag);
                await db.SaveChangesAsync();
            }
            existingEntity.ProjectTags.Add(new ProjectTagEntity
            {
                ProjectId = existingEntity.Id,
                TagId = tag.Id
            });
        }

        var existingConfigs = existingEntity.IdeConfigurations.ToList();
        var incomingConfigs = project.IdeConfigurations;

        foreach (var existingConfig in existingConfigs)
        {
            var match = incomingConfigs.FirstOrDefault(c => c.Id == existingConfig.Id);
            if (match != null)
            {
                existingConfig.Name = match.Name;
                existingConfig.ExecutablePath = match.ExecutablePath;
                existingConfig.CommandArgs = match.CommandArgs;
                existingConfig.IsDefault = match.IsDefault;
                existingConfig.Icon = match.Icon;
            }
            else
            {
                db.IdeConfigurations.Remove(existingConfig);
            }
        }

        foreach (var newConfig in incomingConfigs.Where(c => c.Id == 0))
        {
            var configEntity = newConfig.ToEntity();
            configEntity.ProjectId = existingEntity.Id;
            db.IdeConfigurations.Add(configEntity);
        }

        await db.SaveChangesAsync();
        return project;
    }

    public async Task DeleteProjectAsync(long id)
    {
        using var db = _contextFactory();
        var entity = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (entity != null)
        {
            foreach (var config in entity.IdeConfigurations.ToList())
                db.IdeConfigurations.Remove(config);
            foreach (var tag in entity.ProjectTags.ToList())
                db.ProjectTags.Remove(tag);
            db.Projects.Remove(entity);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<Project>> GetRecentProjectsAsync(int limit = 10)
    {
        using var db = _contextFactory();
        var entities = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.LastOpenedAt)
            .Take(limit)
            .ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<List<Project>> GetFavoriteProjectsAsync()
    {
        using var db = _contextFactory();
        var entities = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.IsFavorite)
            .ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<List<Project>> GetProjectsByTagAsync(string tag)
    {
        using var db = _contextFactory();
        var entities = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.ProjectTags.Any(pt => pt.Tag.Name == tag))
            .ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<List<Project>> GetProjectsByIdsAsync(List<long> ids)
    {
        using var db = _contextFactory();
        var entities = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }
}
