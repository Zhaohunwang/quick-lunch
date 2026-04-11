using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using ProjectHub.Core.Models;
using ProjectHub.Desktop.ViewModels;
using System;
using System.Linq;

namespace ProjectHub.Desktop.Templates
{
    public class ItemTemplateSelector : IDataTemplate
    {
        public bool Match(object? data) => data is Project || data is Workspace;

        public Control? Build(object? param)
        {
            if (param is Workspace workspace)
            {
                return BuildWorkspaceItem(workspace);
            }
            else if (param is Project project)
            {
                return BuildProjectItem(project);
            }
            return null;
        }

        private Control BuildWorkspaceItem(Workspace workspace)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#E3F2FD")),
                BorderBrush = new SolidColorBrush(Color.Parse("#BBDEFB")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(16, 12),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = Cursor.Parse("Hand")
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // 左侧信息
            var leftPanel = new StackPanel();
            Grid.SetColumn(leftPanel, 0);

            // 名称行
            var namePanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 8 
            };
            namePanel.Children.Add(new TextBlock 
            { 
                Text = "📁", 
                FontSize = 16, 
                VerticalAlignment = VerticalAlignment.Center 
            });
            namePanel.Children.Add(new TextBlock 
            { 
                Text = workspace.Name, 
                FontWeight = FontWeight.SemiBold, 
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center 
            });
            leftPanel.Children.Add(namePanel);

            // 描述
            if (!string.IsNullOrEmpty(workspace.Description))
            {
                leftPanel.Children.Add(new TextBlock 
                { 
                    Text = workspace.Description,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.Parse("#6C757D")),
                    Margin = new Thickness(28, 4, 0, 0)
                });
            }

            // 标签
            if (workspace.AllTags.Any())
            {
                var tagsPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    Spacing = 6,
                    Margin = new Thickness(28, 4, 0, 0)
                };
                foreach (var tag in workspace.AllTags)
                {
                    tagsPanel.Children.Add(new Border 
                    { 
                        Background = new SolidColorBrush(Color.Parse("#E3F2FD")),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(6, 2),
                        Child = new TextBlock 
                        { 
                            Text = $"#{tag}", 
                            FontSize = 11, 
                            Foreground = new SolidColorBrush(Color.Parse("#1976D2")) 
                        }
                    });
                }
                leftPanel.Children.Add(tagsPanel);
            }

            // 右侧按钮
            var rightPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(rightPanel, 1);

            // 打开按钮
            var openButton = new Button 
            { 
                Content = "▶",
                Classes = { "PlayButton" }
            };
            ToolTip.SetTip(openButton, "打开工作区");
            openButton.Click += (s, e) => 
            {
                // TODO: 实现打开工作区功能
            };
            rightPanel.Children.Add(openButton);

            // 编辑按钮
            var editButton = new Button 
            { 
                Content = "✏️",
                Classes = { "ToolButton" }
            };
            ToolTip.SetTip(editButton, "编辑工作区");
            editButton.Click += (s, e) => 
            {
                // TODO: 实现编辑工作区功能
            };
            rightPanel.Children.Add(editButton);

            // 删除按钮
            var deleteButton = new Button 
            { 
                Content = "🗑️",
                Classes = { "ToolButton" }
            };
            ToolTip.SetTip(deleteButton, "删除工作区");
            deleteButton.Click += (s, e) => 
            {
                // TODO: 实现删除工作区功能
            };
            rightPanel.Children.Add(deleteButton);

            grid.Children.Add(leftPanel);
            grid.Children.Add(rightPanel);
            border.Child = grid;

            return border;
        }

        private Control BuildProjectItem(Project project)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
                BorderBrush = new SolidColorBrush(Color.Parse("#E9ECEF")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(16, 12),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = Cursor.Parse("Hand")
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // 左侧信息
            var leftPanel = new StackPanel();
            Grid.SetColumn(leftPanel, 0);

            // 名称行
            var namePanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 8 
            };
            namePanel.Children.Add(new TextBlock 
            { 
                Text = "🚀", 
                FontSize = 16, 
                VerticalAlignment = VerticalAlignment.Center 
            });
            namePanel.Children.Add(new TextBlock 
            { 
                Text = project.Name, 
                FontWeight = FontWeight.SemiBold, 
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center 
            });
            leftPanel.Children.Add(namePanel);

            // 别名
            if (!string.IsNullOrEmpty(project.Alias))
            {
                leftPanel.Children.Add(new TextBlock 
                { 
                    Text = $"别名: {project.Alias}",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.Parse("#6C757D")),
                    Margin = new Thickness(28, 4, 0, 0)
                });
            }

            // 标签
            if (project.Tags.Any())
            {
                var tagsPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    Spacing = 6,
                    Margin = new Thickness(28, 4, 0, 0)
                };
                foreach (var tag in project.Tags)
                {
                    tagsPanel.Children.Add(new Border 
                    { 
                        Background = new SolidColorBrush(Color.Parse("#E3F2FD")),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(6, 2),
                        Child = new TextBlock 
                        { 
                            Text = $"#{tag}", 
                            FontSize = 11, 
                            Foreground = new SolidColorBrush(Color.Parse("#1976D2")) 
                        }
                    });
                }
                leftPanel.Children.Add(tagsPanel);
            }

            // 右侧按钮
            var rightPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(rightPanel, 1);

            // IDE菜单
            var ideMenu = new Menu();
            var ideMenuItem = new MenuItem 
            { 
                Header = "🚀 用IDE打开",
                Classes = { "IdeMenuItem" }
            };
            ideMenuItem.Icon = new TextBlock { Text = "🚀" };
            ideMenu.Items.Add(ideMenuItem);
            rightPanel.Children.Add(ideMenu);

            // 打开按钮
            var openButton = new Button 
            { 
                Content = "▶",
                Classes = { "PlayButton" }
            };
            ToolTip.SetTip(openButton, "打开项目");
            openButton.Click += async (s, e) => 
            {
                var mainWindow = GetMainWindow();
                if (mainWindow?.DataContext is MainWindowViewModel vm)
                {
                    await vm.LaunchProjectCommand.ExecuteAsync(project);
                }
            };
            rightPanel.Children.Add(openButton);

            // 编辑按钮
            var editButton = new Button 
            { 
                Content = "✏️",
                Classes = { "ToolButton" }
            };
            ToolTip.SetTip(editButton, "编辑项目");
            editButton.Click += async (s, e) => 
            {
                var mainWindow = GetMainWindow();
                if (mainWindow?.DataContext is MainWindowViewModel vm)
                {
                    await vm.EditProjectCommand.ExecuteAsync(project);
                }
            };
            rightPanel.Children.Add(editButton);

            // 删除按钮
            var deleteButton = new Button 
            { 
                Content = "🗑️",
                Classes = { "ToolButton" }
            };
            ToolTip.SetTip(deleteButton, "删除项目");
            deleteButton.Click += async (s, e) => 
            {
                var mainWindow = GetMainWindow();
                if (mainWindow?.DataContext is MainWindowViewModel vm)
                {
                    await vm.DeleteProjectCommand.ExecuteAsync(project);
                }
            };
            rightPanel.Children.Add(deleteButton);

            grid.Children.Add(leftPanel);
            grid.Children.Add(rightPanel);
            border.Child = grid;

            return border;
        }

        private MainWindow? GetMainWindow()
        {
            return App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;
        }
    }
}
