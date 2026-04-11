using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProjectHub.Desktop.ViewModels;
using ProjectHub.Desktop.Services;
using System;

namespace TestTagDisplay
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = BuildAvaloniaApp();
            app.StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont();
        }
    }

    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }

    public class MainWindow : Window
    {
        public MainWindow()
        {
            Title = "Tag Display Test";
            Width = 400;
            Height = 300;

            var viewModel = new TestViewModel();
            DataContext = viewModel;

            var stackPanel = new StackPanel { Margin = new Avalonia.Thickness(20) };

            var textBlock = new TextBlock { Text = "Tags:", FontSize = 16, FontWeight = Avalonia.Media.FontWeight.SemiBold };
            stackPanel.Children.Add(textBlock);

            var itemsControl = new ItemsControl { ItemsSource = viewModel.DisplayTags };
            itemsControl.ItemTemplate = new Avalonia.Controls.Templates.DataTemplate(() =>
            {
                var button = new Button();
                button.Bind(Button.ContentProperty, new Avalonia.Data.Binding());
                button.Classes.Add("NavigationButton");
                return button;
            });
            stackPanel.Children.Add(itemsControl);

            Content = stackPanel;
        }
    }

    public class TestViewModel
    {
        public TestViewModel()
        {
            var tagService = new TagService();
            LoadTags(tagService);
        }

        public System.Collections.ObjectModel.ObservableCollection<string> Tags { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<string> DisplayTags { get; } = new();

        private async void LoadTags(ProjectHub.Desktop.Services.ITagService tagService)
        {
            try
            {
                var tags = await tagService.GetAllTagsAsync();
                Console.WriteLine($"获取到标签数量: {tags.Count}");
                Tags.Clear();
                foreach (var tag in tags)
                {
                    Tags.Add($"#{tag.Name}");
                    Console.WriteLine($"添加标签: #{tag.Name}");
                }
                UpdateDisplayTags();
                Console.WriteLine($"DisplayTags 数量: {DisplayTags.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载标签失败: {ex.Message}");
            }
        }

        private void UpdateDisplayTags()
        {
            DisplayTags.Clear();
            if (Tags.Count <= 6)
            {
                foreach (var tag in Tags)
                {
                    DisplayTags.Add(tag);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    DisplayTags.Add(Tags[i]);
                }
                DisplayTags.Add("...");
            }
        }
    }
}