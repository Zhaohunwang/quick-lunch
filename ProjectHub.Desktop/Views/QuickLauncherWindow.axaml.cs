using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace ProjectHub.Desktop.Views
{
    public partial class QuickLauncherWindow : Window
    {
        public QuickLauncherWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            // 失焦时关闭窗口
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            // 处理键盘事件
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                // 打开选中的项目
                var viewModel = DataContext as ViewModels.QuickLauncherViewModel;
                viewModel?.LaunchProjectCommand.Execute(null);
                Close();
            }
        }
    }
}