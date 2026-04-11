using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Database;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.Services
{
    public class IdeLauncherService : IIdeLauncherService
    {
        private readonly AppDbContext _dbContext;

        public IdeLauncherService()
        {
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
        }

        public async Task LaunchProjectAsync(Project project, string? ideName = null)
        {
            IdeConfiguration? ideConfig = null;

            if (!string.IsNullOrEmpty(ideName))
            {
                ideConfig = project.IdeConfigurations.FirstOrDefault(ic => ic.Name == ideName);
            }
            else if (project.DefaultIdeId.HasValue)
            {
                ideConfig = project.IdeConfigurations.FirstOrDefault(ic => ic.Id == project.DefaultIdeId.Value);
            }
            else if (project.IdeConfigurations.Count > 0)
            {
                ideConfig = project.IdeConfigurations.FirstOrDefault(ic => ic.IsDefault) ?? project.IdeConfigurations.First();
            }

            if (ideConfig != null)
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = ideConfig.ExecutablePath,
                    Arguments = $"{(string.IsNullOrEmpty(ideConfig.CommandArgs) ? string.Empty : ideConfig.CommandArgs)} \"{project.Path}\"",
                    UseShellExecute = true
                };

                Process.Start(processStartInfo);

                // 更新项目的访问时间和次数
                project.LastOpenedAt = DateTime.UtcNow;
                project.OpenCount++;
                _dbContext.Projects.Update(project);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<IdeTemplate>> GetAvailableIdesAsync()
        {
            return await Task.FromResult(_dbContext.IdeTemplates.ToList());
        }

        public async Task AddIdeTemplateAsync(IdeTemplate template)
        {
            _dbContext.IdeTemplates.Add(template);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateIdeTemplateAsync(IdeTemplate template)
        {
            _dbContext.IdeTemplates.Update(template);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteIdeTemplateAsync(Guid id)
        {
            var template = _dbContext.IdeTemplates.FirstOrDefault(t => t.Id == id);
            if (template != null)
            {
                _dbContext.IdeTemplates.Remove(template);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}