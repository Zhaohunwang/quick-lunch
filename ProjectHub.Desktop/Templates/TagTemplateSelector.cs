using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.VisualTree;
using ProjectHub.Desktop.ViewModels;
using System;

namespace ProjectHub.Desktop.Templates
{
    public class TagTemplateSelector : IDataTemplate
    {
        public bool Match(object? data) => data is string;

        public Control? Build(object? param)
        {
            if (param is string tag)
            {
                var button = new Button
                {
                    Content = tag,
                    Classes = { "NavigationButton" }
                };

                button.Click += (sender, e) =>
                {
                    var btn = sender as Button;
                    var itemsControl = btn?.FindAncestorOfType<ItemsControl>();
                    var mainWindowViewModel = itemsControl?.DataContext as MainWindowViewModel;

                    if (mainWindowViewModel != null)
                    {
                        if (tag == "...")
                        {
                            mainWindowViewModel.ShowAllTagsCommand.Execute(null);
                        }
                        else
                        {
                            mainWindowViewModel.FilterByTagCommand.Execute(tag);
                        }
                    }
                };

                return button;
            }
            return null;
        }
    }
}
