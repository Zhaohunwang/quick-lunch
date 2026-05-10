using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Reflection;

namespace ProjectHub.Desktop.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void LoadVersionInfo()
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            var versionString = version != null
                ? $"Version {version.Major}.{version.Minor}.{version.Build}"
                : "Version 1.0.0";

            if (VersionText != null)
                VersionText.Text = versionString;

            if (TechInfoText != null)
                TechInfoText.Text = $".NET {Environment.Version.Major} \u00b7 Avalonia UI 11.1.0";

            if (CopyrightText != null)
                CopyrightText.Text = $"\u00a9 2024-{DateTime.Now.Year} Project Hub";
        }
    }
}
