namespace ProjectHub.Core.Models.Entities;

public class IdeTemplateEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ExecutablePath { get; set; } = string.Empty;

    public string? DefaultArgs { get; set; }

    public string? Icon { get; set; }

    public string SupportedExtensionsJson { get; set; } = "[]";

    public int Priority { get; set; }
}
