using ProjectHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace ProjectHub.Desktop.Services
{
    public class IdeDetectionService
    {
        private List<IdeDetectionRule>? _cachedRules;

        public List<IdeTemplate> DetectInstalledIdes()
        {
            var rules = GetDetectionRules();
            var detectedIdes = new List<IdeTemplate>();

            foreach (var rule in rules)
            {
                var resolvedPaths = rule.SearchPaths.Select(ExpandEnvironmentVars).ToArray();
                var exePath = FindExecutable(rule.ExeName, resolvedPaths);

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
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ProjectHub");
            var configPath = Path.Combine(appDataDir, "ide_detection.json");

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
                        ExeName = item.TryGetProperty("exeName", out var exeProp) ? exeProp.GetString() ?? "" : "",
                        Icon = item.TryGetProperty("icon", out var iconProp) ? iconProp.GetString() : "💻",
                        Priority = item.TryGetProperty("priority", out var priProp) ? priProp.GetInt32() : 99,
                        DefaultArgs = item.TryGetProperty("defaultArgs", out var argsProp) ? argsProp.GetString() : ""
                    };

                    if (item.TryGetProperty("searchPaths", out var pathsProp))
                    {
                        foreach (var p in pathsProp.EnumerateArray())
                            rule.SearchPaths.Add(p.GetString() ?? "");
                    }

                    if (item.TryGetProperty("supportedExtensions", out var extProp))
                    {
                        foreach (var e in extProp.EnumerateArray())
                            rule.SupportedExtensions.Add(e.GetString() ?? "");
                    }

                    if (!string.IsNullOrEmpty(rule.Id) && !string.IsNullOrEmpty(rule.Name) && !string.IsNullOrEmpty(rule.ExeName))
                    {
                        rules.Add(rule);
                    }
                }
                catch { }
            }

            return rules;
        }

        private string? WhereIs(string exeName)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "where.exe",
                        Arguments = $"\"{exeName}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                process.Start();

                var output = process.StandardOutput.ReadToEnd().Trim();
                _ = process.StandardError.ReadToEnd();
                process.WaitForExit(TimeSpan.FromSeconds(3));

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    var firstLine = output.Split('\n')[0].Trim();
                    if (!string.IsNullOrEmpty(firstLine) && File.Exists(firstLine))
                        return firstLine;
                }
            }
            catch { }

            return null;
        }

        private string? FindExecutable(string exeName, string[] searchDirs)
        {
            var result = WhereIs(exeName);
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
                var fullPath = Path.Combine(dir, exeName);
                if (File.Exists(fullPath)) return fullPath;
            }

            return null;
        }

        private string ExpandEnvironmentVars(string path)
        {
            return Environment.ExpandEnvironmentVariables(path);
        }
    }
}
