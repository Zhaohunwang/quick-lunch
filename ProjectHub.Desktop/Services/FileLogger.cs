using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ProjectHub.Desktop.Services;

public static class FileLogger
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProjectHub", "logs");

    private static readonly object Lock = new();
    private static int _errorCount;

    public static string LatestLogPath { get; private set; } = string.Empty;

    static FileLogger()
    {
        Directory.CreateDirectory(LogDirectory);
        LatestLogPath = Path.Combine(LogDirectory, $"{DateTime.Now:yyyy-MM-dd_HHmmss}.log");
    }

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    public static void HandleUnhandledException(string source, Exception exception)
    {
        var count = Interlocked.Increment(ref _errorCount);
        var message = $"[{source}] Unhandled exception (#{count})";
        Write("FATAL", message, exception);
    }

    private static void Write(string level, string message, Exception? ex = null)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var threadId = Environment.CurrentManagedThreadId;
        var entry = $"[{timestamp}] [{level}] [T{threadId}] {message}";

        if (ex != null)
        {
            entry += $"\n  Exception: {ex.GetType().Name}: {ex.Message}";
            if (ex.InnerException != null)
                entry += $"\n  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
            entry += $"\n  StackTrace:\n{ex.StackTrace}";
        }

        Debug.WriteLine(entry);
        Debug.Flush();

        lock (Lock)
        {
            try
            {
                File.AppendAllText(LatestLogPath, entry + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}
