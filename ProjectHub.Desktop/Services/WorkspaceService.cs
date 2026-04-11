using Microsoft.EntityFrameworkCore;
using ProjectHub.Core.Database;
using ProjectHub.Core.Models;
using ProjectHub.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly AppDbContext _dbContext;
        private readonly IProjectService _projectService;

        public WorkspaceService(IProjectService projectService)
        {
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            _projectService = projectService;
        }

        public async Task<List<Workspace>> GetAllWorkspacesAsync()
        {
            var workspaces = await _dbContext.Workspaces.ToListAsync();
            
            // 为每个工作区刷新继承标签
            foreach (var workspace in workspaces)
            {
                await RefreshInheritedTagsAsync(workspace);
            }
            
            return workspaces;
        }

        public async Task<Workspace?> GetWorkspaceByIdAsync(Guid id)
        {
            var workspace = await _dbContext.Workspaces.FirstOrDefaultAsync(w => w.Id == id);
            if (workspace != null)
            {
                await RefreshInheritedTagsAsync(workspace);
            }
            return workspace;
        }

        public async Task<Workspace?> GetWorkspaceByNameAsync(string name)
        {
            var workspace = await _dbContext.Workspaces.FirstOrDefaultAsync(w => w.Name == name);
            if (workspace != null)
            {
                await RefreshInheritedTagsAsync(workspace);
            }
            return workspace;
        }

        public async Task<Workspace> AddWorkspaceAsync(Workspace workspace)
        {
            workspace.CreatedAt = DateTime.UtcNow;
            _dbContext.Workspaces.Add(workspace);
            await _dbContext.SaveChangesAsync();
            
            // 刷新继承标签
            await RefreshInheritedTagsAsync(workspace);
            
            return workspace;
        }

        public async Task<Workspace> UpdateWorkspaceAsync(Workspace workspace)
        {
            workspace.UpdatedAt = DateTime.UtcNow;
            _dbContext.Workspaces.Update(workspace);
            await _dbContext.SaveChangesAsync();
            return workspace;
        }

        public async Task DeleteWorkspaceAsync(Guid id)
        {
            var workspace = await GetWorkspaceByIdAsync(id);
            if (workspace != null)
            {
                _dbContext.Workspaces.Remove(workspace);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> WorkspaceExistsAsync(string name)
        {
            return await Task.FromResult(_dbContext.Workspaces.Any(w => w.Name == name));
        }

        public async Task<List<Workspace>> GetWorkspacesByProjectAsync(Guid projectId)
        {
            var workspaces = await Task.FromResult(_dbContext.Workspaces
                .Where(w => w.ProjectIds.Contains(projectId))
                .ToList());
                
            foreach (var workspace in workspaces)
            {
                await RefreshInheritedTagsAsync(workspace);
            }
            
            return workspaces;
        }

        /// <summary>
        /// 刷新工作区的继承标签（根据包含的项目重新计算）
        /// </summary>
        public async Task RefreshInheritedTagsAsync(Workspace workspace)
        {
            if (!workspace.AutoInheritTags || !workspace.ProjectIds.Any())
            {
                workspace.InheritedTags.Clear();
                return;
            }

            // 获取所有包含的项目
            var projects = await _projectService.GetProjectsByIdsAsync(workspace.ProjectIds);
            
            // 收集所有项目的标签并去重
            var inheritedTags = projects
                .SelectMany(p => p.Tags)
                .Distinct()
                .Where(t => !workspace.CustomTags.Contains(t)) // 排除已自定义的标签
                .ToList();
            
            workspace.InheritedTags = inheritedTags;
        }

        /// <summary>
        /// 为工作区添加自定义标签
        /// </summary>
        public async Task AddCustomTagAsync(Guid workspaceId, string tag)
        {
            var workspace = await GetWorkspaceByIdAsync(workspaceId);
            if (workspace == null) return;

            var normalizedTag = tag.Trim().ToLower();
            if (string.IsNullOrEmpty(normalizedTag)) return;

            if (!workspace.CustomTags.Contains(normalizedTag))
            {
                workspace.CustomTags.Add(normalizedTag);
                await UpdateWorkspaceAsync(workspace);
            }
        }

        /// <summary>
        /// 从工作区移除自定义标签
        /// </summary>
        public async Task RemoveCustomTagAsync(Guid workspaceId, string tag)
        {
            var workspace = await GetWorkspaceByIdAsync(workspaceId);
            if (workspace == null) return;

            var normalizedTag = tag.Trim().ToLower();
            
            if (workspace.CustomTags.Remove(normalizedTag))
            {
                await UpdateWorkspaceAsync(workspace);
            }
        }

        /// <summary>
        /// 获取所有工作区的标签（用于标签云）
        /// </summary>
        public async Task<List<string>> GetAllWorkspaceTagsAsync()
        {
            var workspaces = await GetAllWorkspacesAsync();
            
            return workspaces
                .SelectMany(w => w.AllTags)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }

        /// <summary>
        /// 根据标签筛选工作区
        /// </summary>
        public async Task<List<Workspace>> GetWorkspacesByTagAsync(string tag)
        {
            var workspaces = await GetAllWorkspacesAsync();
            
            var normalizedTag = tag.Trim().ToLower();
            
            return workspaces
                .Where(w => w.AllTags.Any(t => t.Equals(normalizedTag, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
