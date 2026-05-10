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
            MigrateTables(db);
            MigrateColumns(db);
            FileLogger.Info("Database initialized successfully");
        }
        catch (Exception ex)
        {
            FileLogger.Error("Failed to initialize database", ex);
        }
    }

    private static bool TableExists(System.Data.Common.DbConnection conn, string tableName)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=$name";
        var p = cmd.CreateParameter();
        p.ParameterName = "$name";
        p.Value = tableName;
        cmd.Parameters.Add(p);
        using var reader = cmd.ExecuteReader();
        return reader.HasRows;
    }

    private static bool ColumnExists(System.Data.Common.DbConnection conn, string tableName, string columnName)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info({tableName})";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.GetString(reader.GetOrdinal("name"));
            if (name == columnName)
                return true;
        }
        return false;
    }

    private static void EnsureColumn(System.Data.Common.DbConnection conn, string table, string column, string definition)
    {
        if (TableExists(conn, table) && !ColumnExists(conn, table, column))
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {definition}";
            cmd.ExecuteNonQuery();
            FileLogger.Info($"Migration: Added {column} to {table}");
        }
    }

    private static void MigrateTables(AppDbContext db)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            if (!TableExists(conn, "Tags"))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE Tags (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Color TEXT, CONSTRAINT IX_Tags_Name UNIQUE (Name))";
                cmd.ExecuteNonQuery();
                FileLogger.Info("Migration: Created Tags table");
            }

            if (!TableExists(conn, "ProjectTags"))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE ProjectTags (ProjectId INTEGER NOT NULL, TagId INTEGER NOT NULL, CONSTRAINT PK_ProjectTags PRIMARY KEY (ProjectId, TagId), CONSTRAINT FK_ProjectTags_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id), CONSTRAINT FK_ProjectTags_Tags FOREIGN KEY (TagId) REFERENCES Tags(Id))";
                cmd.ExecuteNonQuery();
                FileLogger.Info("Migration: Created ProjectTags table");
            }

            if (!TableExists(conn, "WorkspaceProjects"))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE WorkspaceProjects (WorkspaceId INTEGER NOT NULL, ProjectId INTEGER NOT NULL, SortOrder INTEGER NOT NULL DEFAULT 0, CONSTRAINT PK_WorkspaceProjects PRIMARY KEY (WorkspaceId, ProjectId), CONSTRAINT FK_WorkspaceProjects_Workspaces FOREIGN KEY (WorkspaceId) REFERENCES Workspaces(Id), CONSTRAINT FK_WorkspaceProjects_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id))";
                cmd.ExecuteNonQuery();
                FileLogger.Info("Migration: Created WorkspaceProjects table");
            }

            conn.Close();
        }
        catch (Exception ex)
        {
            FileLogger.Error("Migration failed for table creation", ex);
        }
    }

    private static void MigrateColumns(AppDbContext db)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            EnsureColumn(conn, "Projects", "DefaultIdeId", "DefaultIdeId INTEGER");
            EnsureColumn(conn, "Projects", "LastOpenedAt", "LastOpenedAt TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'");
            EnsureColumn(conn, "Projects", "OpenCount", "OpenCount INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "Projects", "IsFavorite", "IsFavorite INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "Projects", "CreatedAt", "CreatedAt TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'");
            EnsureColumn(conn, "Projects", "UpdatedAt", "UpdatedAt TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'");

            EnsureColumn(conn, "Workspaces", "DefaultIdeId", "DefaultIdeId INTEGER");
            EnsureColumn(conn, "Workspaces", "AutoInheritTags", "AutoInheritTags INTEGER NOT NULL DEFAULT 1");
            EnsureColumn(conn, "Workspaces", "CustomTagsJson", "CustomTagsJson TEXT NOT NULL DEFAULT '[]'");
            EnsureColumn(conn, "Workspaces", "IsFavorite", "IsFavorite INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "Workspaces", "CreatedAt", "CreatedAt TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'");
            EnsureColumn(conn, "Workspaces", "UpdatedAt", "UpdatedAt TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'");

            EnsureColumn(conn, "IdeTemplates", "SupportedExtensionsJson", "SupportedExtensionsJson TEXT NOT NULL DEFAULT '[]'");
            EnsureColumn(conn, "IdeTemplates", "Priority", "Priority INTEGER NOT NULL DEFAULT 0");

            conn.Close();
        }
        catch (Exception ex)
        {
            FileLogger.Error("Migration failed for column migration", ex);
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
