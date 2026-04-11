using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ProjectHub.Desktop.Services
{
    public class IdeDetectionService
    {
        public List<IdeTemplate> DetectInstalledIdes()
        {
            var detectedIdes = new List<IdeTemplate>();

            // 检测 VS Code
            var vscodePath = DetectVsCode();
            if (vscodePath != null)
            {
                detectedIdes.Add(new IdeTemplate
                {
                    Name = "VS Code",
                    ExecutablePath = vscodePath,
                    DefaultArgs = "",
                    Icon = "🆚",
                    Priority = 1
                });
            }

            // 检测 Trae
            var traePath = DetectTrae();
            if (traePath != null)
            {
                detectedIdes.Add(new IdeTemplate
                {
                    Name = "Trae",
                    ExecutablePath = traePath,
                    DefaultArgs = "",
                    Icon = "🤖",
                    Priority = 2
                });
            }

            // 检测 Trae-CN
            var traeCnPath = DetectTraeCn();
            if (traeCnPath != null)
            {
                detectedIdes.Add(new IdeTemplate
                {
                    Name = "Trae-CN",
                    ExecutablePath = traeCnPath,
                    DefaultArgs = "",
                    Icon = "🤖",
                    Priority = 3
                });
            }

            return detectedIdes;
        }

        private string? DetectVsCode()
        {
            // 检查 PATH 环境变量
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv != null)
            {
                var paths = pathEnv.Split(Path.PathSeparator);
                foreach (var path in paths)
                {
                    var codePath = Path.Combine(path, "code.exe");
                    if (File.Exists(codePath))
                    {
                        return codePath;
                    }
                }
            }

            // 检查常见安装路径
            var commonPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code", "code.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "code.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "Programs", "Microsoft VS Code", "code.exe")
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private string? DetectTrae()
        {
            // 检查 PATH 环境变量
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv != null)
            {
                var paths = pathEnv.Split(Path.PathSeparator);
                foreach (var path in paths)
                {
                    var traePath = Path.Combine(path, "Trae.exe");
                    if (File.Exists(traePath))
                    {
                        return traePath;
                    }
                }
            }

            // 检查常见安装路径
            var commonPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Trae", "Trae.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Trae", "Trae.exe")
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private string? DetectTraeCn()
        {
            // 检查 PATH 环境变量
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv != null)
            {
                var paths = pathEnv.Split(Path.PathSeparator);
                foreach (var path in paths)
                {
                    var traeCnPath = Path.Combine(path, "Trae.exe");
                    if (File.Exists(traeCnPath))
                    {
                        return traeCnPath;
                    }
                }
            }

            // 检查常见安装路径
            var commonPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Trae CN", "Trae.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Trae CN", "Trae.exe")
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}