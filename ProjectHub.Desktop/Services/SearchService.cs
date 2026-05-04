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

public class SearchService : ISearchService
{
    private readonly Func<AppDbContext> _contextFactory;

    public SearchService() : this(() => new AppDbContext()) { }

    public SearchService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Project>> SearchProjectsAsync(string query)
    {
        using var db = _contextFactory();
        var entities = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .ToListAsync();
        var projects = entities.Select(e => e.ToDomain()).ToList();
        return projects.Where(p => p.SearchText.Contains(query.ToLower())).ToList();
    }

    public async Task<List<Workspace>> SearchWorkspacesAsync(string query)
    {
        using var db = _contextFactory();
        var workspaceEntities = await db.Workspaces
            .Include(w => w.WorkspaceProjects)
            .ToListAsync();

        var results = new List<Workspace>();
        foreach (var we in workspaceEntities)
        {
            var projectIds = we.WorkspaceProjects.OrderBy(wp => wp.SortOrder).Select(wp => wp.ProjectId).ToList();
            var inheritedTags = await ComputeInheritedTagsAsync(projectIds);
            var workspace = we.ToDomain(inheritedTags);
            if (workspace.SearchText.Contains(query.ToLower()))
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

    public async Task<List<Project>> SearchProjectsByTagsAsync(IEnumerable<string> tags)
    {
        var tagList = tags.ToList();
        using var db = _contextFactory();
        var entities = await db.Projects
            .Include(p => p.IdeConfigurations)
            .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
            .ToListAsync();
        return entities
            .Where(e => tagList.All(t => e.ProjectTags.Any(pt => pt.Tag.Name == t)))
            .Select(e => e.ToDomain())
            .ToList();
    }
}
