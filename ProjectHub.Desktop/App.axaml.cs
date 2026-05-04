using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Core.Database;
using ProjectHub.Core.Services;
using ProjectHub.Desktop.Converters;
using ProjectHub.Desktop.Services;
using ProjectHub.Desktop.ViewModels;
using ProjectHub.Desktop.Views;
using System;

namespace ProjectHub.Desktop;

public partial class App : Application
{
    public static ThemeService ThemeService { get; } = new();
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        GlobalExceptionHandler.Initialize();
        FileLogger.Info("DI container and global exception handler initialized");

        InitializeDatabase();

        Resources.Add("StringFormatConverter", new StringFormatConverter());
        ThemeService.LoadSavedTheme(this);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<Func<AppDbContext>>(() => new AppDbContext());

        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<ITagService, TagService>();
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<ISearchService, SearchService>();
        services.AddSingleton<IIdeLauncherService, IdeLauncherService>();
        services.AddSingleton<IdeDetectionService>();

        services.AddTransient<MainWindow>();
        services.AddTransient<QuickLauncherWindow>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<QuickLauncherViewModel>();
        services.AddTransient<AddProjectDialogViewModel>();
        services.AddTransient<AddTagDialogViewModel>();
        services.AddTransient<AddWorkspaceDialogViewModel>();
        services.AddTransient<IdeSettingsDialogViewModel>();
    }

    private static void InitializeDatabase()
    {
        try
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            FileLogger.Info("Database initialized successfully");
        }
        catch (Exception ex)
        {
            FileLogger.Error("Failed to initialize database", ex);
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        GlobalExceptionHandler.HookAvaloniaDispatcher();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
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
        var quickLauncher = Services.GetRequiredService<QuickLauncherWindow>();
        quickLauncher.Show();
    }
}
