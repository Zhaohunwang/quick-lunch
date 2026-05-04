using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace ProjectHub.Desktop.Services;

public static class ErrorMessageBox
{
    public static Task ShowAsync(string title, string message, string? detail = null)
    {
        var tcs = new TaskCompletionSource();

        var dialog = new Window
        {
            Title = title,
            Width = 520,
            Height = 420,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new SolidColorBrush(Color.Parse("#1E1E2E")),
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Microsoft YaHei UI, Segoe UI, sans-serif")
        };

        var mainPanel = new StackPanel { Margin = new Thickness(24), Spacing = 12 };

        mainPanel.Children.Add(new TextBlock
        {
            Text = "\u26A0",
            FontSize = 36,
            HorizontalAlignment = HorizontalAlignment.Left,
            Foreground = new SolidColorBrush(Color.Parse("#FF6B6B"))
        });

        mainPanel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#FF6B6B")),
            TextWrapping = TextWrapping.Wrap
        });

        mainPanel.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#CCCCCC")),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0)
        });

        if (!string.IsNullOrEmpty(detail))
        {
            mainPanel.Children.Add(new TextBox
            {
                Text = detail,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                Background = new SolidColorBrush(Color.Parse("#2B2B3C")),
                Foreground = new SolidColorBrush(Color.Parse("#9CA3AF")),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.Parse("#3D3D52")),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 8, 0, 0),
                MaxHeight = 120
            });
        }

        mainPanel.Children.Add(new TextBlock
        {
            Text = $"日志: {FileLogger.LatestLogPath}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#6B7280")),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0)
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 12,
            Margin = new Thickness(0, 12, 0, 0)
        };

        var copyButton = new Button
        {
            Content = "复制",
            Height = 36,
            Padding = new Thickness(16, 0),
            Background = new SolidColorBrush(Color.Parse("#3D3D52")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#555577")),
            CornerRadius = new CornerRadius(4)
        };

        var okButton = new Button
        {
            Content = "确定",
            Height = 36,
            Padding = new Thickness(24, 0),
            Background = new SolidColorBrush(Color.Parse("#4CC2FF")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(4),
            FontWeight = FontWeight.SemiBold
        };

        copyButton.Click += async (s, e) =>
        {
            var clipboard = TopLevel.GetTopLevel(dialog)?.Clipboard;
            if (clipboard != null)
            {
                var data = new DataObject();
                data.Set(DataFormats.Text, $"{title}\n{message}\n\n{detail}");
                await clipboard.SetDataObjectAsync(data);
                copyButton.Content = "已复制!";
                await Task.Delay(1500);
                copyButton.Content = "复制";
            }
        };

        okButton.Click += (s, e) =>
        {
            tcs.TrySetResult();
            dialog.Close();
        };

        buttonPanel.Children.Add(copyButton);
        buttonPanel.Children.Add(okButton);
        mainPanel.Children.Add(buttonPanel);

        dialog.Content = mainPanel;
        dialog.Closed += (s, e) => tcs.TrySetResult();

        dialog.Show();
        return tcs.Task;
    }
}
