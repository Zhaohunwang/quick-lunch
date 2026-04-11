using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjectHub.Desktop.Views;
using System;

namespace ProjectHub.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            // 注册全局快捷键
            RegisterGlobalHotKeys();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterGlobalHotKeys()
    {
        // 这里将注册全局快捷键
        // 暂时使用模拟实现，实际项目中需要使用全局快捷键库
        Console.WriteLine("Global hotkeys registered: Ctrl+Shift+P (Quick Launch), Ctrl+Shift+O (Manager)");
    }

    public void ShowQuickLauncher()
    {
        var quickLauncher = new QuickLauncherWindow();
        quickLauncher.Show();
    }
}