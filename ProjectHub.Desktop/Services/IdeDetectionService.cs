using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ProjectHub.Desktop.Services
{
    public class IdeDetectionService
    {
        private List<IdeDetectionRule>? _cachedRules;

        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        private static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public List<IdeTemplate> DetectInstalledIdes()
        {
            var rules = GetDetectionRules();
            var detectedIdes = new List<IdeTemplate>();

            foreach (var rule in rules)
            {
                var platformExe = GetPlatformValue(rule.ExeName);
                if (string.IsNullOrEmpty(platformExe)) continue;

                var platformPaths = GetPlatformPaths(rule.SearchPaths);
                var resolvedPaths = platformPaths.Select(ExpandPath).ToArray();

                var exePath = FindExecutable(platformExe, resolvedPaths);
                if (exePath != null)
                {
                    detectedIdes.Add(new IdeTemplate
                    {
                        Name = rule.Name,
                        ExecutablePath = exePath,
                        DefaultArgs = rule.DefaultArgs ?? "",
                        Icon = rule.Icon ?? "💻",
                        Priority = rule.Priority,
                        SupportedExtensions = rule.SupportedExtensions
                    });
                }
            }

            return detectedIdes.OrderBy(ide => ide.Priority).ToList();
        }

        private static string? GetPlatformValue(PlatformValue value)
        {
            if (IsWindows) return value.Windows;
            if (IsMacOS) return value.MacOS;
            if (IsLinux) return value.Linux;
            return null;
        }

        private static List<string> GetPlatformPaths(PlatformPaths paths)
        {
            if (IsWindows) return paths.Windows;
            if (IsMacOS) return paths.MacOS;
            if (IsLinux) return paths.Linux;
            return new List<string>();
        }

        private List<IdeDetectionRule> GetDetectionRules()
        {
            if (_cachedRules != null) return _cachedRules;

            var rules = LoadEmbeddedConfig();
            var externalRules = LoadExternalConfig();

            if (externalRules != null && externalRules.Any())
            {
                var mergedIds = externalRules.Select(r => r.Id).ToHashSet();
                var additionalRules = rules.Where(r => !mergedIds.Contains(r.Id));
                rules = externalRules.Concat(additionalRules).ToList();
            }

            _cachedRules = rules;
            return rules;
        }

        private List<IdeDetectionRule> LoadEmbeddedConfig()
        {
            try
            {
                using var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("ide_detection.json");
                if (stream == null) return new List<IdeDetectionRule>();
                return ParseConfig(stream);
            }
            catch
            {
                return new List<IdeDetectionRule>();
            }
        }

        private List<IdeDetectionRule>? LoadExternalConfig()
        {
            var configPath = GetExternalConfigPath();
            if (!File.Exists(configPath)) return null;

            try
            {
                using var fs = File.OpenRead(configPath);
                return ParseConfig(fs);
            }
            catch
            {
                return null;
            }
        }

        private static string GetExternalConfigPath()
        {
            if (IsWindows)
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "ProjectHub", "ide_detection.json");
            }

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(home))
                home = Environment.GetEnvironmentVariable("HOME") ?? "";
            return Path.Combine(home, ".config", "ProjectHub", "ide_detection.json");
        }

        private static List<IdeDetectionRule> ParseConfig(Stream jsonStream)
        {
            var doc = JsonDocument.Parse(jsonStream);
            if (!doc.RootElement.TryGetProperty("ides", out var idesElement))
                return new List<IdeDetectionRule>();

            var rules = new List<IdeDetectionRule>();
            foreach (var item in idesElement.EnumerateArray())
            {
                try
                {
                    var rule = new IdeDetectionRule
                    {
                        Id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "",
                        Name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "",
                        Icon = item.TryGetProperty("icon", out var iconProp) ? iconProp.GetString() : "💻",
                        Priority = item.TryGetProperty("priority", out var priProp) ? priProp.GetInt32() : 99,
                        DefaultArgs = item.TryGetProperty("defaultArgs", out var argsProp) ? argsProp.GetString() : ""
                    };

                    if (item.TryGetProperty("exeName", out var exeProp))
                    {
                        if (exeProp.ValueKind == JsonValueKind.Object)
                        {
                            if (exeProp.TryGetProperty("windows", out var w))
                                rule.ExeName.Windows = w.GetString();
                            if (exeProp.TryGetProperty("macOS", out var m))
                                rule.ExeName.MacOS = m.GetString();
                            if (exeProp.TryGetProperty("linux", out var l))
                                rule.ExeName.Linux = l.GetString();
                        }
                        else
                        {
                            rule.ExeName.Windows = exeProp.GetString();
                        }
                    }

                    if (item.TryGetProperty("searchPaths", out var pathsProp))
                    {
                        if (pathsProp.ValueKind == JsonValueKind.Object)
                        {
                            if (pathsProp.TryGetProperty("windows", out var wp))
                                foreach (var p in wp.EnumerateArray())
                                    rule.SearchPaths.Windows.Add(p.GetString() ?? "");
                            if (pathsProp.TryGetProperty("macOS", out var mp))
                                foreach (var p in mp.EnumerateArray())
                                    rule.SearchPaths.MacOS.Add(p.GetString() ?? "");
                            if (pathsProp.TryGetProperty("linux", out var lp))
                                foreach (var p in lp.EnumerateArray())
                                    rule.SearchPaths.Linux.Add(p.GetString() ?? "");
                        }
                    }

                    if (item.TryGetProperty("supportedExtensions", out var extProp))
                    {
                        foreach (var e in extProp.EnumerateArray())
                            rule.SupportedExtensions.Add(e.GetString() ?? "");
                    }

                    if (!string.IsNullOrEmpty(rule.Id) && !string.IsNullOrEmpty(rule.Name))
                        rules.Add(rule);
                }
                catch { }
            }

            return rules;
        }

        private static string? CommandExists(string command)
        {
            try
            {
                var shellCmd = IsWindows ? "where.exe" : "which";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = shellCmd,
                        Arguments = command,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                process.Start();
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(TimeSpan.FromSeconds(3));

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    var firstLine = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                    if (File.Exists(firstLine) || Directory.Exists(firstLine))
                        return firstLine;
                }
            }
            catch { }

            return null;
        }

        private static string? FindExecutable(string exeName, string[] searchDirs)
        {
            var result = CommandExists(exeName);
            if (result != null) return result;

            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var dir in pathEnv.Split(Path.PathSeparator))
                {
                    var fullPath = Path.Combine(dir.Trim(), exeName);
                    if (File.Exists(fullPath)) return fullPath;
                }
            }

            foreach (var dir in searchDirs)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                var fullPath = Path.Combine(dir, exeName);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return fullPath;
            }

            return null;
        }

        private static string ExpandPath(string path)
        {
            if (IsWindows)
                return Environment.ExpandEnvironmentVariables(path);

            var result = path;
            if (result.StartsWith('~'))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (string.IsNullOrEmpty(home))
                    home = Environment.GetEnvironmentVariable("HOME") ?? "";
                result = Path.Combine(home, result[1..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }
            return Environment.ExpandEnvironmentVariables(result);
        }
    }
}
