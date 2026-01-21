using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Threading.Tasks;

namespace Ufex.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0)
        {
            var file = files[0];
            // TODO: Handle the opened file
            Title = $"ufex - {file.Name}";
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Close the currently opened file
        Title = "ufex - Universal File Explorer";
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
