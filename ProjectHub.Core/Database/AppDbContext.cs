using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Models.Entities;

namespace ProjectHub.Core.Database;

public class AppDbContext : DbContext
{
    public DbSet<ProjectEntity> Projects { get; set; } = null!;
    public DbSet<IdeConfigurationEntity> IdeConfigurations { get; set; } = null!;
    public DbSet<IdeTemplateEntity> IdeTemplates { get; set; } = null!;
    public DbSet<WorkspaceEntity> Workspaces { get; set; } = null!;
    public DbSet<TagEntity> Tags { get; set; } = null!;
    public DbSet<ProjectTagEntity> ProjectTags { get; set; } = null!;
    public DbSet<WorkspaceProjectEntity> WorkspaceProjects { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProjectHub",
            "projecthub.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectEntity>(e =>
        {
            e.ToTable("Projects");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.Alias).HasMaxLength(256);
            e.Property(x => x.Path).HasMaxLength(1024).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2048);
            e.Property(x => x.Color).HasMaxLength(16);
            e.Property(x => x.Icon).HasMaxLength(64);
        });

        modelBuilder.Entity<TagEntity>(e =>
        {
            e.ToTable("Tags");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.Color).HasMaxLength(16);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<ProjectTagEntity>(e =>
        {
            e.ToTable("ProjectTags");
            e.HasKey(x => new { x.ProjectId, x.TagId });
            e.HasOne(x => x.Project)
                .WithMany(p => p.ProjectTags)
                .HasForeignKey(x => x.ProjectId);
            e.HasOne(x => x.Tag)
                .WithMany(t => t.ProjectTags)
                .HasForeignKey(x => x.TagId);
        });

        modelBuilder.Entity<IdeConfigurationEntity>(e =>
        {
            e.ToTable("IdeConfigurations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.ExecutablePath).HasMaxLength(1024).IsRequired();
            e.Property(x => x.CommandArgs).HasMaxLength(1024);
            e.Property(x => x.Icon).HasMaxLength(64);
            e.HasOne(x => x.Project)
                .WithMany(p => p.IdeConfigurations)
                .HasForeignKey(x => x.ProjectId);
        });

        modelBuilder.Entity<IdeTemplateEntity>(e =>
        {
            e.ToTable("IdeTemplates");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.ExecutablePath).HasMaxLength(1024).IsRequired();
            e.Property(x => x.DefaultArgs).HasMaxLength(1024);
            e.Property(x => x.Icon).HasMaxLength(64);
            e.Property(x => x.SupportedExtensionsJson).HasMaxLength(4096);
        });

        modelBuilder.Entity<WorkspaceEntity>(e =>
        {
            e.ToTable("Workspaces");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2048);
            e.Property(x => x.CustomTagsJson).HasMaxLength(4096);
        });

        modelBuilder.Entity<WorkspaceProjectEntity>(e =>
        {
            e.ToTable("WorkspaceProjects");
            e.HasKey(x => new { x.WorkspaceId, x.ProjectId });
            e.HasOne(x => x.Workspace)
                .WithMany(w => w.WorkspaceProjects)
                .HasForeignKey(x => x.WorkspaceId);
            e.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId);
            e.Property(x => x.SortOrder).HasDefaultValue(0);
        });
    }
}
