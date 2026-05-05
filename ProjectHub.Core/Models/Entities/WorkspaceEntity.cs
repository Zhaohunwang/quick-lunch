namespace ProjectHub.Core.Models.Entities;

public class WorkspaceEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long? DefaultIdeId { get; set; }

    public bool AutoInheritTags { get; set; } = true;

    public string CustomTagsJson { get; set; } = "[]";

    public bool IsFavorite { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<WorkspaceProjectEntity> WorkspaceProjects { get; set; } = new();
}
