using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectHub.Core.Services
{
    public interface IWorkspaceService
    {
        Task<List<Workspace>> GetAllWorkspacesAsync();
        Task<Workspace?> GetWorkspaceByIdAsync(Guid id);
        Task<Workspace?> GetWorkspaceByNameAsync(string name);
        Task<Workspace> AddWorkspaceAsync(Workspace workspace);
        Task<Workspace> UpdateWorkspaceAsync(Workspace workspace);
        Task DeleteWorkspaceAsync(Guid id);
        Task<bool> WorkspaceExistsAsync(string name);
        Task<List<Workspace>> GetWorkspacesByProjectAsync(Guid projectId);
        
        /// <summary>
        /// 刷新工作区的继承标签（根据包含的项目重新计算）
        /// </summary>
        Task RefreshInheritedTagsAsync(Workspace workspace);
        
        /// <summary>
        /// 为工作区添加自定义标签
        /// </summary>
        Task AddCustomTagAsync(Guid workspaceId, string tag);
        
        /// <summary>
        /// 从工作区移除自定义标签
        /// </summary>
        Task RemoveCustomTagAsync(Guid workspaceId, string tag);
        
        /// <summary>
        /// 获取所有工作区的标签（用于标签云）
        /// </summary>
        Task<List<string>> GetAllWorkspaceTagsAsync();
        
        /// <summary>
        /// 根据标签筛选工作区
        /// </summary>
        Task<List<Workspace>> GetWorkspacesByTagAsync(string tag);
    }
}
