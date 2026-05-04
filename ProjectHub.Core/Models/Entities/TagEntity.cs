namespace ProjectHub.Core.Models.Entities;

public class TagEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = "#3498db";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ProjectTagEntity> ProjectTags { get; set; } = new();
}
