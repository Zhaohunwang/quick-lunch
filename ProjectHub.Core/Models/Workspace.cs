namespace ProjectHub.Core.Models;

public class Workspace
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<long> ProjectIds { get; set; } = new();

    public List<string> CustomTags { get; set; } = new();

    public List<string> InheritedTags { get; set; } = new();

    public bool AutoInheritTags { get; set; } = true;

    public List<string> AllTags => AutoInheritTags
        ? CustomTags.Union(InheritedTags).ToList()
        : CustomTags;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string SearchText => $"{Name} {Description} {string.Join(" ", AllTags)}".ToLower();
}
