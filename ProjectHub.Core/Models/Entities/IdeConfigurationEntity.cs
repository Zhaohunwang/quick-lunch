namespace ProjectHub.Core.Models.Entities;

public class IdeConfigurationEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ExecutablePath { get; set; } = string.Empty;

    public string? CommandArgs { get; set; }

    public bool IsDefault { get; set; }

    public string? Icon { get; set; }

    public long ProjectId { get; set; }

    public ProjectEntity Project { get; set; } = null!;
}
