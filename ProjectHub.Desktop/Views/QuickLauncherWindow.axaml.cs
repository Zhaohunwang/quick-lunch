using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Desktop.ViewModels;
using System;

namespace ProjectHub.Desktop.Views
{
    public partial class QuickLauncherWindow : Window
    {
        public QuickLauncherWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<QuickLauncherViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                var viewModel = DataContext as QuickLauncherViewModel;
                viewModel?.LaunchProjectCommand.Execute(null);
                Close();
            }
        }
    }
}