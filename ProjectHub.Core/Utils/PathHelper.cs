using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Core.Utils
{
    public static class PathHelper
    {
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetParentDirectoryName(string path)
        {
            return Path.GetFileName(Path.GetDirectoryName(path) ?? string.Empty);
        }

        public static bool IsValidPath(string path)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                return Directory.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }
    }
}
