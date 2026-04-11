using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectHub.Core.Services
{
    public interface ITagService
    {
        Task<List<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(Guid id);
        Task<Tag?> GetTagByNameAsync(string name);
        Task<Tag> AddTagAsync(Tag tag);
        Task<Tag> UpdateTagAsync(Tag tag);
        Task DeleteTagAsync(Guid id);
        Task<bool> TagExistsAsync(string name);
    }
}
