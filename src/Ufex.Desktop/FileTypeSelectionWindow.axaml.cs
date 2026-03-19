using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentIcons.Common;
using Ufex.FileType;

namespace Ufex.Desktop;

public partial class FileTypeSelectionWindow : Window
{
	private readonly FileTypeRecord[] _fileTypes;

	public FileTypeSelectionWindow(FileTypeRecord[] fileTypes, FileTypeManager fileTypeManager)
	{
		InitializeComponent();

		_fileTypes = fileTypes;

		var items = new FileTypeSelectionItem[fileTypes.Length];
		for (int i = 0; i < fileTypes.Length; i++)
		{
			var ft = fileTypes[i];
			var classes = fileTypeManager.GetFileTypeClassesByFileType(ft.ID);
			bool hasPlugin = classes != null && classes.Length > 0;
			items[i] = new FileTypeSelectionItem
			{
				Index = i,
				DisplayText = hasPlugin ? $"{ft.Description} (plugin available)" : ft.Description,
				Icon = hasPlugin ? Symbol.PlugConnected : Symbol.Document,
			};
		}

		FileTypeListBox.ItemsSource = items;

		if (items.Length > 0)
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
			Close(_fileTypes[item.Index]);
		}
	}
}

public class FileTypeSelectionItem
{
	public int Index { get; set; }
	public string DisplayText { get; set; } = "";
	public Symbol Icon { get; set; } = Symbol.Document;
}
