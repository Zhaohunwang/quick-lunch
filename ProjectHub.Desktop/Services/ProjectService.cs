using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Database;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _dbContext;

        public ProjectService()
        {
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _dbContext.Projects.Include(p => p.IdeConfigurations).ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(Guid id)
        {
            return await _dbContext.Projects.Include(p => p.IdeConfigurations).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project> AddProjectAsync(Project project)
        {
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            
            var existingProject = await _dbContext.Projects.FindAsync(project.Id);
            if (existingProject != null)
            {
                _dbContext.Entry(existingProject).CurrentValues.SetValues(project);
                await _dbContext.SaveChangesAsync();
                return existingProject;
            }
            
            _dbContext.Projects.Update(project);
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task DeleteProjectAsync(Guid id)
        {
            var project = await GetProjectByIdAsync(id);
            if (project != null)
            {
                _dbContext.Projects.Remove(project);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Project>> GetRecentProjectsAsync(int limit = 10)
        {
            return await Task.FromResult(_dbContext.Projects
                .Include(p => p.IdeConfigurations)
                .OrderByDescending(p => p.LastOpenedAt)
                .Take(limit)
                .ToList());
        }

        public async Task<List<Project>> GetFavoriteProjectsAsync()
        {
            return await Task.FromResult(_dbContext.Projects
                .Include(p => p.IdeConfigurations)
                .Where(p => p.IsFavorite)
                .ToList());
        }

        public async Task<List<Project>> GetProjectsByTagAsync(string tag)
        {
            return await Task.FromResult(_dbContext.Projects
                .Include(p => p.IdeConfigurations)
                .Where(p => p.Tags.Contains(tag))
                .ToList());
        }

        public async Task<List<Project>> GetProjectsByIdsAsync(List<Guid> ids)
        {
            return await Task.FromResult(_dbContext.Projects
                .Include(p => p.IdeConfigurations)
                .Where(p => ids.Contains(p.Id))
                .ToList());
        }
    }
}
