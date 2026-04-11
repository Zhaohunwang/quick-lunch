using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Services
{
    public interface ISettingsService
    {
        Task<AppSettings> GetSettingsAsync();

        Task SaveSettingsAsync(AppSettings settings);
    }
}
