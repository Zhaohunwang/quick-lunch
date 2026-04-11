using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<IdeConfiguration> IdeConfigurations { get; set; }
        public DbSet<IdeTemplate> IdeTemplates { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProjectHub", "projecthub.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasMany(p => p.IdeConfigurations)
                .WithOne(ic => ic.Project)
                .HasForeignKey(ic => ic.ProjectId);
        }
    }
}
