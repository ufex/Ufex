using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentIcons.Common;
using System.Linq;
using Ufex.FileType;

namespace Ufex.Desktop;

public partial class FileTypeSelectionWindow : Window
{
	private readonly DetectionMatch[] _matches;
	private FileTypeSelectionItem[] _items;

	public FileTypeSelectionWindow(DetectionResult result, FileTypeManager fileTypeManager)
	{
		InitializeComponent();

		_matches = result.Matches.ToArray();
		_items = new FileTypeSelectionItem[_matches.Length];

		for (int i = 0; i < _matches.Length; i++)
		{
			var match = _matches[i];
			var ft = match.FileType;
			var classes = fileTypeManager.GetFileTypeClassesByFileType(ft.ID);
			bool hasPlugin = classes != null && classes.Length > 0;

			_items[i] = new FileTypeSelectionItem
			{
				Index = i,
				DisplayText = hasPlugin ? $"{ft.Description} (plugin available)" : ft.Description,
				Icon = hasPlugin ? Symbol.PlugConnected : Symbol.Document,
				MatchMethodText = DetectionResultFormatter.FormatMatchMethodLabel(match.Method),
				MatchDetails = DetectionResultFormatter.FormatMatchDetails(match),
			};
		}

		FileTypeListBox.ItemsSource = _items;

		if (_items.Length > 0)
		{
			FileTypeListBox.SelectedIndex = 0;
		}
	}

	private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		OkButton.IsEnabled = FileTypeListBox.SelectedItem != null;
	}

	private void OnOkClick(object? sender, RoutedEventArgs e)
	{
		ConfirmSelection();
	}

	private void OnCancelClick(object? sender, RoutedEventArgs e)
	{
		Close(null);
	}

	private void OnListBoxDoubleTapped(object? sender, RoutedEventArgs e)
	{
		ConfirmSelection();
	}

	private void ConfirmSelection()
	{
		if (FileTypeListBox.SelectedItem is FileTypeSelectionItem item)
		{
			Close(_matches[item.Index]);
		}
	}

	private async void OnShowMatchDetails(object? sender, RoutedEventArgs e)
	{
		if (sender is Button button && button.Tag is int index)
		{
			var item = _items[index];
			var detailsWindow = new MatchDetailsWindow(item.DisplayText, item.MatchDetails);
			await detailsWindow.ShowDialog(this);
		}
	}
}

public class FileTypeSelectionItem
{
	public int Index { get; set; }
	public string DisplayText { get; set; } = "";
	public Symbol Icon { get; set; } = Symbol.Document;
	public string MatchMethodText { get; set; } = "";
	public string MatchDetails { get; set; } = "";
}
