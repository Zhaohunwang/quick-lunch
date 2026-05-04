namespace ProjectHub.Core.Models.Entities;

public class WorkspaceProjectEntity
{
    public long WorkspaceId { get; set; }

    public WorkspaceEntity Workspace { get; set; } = null!;

    public long ProjectId { get; set; }

    public ProjectEntity Project { get; set; } = null!;

    public int SortOrder { get; set; }
}
