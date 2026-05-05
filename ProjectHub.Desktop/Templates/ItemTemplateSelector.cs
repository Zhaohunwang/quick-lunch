using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;
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

            var leftPanel = new StackPanel();
            Grid.SetColumn(leftPanel, 0);

            var namePanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 8 
            };
            namePanel.Children.Add(new MaterialIcon 
            { 
                Kind = MaterialIconKind.BriefcaseOutline, 
                Width = 16, 
                Height = 16, 
                VerticalAlignment = VerticalAlignment.Center 
            });
            namePanel.Children.Add(new TextBlock 
            { 
                Text = workspace.Name, 
                FontWeight = FontWeight.SemiBold, 
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center 
            });
            if (workspace.IsFavorite)
            {
                namePanel.Children.Add(new MaterialIcon 
                { 
                    Kind = MaterialIconKind.Star, 
                    Width = 14, 
                    Height = 14, 
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.Parse("#FFC107"))
                });
            }
            leftPanel.Children.Add(namePanel);

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

            var rightPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(rightPanel, 1);

            var openButton = new Button 
            { 
                Classes = { "PlayButton" }
            };
            openButton.Content = new MaterialIcon { Kind = MaterialIconKind.Play, Width = 12, Height = 12 };
            ToolTip.SetTip(openButton, "打开工作区");
            openButton.Click += async (s, e) => 
            {
                var vmCtx = GetViewModel();
                if (vmCtx != null)
                {
                    await vmCtx.LaunchWorkspaceCommand.ExecuteAsync(workspace);
                }
            };
            rightPanel.Children.Add(openButton);

            var editButton = new Button 
            { 
                Classes = { "ToolButton" }
            };
            editButton.Content = new MaterialIcon { Kind = MaterialIconKind.Pencil, Width = 14, Height = 14 };
            ToolTip.SetTip(editButton, "编辑工作区");
            editButton.Click += async (s, e) => 
            {
                var vmCtx = GetViewModel();
                if (vmCtx != null)
                {
                    await vmCtx.EditWorkspaceCommand.ExecuteAsync(workspace);
                }
            };
            rightPanel.Children.Add(editButton);

            var deleteButton = new Button 
            { 
                Classes = { "ToolButton" }
            };
            deleteButton.Content = new MaterialIcon { Kind = MaterialIconKind.DeleteOutline, Width = 14, Height = 14 };
            ToolTip.SetTip(deleteButton, "删除工作区");
            deleteButton.Click += (s, e) => 
            {
                var vmCtx = GetViewModel();
                if (vmCtx != null)
                {
                    vmCtx.DeleteWorkspaceCommand.Execute(workspace);
                }
            };
            rightPanel.Children.Add(deleteButton);

            var moreButton = new Button 
            { 
                Content = "⋮",
                Classes = { "ToolButton" },
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(8, 4)
            };
            ToolTip.SetTip(moreButton, "更多操作");

            var contextMenu = new ContextMenu();
            var favMenuItem = new MenuItem
            {
                Header = workspace.IsFavorite ? "取消收藏" : "收藏"
            };
            favMenuItem.Click += async (s, e) =>
            {
                var vmCtx = GetViewModel();
                if (vmCtx != null)
                {
                    await vmCtx.ToggleFavoriteWorkspaceCommand.ExecuteAsync(workspace);
                }
            };
            contextMenu.Items.Add(favMenuItem);
            contextMenu.Items.Add(new Separator());
            var editCtxMenuItem = new MenuItem { Header = "编辑" };
            editCtxMenuItem.Click += async (s, e) =>
            {
                var vmCtx = GetViewModel();
                if (vmCtx != null)
                {
                    await vmCtx.EditWorkspaceCommand.ExecuteAsync(workspace);
                }
            };
            contextMenu.Items.Add(editCtxMenuItem);
            var deleteCtxMenuItem = new MenuItem { Header = "删除" };
            deleteCtxMenuItem.Click += async (s, e) =>
            {
                var vmCtx = GetViewModel();
                if (vmCtx != null)
                {
                    await vmCtx.DeleteWorkspaceCommand.ExecuteAsync(workspace);
                }
            };
            contextMenu.Items.Add(deleteCtxMenuItem);
            moreButton.ContextMenu = contextMenu;
            rightPanel.Children.Add(moreButton);

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

            var leftPanel = new StackPanel();
            Grid.SetColumn(leftPanel, 0);

            var namePanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 8 
            };
            namePanel.Children.Add(new MaterialIcon 
            { 
                Kind = MaterialIconKind.LightningBolt, 
                Width = 16, 
                Height = 16, 
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

            var rightPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(rightPanel, 1);

            var vm = GetViewModel();
            var defaultIde = vm?.GetDefaultIdeTemplate(project);

            var defaultIdeButton = new Button 
            { 
                Content = defaultIde != null ? $"用 {defaultIde.Name} 打开" : "打开",
                Classes = { "PlayButton" }
            };
            ToolTip.SetTip(defaultIdeButton, defaultIde != null ? $"用 {defaultIde.Name} 打开" : "用默认IDE打开");
            defaultIdeButton.Click += async (s, e) => 
            {
                if (vm != null)
                {
                    if (defaultIde != null)
                    {
                        await vm.LaunchWithIdeCommand.ExecuteAsync((project, defaultIde));
                    }
                    else
                    {
                        await vm.LaunchProjectCommand.ExecuteAsync(project);
                    }
                }
            };
            rightPanel.Children.Add(defaultIdeButton);

            var moreButton = new Button 
            { 
                Content = "⋮",
                Classes = { "ToolButton" },
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(8, 4)
            };
            ToolTip.SetTip(moreButton, "更多操作");

            var contextMenu = new ContextMenu();
            var ideMenuItem = new MenuItem 
            { 
                Header = "使用 IDE 打开"
            };

            if (vm != null && vm.AvailableIdes.Any())
            {
                foreach (var ide in vm.AvailableIdes)
                {
                    var ideSubItem = new MenuItem 
                    { 
                        Header = $"{(defaultIde?.Id == ide.Id ? "✓ " : "")}{ide.Name}"
                    };
                    var capturedIde = ide;
                    ideSubItem.Click += async (s, e) => 
                    {
                        if (vm != null)
                        {
                            await vm.LaunchWithIdeCommand.ExecuteAsync((project, capturedIde));
                        }
                    };
                    ideMenuItem.Items.Add(ideSubItem);
                }
            }
            else
            {
                var emptyItem = new MenuItem { Header = "(暂无IDE，请先配置)", IsEnabled = false };
                ideMenuItem.Items.Add(emptyItem);
            }
            contextMenu.Items.Add(ideMenuItem);

            contextMenu.Items.Add(new Separator());

            var editMenuItem = new MenuItem { Header = "编辑" };
            editMenuItem.Click += async (s, e) => 
            {
                if (vm != null)
                {
                    await vm.EditProjectCommand.ExecuteAsync(project);
                }
            };
            contextMenu.Items.Add(editMenuItem);

            var deleteMenuItem = new MenuItem { Header = "删除" };
            deleteMenuItem.Click += async (s, e) => 
            {
                if (vm != null)
                {
                    await vm.DeleteProjectCommand.ExecuteAsync(project);
                }
            };
            contextMenu.Items.Add(deleteMenuItem);

            moreButton.ContextMenu = contextMenu;
            rightPanel.Children.Add(moreButton);

            grid.Children.Add(leftPanel);
            grid.Children.Add(rightPanel);
            border.Child = grid;

            return border;
        }

        private MainWindowViewModel? GetViewModel()
        {
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow as MainWindow
                : null;
            return mainWindow?.DataContext as MainWindowViewModel;
        }
    }
}