using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Models
{
    public class Workspace
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public List<Guid> ProjectIds { get; set; } = new();

        /// <summary>
        /// 自定义标签（用户手动添加的）
        /// </summary>
        public List<string> CustomTags { get; set; } = new();

        /// <summary>
        /// 继承自项目的标签（自动计算，不持久化）
        /// </summary>
        public List<string> InheritedTags { get; set; } = new();

        /// <summary>
        /// 是否自动继承项目标签
        /// </summary>
        public bool AutoInheritTags { get; set; } = true;

        /// <summary>
        /// 所有标签（自定义+继承，去重）
        /// </summary>
        public List<string> AllTags => AutoInheritTags
            ? CustomTags.Union(InheritedTags).ToList()
            : CustomTags;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 获取搜索文本（用于搜索功能）
        /// </summary>
        public string SearchText => $"{Name} {Description} {string.Join(" ", AllTags)}".ToLower();
    }
}
