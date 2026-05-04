using Avalonia;
using ProjectHub.Desktop.Services;
using System;

namespace ProjectHub.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        GlobalExceptionHandler.Initialize();
        FileLogger.Info("Application starting");

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            FileLogger.Error("Fatal startup exception", ex);
            throw;
        }
        finally
        {
            FileLogger.Info("Application shutting down");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
