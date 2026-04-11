using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Models
{
    public class AppSettings
    {
        public string HotKeyQuickLaunch { get; set; } = "Ctrl+Shift+P";

        public string HotKeyOpenManager { get; set; } = "Ctrl+Shift+O";

        public string Theme { get; set; } = "System";

        public string Language { get; set; } = "zh-CN";

        public bool StartWithSystem { get; set; } = false;

        public bool MinimizeToTray { get; set; } = true;

        public int MaxRecentProjects { get; set; } = 10;
    }
}
