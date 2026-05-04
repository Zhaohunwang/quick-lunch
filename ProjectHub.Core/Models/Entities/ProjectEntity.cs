namespace ProjectHub.Core.Models.Entities;

public class ProjectEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Alias { get; set; }

    public string Path { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public long? DefaultIdeId { get; set; }

    public DateTime LastOpenedAt { get; set; }

    public int OpenCount { get; set; }

    public bool IsFavorite { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<IdeConfigurationEntity> IdeConfigurations { get; set; } = new();

    public List<ProjectTagEntity> ProjectTags { get; set; } = new();
}
