using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Database;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.Services
{
    public class TagService : ITagService
    {
        private readonly AppDbContext _dbContext;

        public TagService()
        {
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            var tags = await _dbContext.Tags.ToListAsync();
            Console.WriteLine($"获取到标签数量: {tags.Count}");
            foreach (var tag in tags)
            {
                Console.WriteLine($"标签: {tag.Name}, ID: {tag.Id}");
            }
            return tags;
        }

        public async Task<List<string>> GetAllTagNamesAsync()
        {
            var tags = await _dbContext.Tags.ToListAsync();
            return tags.Select(t => t.Name).ToList();
        }

        public async Task<Tag?> GetTagByIdAsync(Guid id)
        {
            return await _dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            return await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Tag> AddTagAsync(Tag tag)
        {
            tag.CreatedAt = DateTime.UtcNow;
            _dbContext.Tags.Add(tag);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"添加标签成功: {tag.Name}, ID: {tag.Id}");
            return tag;
        }

        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            _dbContext.Tags.Update(tag);
            await _dbContext.SaveChangesAsync();
            return tag;
        }

        public async Task DeleteTagAsync(Guid id)
        {
            var tag = await GetTagByIdAsync(id);
            if (tag != null)
            {
                _dbContext.Tags.Remove(tag);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> TagExistsAsync(string name)
        {
            return await _dbContext.Tags.AnyAsync(t => t.Name == name);
        }
    }
}
