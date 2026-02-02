using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using Ufex.FileType;

namespace Ufex.Desktop;

/// <summary>
/// View model for file type items displayed in the list box.
/// </summary>
public class FileTypeListItem
{
	public string Id { get; set; } = string.Empty;
	public FileTypeRecord Record { get; set; } = null!;

	public override string ToString() => Id;
}

public partial class FileTypeManagerWindow : Window
{
	private readonly FileTypeManager _fileTypeManager;
	private List<FileTypeListItem> _allFileTypes = new();

	public FileTypeManagerWindow(FileTypeManager fileTypeManager)
	{
		InitializeComponent();
		_fileTypeManager = fileTypeManager;

		LoadFileTypes();
		SetupEventHandlers();
	}

	private void SetupEventHandlers()
	{
		SearchTextBox.TextChanged += OnSearchTextChanged;
		FileTypeListBox.SelectionChanged += OnFileTypeSelectionChanged;
	}

	private void LoadFileTypes()
	{
		var fileTypes = _fileTypeManager.FileTypes.GetFileTypes();

		_allFileTypes = fileTypes
			.Where(ft => ft != null && !string.IsNullOrEmpty(ft.ID))
			.OrderBy(ft => ft.ID, StringComparer.OrdinalIgnoreCase)
			.Select(ft => new FileTypeListItem
			{
				Id = ft.ID,
				Record = ft
			})
			.ToList();

		ApplyFilter(string.Empty);
	}

	private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
	{
		var searchText = SearchTextBox.Text ?? string.Empty;
		ApplyFilter(searchText);
	}

	private void ApplyFilter(string searchText)
	{
		IEnumerable<FileTypeListItem> filteredItems;

		if (string.IsNullOrWhiteSpace(searchText))
		{
			filteredItems = _allFileTypes;
		}
		else
		{
			var lowerSearch = searchText.ToLowerInvariant();
			filteredItems = _allFileTypes.Where(ft => MatchesSearch(ft.Record, lowerSearch));
		}

		FileTypeListBox.ItemsSource = filteredItems.ToList();

		// Select first item if available
		if (FileTypeListBox.ItemCount > 0)
		{
			FileTypeListBox.SelectedIndex = 0;
		}
		else
		{
			ClearProperties();
		}
	}

	private static bool MatchesSearch(FileTypeRecord record, string lowerSearch)
	{
		// Check ID
		if (record.ID?.ToLowerInvariant().Contains(lowerSearch) == true)
			return true;

		// Check Description
		if (record.Description?.ToLowerInvariant().Contains(lowerSearch) == true)
			return true;

		// Check Category
		if (record.Category?.ToLowerInvariant().Contains(lowerSearch) == true)
			return true;

		// Check MIME Types
		if (record.MIMETypes != null)
		{
			foreach (var mimeType in record.MIMETypes)
			{
				if (mimeType?.ToLowerInvariant().Contains(lowerSearch) == true)
					return true;
			}
		}

		// Check Extensions
		if (record.Extensions != null)
		{
			foreach (var extension in record.Extensions)
			{
				if (extension?.ToLowerInvariant().Contains(lowerSearch) == true)
					return true;
			}
		}

		return false;
	}

	private void OnFileTypeSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (FileTypeListBox.SelectedItem is FileTypeListItem selectedItem)
		{
			DisplayProperties(selectedItem.Record);
		}
		else
		{
			ClearProperties();
		}
	}

	private void DisplayProperties(FileTypeRecord record)
	{
		TypeIdTextBox.Text = record.ID ?? string.Empty;
		DescriptionTextBox.Text = record.Description ?? string.Empty;
		CategoryTextBox.Text = FormatCategory(record);
		MimeTypesTextBox.Text = record.MIMETypes != null
			? string.Join(", ", record.MIMETypes)
			: string.Empty;
		ExtensionsTextBox.Text = record.Extensions != null
			? string.Join(", ", record.Extensions)
			: string.Empty;
		WikipediaTextBox.Text = record.Wikipedia ?? string.Empty;
	}

	private static string FormatCategory(FileTypeRecord record)
	{
		if (string.IsNullOrEmpty(record.Category))
			return string.Empty;

		if (string.IsNullOrEmpty(record.SubCategory))
			return record.Category;

		return $"{record.Category} / {record.SubCategory}";
	}

	private void ClearProperties()
	{
		TypeIdTextBox.Text = string.Empty;
		DescriptionTextBox.Text = string.Empty;
		CategoryTextBox.Text = string.Empty;
		MimeTypesTextBox.Text = string.Empty;
		ExtensionsTextBox.Text = string.Empty;
		WikipediaTextBox.Text = string.Empty;
	}

	private void OnCloseClick(object? sender, RoutedEventArgs e)
	{
		Close();
	}
}
