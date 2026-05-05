using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ProjectHub.Desktop.Views
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public ConfirmDialog(string title, string message, string confirmText = "删除") : this()
        {
            Title = title;
            DataContext = new ConfirmDialogViewModel(title, message, confirmText);
        }

        private void OnCancelClicked(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void OnConfirmClicked(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }
    }

    public class ConfirmDialogViewModel
    {
        public string Title { get; }
        public string Message { get; }
        public string ConfirmText { get; }

        public ConfirmDialogViewModel(string title, string message, string confirmText)
        {
            Title = title;
            Message = message;
            ConfirmText = confirmText;
        }
    }
}
