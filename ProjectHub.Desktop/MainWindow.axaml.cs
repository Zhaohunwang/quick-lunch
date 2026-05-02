using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ProjectHub.Core.Models;
using ProjectHub.Desktop.ViewModels;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHub.Desktop;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private bool _isSubscribed = false;

    public MainWindow()
    {
        InitializeComponent();
        
        Loaded += OnLoaded;
        
        DataContextChanged += OnDataContextChanged;
    }
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // 窗口加载完成后延迟更新按钮样式，确保 UI 完全初始化
        Dispatcher.UIThread.Post(async () =>
        {
            // 等待 UI 完全渲染
            await Task.Delay(100);
            if (DataContext is MainWindowViewModel viewModel)
            {
                UpdateNavigationButtonStyles(viewModel);
            }
        }, DispatcherPriority.Background);
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            
            // 只订阅一次
            if (!_isSubscribed)
            {
                viewModel.PropertyChanged += OnPropertyChanged;
                _isSubscribed = true;
            }
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewModel != null && 
            (e.PropertyName == nameof(MainWindowViewModel.IsAllSelected) ||
             e.PropertyName == nameof(MainWindowViewModel.IsFavoriteSelected) ||
             e.PropertyName == nameof(MainWindowViewModel.IsRecentSelected)))
        {
            UpdateNavigationButtonStyles(_viewModel);
        }
    }

    private void UpdateNavigationButtonStyles(MainWindowViewModel viewModel)
    {
        // 获取按钮引用
        var allProjectsButton = this.FindControl<ToggleButton>("AllProjectsButton");
        var favoriteProjectsButton = this.FindControl<ToggleButton>("FavoriteProjectsButton");
        var recentProjectsButton = this.FindControl<ToggleButton>("RecentProjectsButton");

        // 更新按钮样式
        UpdateButtonStyle(allProjectsButton, viewModel.IsAllSelected);
        UpdateButtonStyle(favoriteProjectsButton, viewModel.IsFavoriteSelected);
        UpdateButtonStyle(recentProjectsButton, viewModel.IsRecentSelected);
    }

    private void UpdateButtonStyle(ToggleButton? button, bool isSelected)
    {
        if (button == null) return;

        button.Classes.Clear();
        button.Classes.Add("NavigationButton");
        
        if (isSelected)
        {
            button.Classes.Add("active");
        }
    }

    private async void AddProjectButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.AddProjectCommand.ExecuteAsync(null);
        }
    }

    private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SearchCommand.ExecuteAsync(null);
        }
    }

    private void OnMoreOptionsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.DataContext = DataContext;

            foreach (var item in button.ContextMenu.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.CommandParameter = button.Tag;
                }
            }

            InjectIdeSubMenu(button.ContextMenu, button.Tag as Project);
            button.ContextMenu.Open(button);
        }
    }

    private void InjectIdeSubMenu(ContextMenu contextMenu, Project? project)
    {
        if (project == null || DataContext is not MainWindowViewModel vm) return;

        var defaultIde = vm.GetDefaultIdeTemplate(project);

        var ideMenuItem = new MenuItem { Header = "💻 使用IDE打开" };

        if (vm.AvailableIdes.Any())
        {
            foreach (var ide in vm.AvailableIdes)
            {
                var capturedIde = ide;
                var isDefault = defaultIde?.Id == ide.Id;
                var ideSubItem = new MenuItem 
                { 
                    Header = $"{ide.Icon ?? "💻"} {(isDefault ? "✓ " : "")}{ide.Name}"
                };
                ideSubItem.Click += async (s, args) =>
                {
                    await vm.LaunchWithIdeCommand.ExecuteAsync((project, capturedIde));
                };
                ideMenuItem.Items.Add(ideSubItem);
            }
        }
        else
        {
            ideMenuItem.Items.Add(new MenuItem { Header = "(暂无IDE，请先配置)", IsEnabled = false });
        }

        contextMenu.Items.Insert(0, ideMenuItem);
        contextMenu.Items.Insert(1, new Separator());
    }
}
