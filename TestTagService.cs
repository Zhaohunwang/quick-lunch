using ProjectHub.Core.Models;
using ProjectHub.Desktop.Services;
using System;
using System.Threading.Tasks;

namespace ProjectHub.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("测试TagService...");
            
            var tagService = new TagService();
            
            // 测试添加标签
            Console.WriteLine("添加标签...");
            var tag1 = new Tag { Name = "web", Color = "#3498db" };
            await tagService.AddTagAsync(tag1);
            
            var tag2 = new Tag { Name = "backend", Color = "#2ecc71" };
            await tagService.AddTagAsync(tag2);
            
            // 测试获取标签
            Console.WriteLine("获取标签...");
            var tags = await tagService.GetAllTagsAsync();
            Console.WriteLine($"获取到 {tags.Count} 个标签");
            
            foreach (var tag in tags)
            {
                Console.WriteLine($"标签: {tag.Name}, ID: {tag.Id}, 颜色: {tag.Color}");
            }
            
            Console.WriteLine("测试完成！");
            Console.ReadLine();
        }
    }
}