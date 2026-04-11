using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Services
{
    public interface ISearchService
    {
        Task<List<Project>> SearchProjectsAsync(string query);

        Task<List<Project>> SearchProjectsByTagsAsync(IEnumerable<string> tags);
    }
}
