using System.Text.Json;
using ProjectHub.Core.Models;
using ProjectHub.Core.Models.Entities;

namespace ProjectHub.Core.Database;

public static class EntityMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static Project ToDomain(this ProjectEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Alias = entity.Alias,
        Path = entity.Path,
        Description = entity.Description,
        Color = entity.Color,
        Icon = entity.Icon,
        DefaultIdeId = entity.DefaultIdeId,
        LastOpenedAt = entity.LastOpenedAt,
        OpenCount = entity.OpenCount,
        IsFavorite = entity.IsFavorite,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        Tags = entity.ProjectTags.Select(pt => pt.Tag.Name).ToList(),
        IdeConfigurations = entity.IdeConfigurations.Select(ic => ic.ToDomain()).ToList()
    };

    public static ProjectEntity ToEntity(this Project domain) => new()
    {
        Id = domain.Id,
        Name = domain.Name,
        Alias = domain.Alias,
        Path = domain.Path,
        Description = domain.Description,
        Color = domain.Color,
        Icon = domain.Icon,
        DefaultIdeId = domain.DefaultIdeId,
        LastOpenedAt = domain.LastOpenedAt,
        OpenCount = domain.OpenCount,
        IsFavorite = domain.IsFavorite,
        CreatedAt = domain.CreatedAt,
        UpdatedAt = domain.UpdatedAt
    };

    public static Tag ToDomain(this TagEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Color = entity.Color,
        CreatedAt = entity.CreatedAt
    };

    public static TagEntity ToEntity(this Tag domain) => new()
    {
        Id = domain.Id,
        Name = domain.Name,
        Color = domain.Color,
        CreatedAt = domain.CreatedAt
    };

    public static IdeConfiguration ToDomain(this IdeConfigurationEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        ExecutablePath = entity.ExecutablePath,
        CommandArgs = entity.CommandArgs,
        IsDefault = entity.IsDefault,
        Icon = entity.Icon,
        ProjectId = entity.ProjectId
    };

    public static IdeConfigurationEntity ToEntity(this IdeConfiguration domain) => new()
    {
        Id = domain.Id,
        Name = domain.Name,
        ExecutablePath = domain.ExecutablePath,
        CommandArgs = domain.CommandArgs,
        IsDefault = domain.IsDefault,
        Icon = domain.Icon,
        ProjectId = domain.ProjectId
    };

    public static IdeTemplate ToDomain(this IdeTemplateEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        ExecutablePath = entity.ExecutablePath,
        DefaultArgs = entity.DefaultArgs,
        Icon = entity.Icon,
        Priority = entity.Priority,
        SupportedExtensions = JsonSerializer.Deserialize<List<string>>(entity.SupportedExtensionsJson, JsonOptions) ?? new List<string>()
    };

    public static IdeTemplateEntity ToEntity(this IdeTemplate domain) => new()
    {
        Id = domain.Id,
        Name = domain.Name,
        ExecutablePath = domain.ExecutablePath,
        DefaultArgs = domain.DefaultArgs,
        Icon = domain.Icon,
        Priority = domain.Priority,
        SupportedExtensionsJson = JsonSerializer.Serialize(domain.SupportedExtensions, JsonOptions)
    };

    public static Workspace ToDomain(this WorkspaceEntity entity, List<string> inheritedTags) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        AutoInheritTags = entity.AutoInheritTags,
        CustomTags = JsonSerializer.Deserialize<List<string>>(entity.CustomTagsJson, JsonOptions) ?? new List<string>(),
        InheritedTags = inheritedTags,
        ProjectIds = entity.WorkspaceProjects.OrderBy(wp => wp.SortOrder).Select(wp => wp.ProjectId).ToList(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static WorkspaceEntity ToEntity(this Workspace domain) => new()
    {
        Id = domain.Id,
        Name = domain.Name,
        Description = domain.Description,
        AutoInheritTags = domain.AutoInheritTags,
        CustomTagsJson = JsonSerializer.Serialize(domain.CustomTags, JsonOptions),
        CreatedAt = domain.CreatedAt,
        UpdatedAt = domain.UpdatedAt
    };
}
