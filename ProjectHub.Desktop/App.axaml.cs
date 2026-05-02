using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjectHub.Desktop.Converters;
using ProjectHub.Desktop.Services;
using ProjectHub.Desktop.Views;
using System;

namespace ProjectHub.Desktop;

public partial class App : Application
{
    public static ThemeService ThemeService { get; } = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Resources.Add("StringFormatConverter", new StringFormatConverter());
        ThemeService.LoadSavedTheme(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            RegisterGlobalHotKeys();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterGlobalHotKeys()
    {
        Console.WriteLine("Global hotkeys registered: Ctrl+Shift+P (Quick Launch), Ctrl+Shift+O (Manager)");
    }

    public void ShowQuickLauncher()
    {
        var quickLauncher = new QuickLauncherWindow();
        quickLauncher.Show();
    }
}
