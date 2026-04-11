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
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _dbContext;

        public SearchService()
        {
            _dbContext = new AppDbContext();
        }

        public async Task<List<Project>> SearchProjectsAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return await _dbContext.Projects.Include(p => p.IdeConfigurations).ToListAsync();
            }

            var lowerQuery = query.ToLower();
            var projects = await _dbContext.Projects.Include(p => p.IdeConfigurations).ToListAsync();
            return projects.Where(p => p.SearchText.Contains(lowerQuery)).ToList();
        }

        public async Task<List<Project>> SearchProjectsByTagsAsync(IEnumerable<string> tags)
        {
            var tagList = tags.ToList();
            if (!tagList.Any())
            {
                return await Task.FromResult(_dbContext.Projects.Include(p => p.IdeConfigurations).ToList());
            }

            return await Task.FromResult(_dbContext.Projects
                .Include(p => p.IdeConfigurations)
                .Where(p => tagList.All(tag => p.Tags.Contains(tag)))
                .ToList());
        }
    }
}