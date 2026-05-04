using ProjectHub.Core.Models;

namespace ProjectHub.Core.Services;

public interface ITagService
{
    Task<List<Tag>> GetAllTagsAsync();
    Task<List<string>> GetAllTagNamesAsync();
    Task<Tag?> GetTagByIdAsync(long id);
    Task<Tag?> GetTagByNameAsync(string name);
    Task<Tag> AddTagAsync(Tag tag);
    Task<Tag> UpdateTagAsync(Tag tag);
    Task DeleteTagAsync(long id);
    Task<bool> TagExistsAsync(string name);
}
