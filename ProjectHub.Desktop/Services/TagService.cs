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

public class TagService : ITagService
{
    private readonly Func<AppDbContext> _contextFactory;

    public TagService() : this(() => new AppDbContext()) { }

    public TagService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        using var db = _contextFactory();
        var entities = await db.Tags.ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<List<string>> GetAllTagNamesAsync()
    {
        using var db = _contextFactory();
        return await db.Tags.Select(t => t.Name).ToListAsync();
    }

    public async Task<Tag?> GetTagByIdAsync(long id)
    {
        using var db = _contextFactory();
        var entity = await db.Tags.FindAsync(id);
        return entity?.ToDomain();
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        using var db = _contextFactory();
        var entity = await db.Tags.FirstOrDefaultAsync(t => t.Name == name);
        return entity?.ToDomain();
    }

    public async Task<Tag> AddTagAsync(Tag tag)
    {
        using var db = _contextFactory();
        var entity = tag.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        db.Tags.Add(entity);
        await db.SaveChangesAsync();
        tag.Id = entity.Id;
        return tag;
    }

    public async Task<Tag> UpdateTagAsync(Tag tag)
    {
        using var db = _contextFactory();
        var existingEntity = await db.Tags.FindAsync(tag.Id);
        if (existingEntity == null)
            throw new InvalidOperationException($"Tag with Id '{tag.Id}' not found");

        existingEntity.Name = tag.Name;
        existingEntity.Color = tag.Color;
        await db.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteTagAsync(long id)
    {
        using var db = _contextFactory();
        var entity = await db.Tags
            .Include(t => t.ProjectTags)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (entity != null)
        {
            foreach (var link in entity.ProjectTags.ToList())
                db.ProjectTags.Remove(link);
            db.Tags.Remove(entity);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> TagExistsAsync(string name)
    {
        using var db = _contextFactory();
        return await db.Tags.AnyAsync(t => t.Name == name);
    }
}
