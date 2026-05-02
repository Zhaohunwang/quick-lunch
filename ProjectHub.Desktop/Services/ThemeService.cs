using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.IO;
using System.Text.Json;

namespace ProjectHub.Desktop.Services;

public enum ThemeMode
{
    Default,
    Light,
    Dark
}

public class ThemeService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProjectHub", "theme.json");

    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Default;

    public bool IsDarkTheme
    {
        get
        {
            if (CurrentTheme == ThemeMode.Dark)
                return true;
            if (CurrentTheme == ThemeMode.Default && Application.Current?.ActualThemeVariant == ThemeVariant.Dark)
                return true;
            return false;
        }
    }

    public event Action<ThemeMode>? ThemeChanged;

    private static readonly (string Key, string LightHex, string DarkHex)[] ThemeTokens =
    [
        ("App.PrimaryBrush", "#0078D4", "#4CC2FF"),
        ("App.PrimaryHoverBrush", "#106EBE", "#70D0FF"),
        ("App.PrimaryPressedBrush", "#005A9E", "#3AB0E8"),
        ("App.PrimaryTextBrush", "#0078D4", "#4CC2FF"),
        ("App.SecondaryTextBrush", "#6C757D", "#9CA3AF"),
        ("App.BackgroundBrush", "#FFFFFF", "#1E1E2E"),
        ("App.SurfaceBrush", "#FFFFFF", "#2B2B3C"),
        ("App.SurfaceSecondaryBrush", "#F8F9FA", "#313145"),
        ("App.HoverSurfaceBrush", "#F0F0F0", "#3A3A4E"),
        ("App.BorderBrush", "#DEE2E6", "#3D3D52"),
        ("App.CardBorderBrush", "#DEE2E6", "#3D3D52"),
        ("App.TitleBarBackgroundBrush", "#E9ECEF", "#252536"),
        ("App.SidebarBackgroundBrush", "#F0F2F5", "#222233"),
        ("App.NavigationActiveBackgroundBrush", "#E3F2FD", "#1A3A5C"),
        ("App.NavigationActiveIndicatorBrush", "#0078D4", "#4CC2FF"),
        ("App.NavigationActiveTextBrush", "#0078D4", "#4CC2FF"),
        ("App.TextBoxBackgroundBrush", "#F8F9FA", "#1E1E2E"),
        ("App.TextBoxBorderBrush", "#DEE2E6", "#3D3D52"),
        ("App.DangerBrush", "#DC3545", "#FF6B7A"),
        ("App.DangerBackgroundBrush", "#FFF5F5", "#3A1A1E"),
        ("App.IdeButtonBorderBrush", "#DEE2E6", "#3D3D52"),
        ("App.IconBrush", "#6C757D", "#9CA3AF"),
        ("App.MenuBorderBrush", "#DEE2E6", "#3D3D52"),
        ("App.MenuHoverBrush", "#F0F0F0", "#3A3A4E"),
    ];

    private static readonly (string Key, string LightHex, string DarkHex)[] ColorTokens =
    [
        ("App.HoverSurfaceColor", "#F0F0F0", "#3A3A4E"),
    ];

    public void ApplyTheme(Application app, ThemeMode mode)
    {
        CurrentTheme = mode;
        app.RequestedThemeVariant = mode switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
        UpdateThemeResources(app, mode);
        SavePreference(mode);
        ThemeChanged?.Invoke(mode);
    }

    private void UpdateThemeResources(Application app, ThemeMode mode)
    {
        bool useDark = mode == ThemeMode.Dark ||
                       (mode == ThemeMode.Default && app.ActualThemeVariant == ThemeVariant.Dark);

        foreach (var (key, lightHex, darkHex) in ThemeTokens)
        {
            var hex = useDark ? darkHex : lightHex;
            if (Color.TryParse(hex, out var color))
            {
                app.Resources[key] = new SolidColorBrush(color);
            }
        }

        foreach (var (key, lightHex, darkHex) in ColorTokens)
        {
            var hex = useDark ? darkHex : lightHex;
            if (Color.TryParse(hex, out var color))
            {
                app.Resources[key] = color;
            }
        }
    }

    public void LoadSavedTheme(Application app)
    {
        var saved = LoadPreference();
        ApplyTheme(app, saved);
    }

    public void SwitchToLightTheme()
    {
        if (Application.Current is { } app)
            ApplyTheme(app, ThemeMode.Light);
    }

    public void SwitchToDarkTheme()
    {
        if (Application.Current is { } app)
            ApplyTheme(app, ThemeMode.Dark);
    }

    public void SwitchToDefaultTheme()
    {
        if (Application.Current is { } app)
            ApplyTheme(app, ThemeMode.Default);
    }

    private void SavePreference(ThemeMode mode)
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            var json = JsonSerializer.Serialize(new { Theme = mode.ToString() });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
        }
    }

    private ThemeMode LoadPreference()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return ThemeMode.Default;

            var json = File.ReadAllText(SettingsPath);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Theme", out var el))
            {
                var value = el.GetString();
                if (Enum.TryParse<ThemeMode>(value, out var mode))
                    return mode;
            }
        }
        catch
        {
        }
        return ThemeMode.Default;
    }
}
