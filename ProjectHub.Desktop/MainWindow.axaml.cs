using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ProjectHub.Desktop.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ProjectHub.Desktop;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private bool _isSubscribed = false;

    public MainWindow()
    {
        InitializeComponent();
        
        // 订阅加载完成事件，确保 UI 已经初始化
        Loaded += OnLoaded;
        
        // 订阅 DataContext 变化，以便在 ViewModel 设置后监听属性变化
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
        var allProjectsButton = this.FindControl<Button>("AllProjectsButton");
        var favoriteProjectsButton = this.FindControl<Button>("FavoriteProjectsButton");
        var recentProjectsButton = this.FindControl<Button>("RecentProjectsButton");

        // 更新按钮样式
        UpdateButtonStyle(allProjectsButton, viewModel.IsAllSelected);
        UpdateButtonStyle(favoriteProjectsButton, viewModel.IsFavoriteSelected);
        UpdateButtonStyle(recentProjectsButton, viewModel.IsRecentSelected);
    }

    private void UpdateButtonStyle(Button? button, bool isSelected)
    {
        if (button == null) return;

        button.Classes.Clear();
        button.Classes.Add("NavigationButton");
        
        if (isSelected)
        {
            button.Classes.Add("Active");
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
}
