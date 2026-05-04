namespace ProjectHub.Core.Models.Entities;

public class ProjectTagEntity
{
    public long ProjectId { get; set; }

    public ProjectEntity Project { get; set; } = null!;

    public long TagId { get; set; }

    public TagEntity Tag { get; set; } = null!;
}
