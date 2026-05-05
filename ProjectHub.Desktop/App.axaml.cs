using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Core.Database;
using ProjectHub.Core.Services;
using ProjectHub.Desktop.Converters;
using ProjectHub.Desktop.Services;
using ProjectHub.Desktop.ViewModels;
using ProjectHub.Desktop.Views;
using System;
using System.Linq;

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
        services.AddSingleton<IWorkspaceLauncherService, WorkspaceLauncherService>();
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
            MigrateWorkspaceDefaultIdeId(db);
            MigrateWorkspaceIsFavorite(db);
            FileLogger.Info("Database initialized successfully");
        }
        catch (Exception ex)
        {
            FileLogger.Error("Failed to initialize database", ex);
        }
    }

    private static void MigrateWorkspaceDefaultIdeId(AppDbContext db)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            bool hasColumn = false;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(Workspaces)";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var name = reader.GetString(reader.GetOrdinal("name"));
                    if (name == "DefaultIdeId")
                    {
                        hasColumn = true;
                        break;
                    }
                }
            }

            if (!hasColumn)
            {
                using var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE Workspaces ADD COLUMN DefaultIdeId INTEGER";
                alterCmd.ExecuteNonQuery();
                FileLogger.Info("Migration: Added DefaultIdeId column to Workspaces");
            }

            conn.Close();
        }
        catch (Exception ex)
        {
            FileLogger.Error("Migration failed for DefaultIdeId", ex);
        }
    }

    private static void MigrateWorkspaceIsFavorite(AppDbContext db)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            bool hasColumn = false;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(Workspaces)";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var name = reader.GetString(reader.GetOrdinal("name"));
                    if (name == "IsFavorite")
                    {
                        hasColumn = true;
                        break;
                    }
                }
            }

            if (!hasColumn)
            {
                using var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE Workspaces ADD COLUMN IsFavorite INTEGER NOT NULL DEFAULT 0";
                alterCmd.ExecuteNonQuery();
                FileLogger.Info("Migration: Added IsFavorite column to Workspaces");
            }

            conn.Close();
        }
        catch (Exception ex)
        {
            FileLogger.Error("Migration failed for IsFavorite", ex);
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
