using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.Services;

public static class GlobalExceptionHandler
{
    public static void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    public static void HookAvaloniaDispatcher()
    {
        Dispatcher.UIThread.UnhandledException += OnApplicationUnhandledException;
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            FileLogger.HandleUnhandledException("AppDomain", ex);
            ShowErrorSafe("应用程序异常", ex.Message, ex.ToString());
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        FileLogger.HandleUnhandledException("TaskScheduler", e.Exception);
        e.SetObserved();
        ShowErrorSafe("异步任务异常", e.Exception.Message, e.Exception.ToString());
    }

    private static void OnApplicationUnhandledException(object? sender, Avalonia.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        FileLogger.HandleUnhandledException("Avalonia Dispatcher", e.Exception);
        e.Handled = true;
        ShowErrorSafe("界面异常", e.Exception.Message, e.Exception.ToString());
    }

    private static void ShowErrorSafe(string title, string message, string? detail)
    {
        try
        {
            Dispatcher.UIThread.Post(async () =>
            {
                try
                {
                    await ErrorMessageBox.ShowAsync(title, message, detail);
                }
                catch
                {
                    FileLogger.Error("Failed to show error dialog");
                }
            });
        }
        catch
        {
            FileLogger.Error("Failed to dispatch error dialog to UI thread");
        }
    }
}
