using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ufex.FileType;
using Ufex.FileType.Config;

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
	private readonly FileTypeManager? _fileTypeManager;
	private List<FileTypeListItem> _allFileTypes = new();

	public FileTypeManagerWindow()
	{
		InitializeComponent();
		SetupEventHandlers();
		ClearProperties();
	}

	public FileTypeManagerWindow(FileTypeManager fileTypeManager)
	{
		InitializeComponent();
		_fileTypeManager = fileTypeManager;
		SetupEventHandlers();
		LoadFileTypes();
	}

	private void SetupEventHandlers()
	{
		SearchTextBox.TextChanged += OnSearchTextChanged;
		FileTypeListBox.SelectionChanged += OnFileTypeSelectionChanged;
	}

	private void LoadFileTypes()
	{
		if (_fileTypeManager == null)
		{
			_allFileTypes = new List<FileTypeListItem>();
			FileTypeListBox.ItemsSource = _allFileTypes;
			ClearProperties();
			DescriptionTextBox.Text = "File Type Manager is not initialized.";
			return;
		}

		FileTypeRecord[] fileTypes;
		try
		{
			fileTypes = _fileTypeManager.FileTypes.GetFileTypes();
		}
		catch (Exception ex)
		{
			_allFileTypes = new List<FileTypeListItem>();
			FileTypeListBox.ItemsSource = _allFileTypes;
			ClearProperties();
			DescriptionTextBox.Text = ex.Message;
			return;
		}

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

		// Check signatures (new Ufex.FileType model)
		if (record.Signatures != null)
		{
			foreach (var signature in record.Signatures)
			{
				if (signature?.Items == null)
					continue;

				foreach (var node in signature.Items)
				{
					if (node is Rule rule)
					{
						if (rule.Type?.ToLowerInvariant().Contains(lowerSearch) == true)
							return true;
						if (rule.Value?.ToLowerInvariant().Contains(lowerSearch) == true)
							return true;
					}
					else if (node is SearchRule searchRule)
					{
						if (searchRule.Type?.ToLowerInvariant().Contains(lowerSearch) == true)
							return true;
						if (searchRule.Value?.ToLowerInvariant().Contains(lowerSearch) == true)
							return true;
					}
					else if (node is RuleRef ruleRef)
					{
						if (ruleRef.Name?.ToLowerInvariant().Contains(lowerSearch) == true)
							return true;
					}
				}
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
		SignaturesTextBox.Text = FormatSignatures(record);
	}

	private static string FormatSignatures(FileTypeRecord record)
	{
		if (record.Signatures == null || record.Signatures.Count == 0)
			return string.Empty;

		StringBuilder output = new StringBuilder();
		for (int i = 0; i < record.Signatures.Count; i++)
		{
			Signature? signature = record.Signatures[i];
			if (signature == null)
				continue;

			if (output.Length > 0)
				output.AppendLine();

			output.Append("Signature ").Append(i + 1);
			if (signature.MinSize > 0)
			{
				output.Append(" (minSize=").Append(signature.MinSize).Append(')');
			}
			output.AppendLine();

			if (signature.Items == null || signature.Items.Count == 0)
			{
				output.AppendLine("  - (no items)");
				continue;
			}

			foreach (var item in signature.Items)
			{
				output.Append("  - ").AppendLine(FormatSignatureItem(item));
			}
		}

		return output.ToString();
	}

	private static string FormatSignatureItem(SignatureNode node)
	{
		if (node is Rule rule)
		{
			string offset = rule.Offset != null ? rule.Offset.RawExpression : "0";
			string value = rule.Value ?? string.Empty;
			return $"Rule type={rule.Type}, op={rule.Operator}, offset={offset}, value={value}";
		}

		if (node is SearchRule searchRule)
		{
			string offset = searchRule.Offset != null ? searchRule.Offset.RawExpression : "0";
			string value = searchRule.Value ?? string.Empty;
			string maxLength = searchRule.MaxLengthSpecified ? searchRule.MaxLength.ToString() : "(none)";
			return $"SearchRule type={searchRule.Type}, op={searchRule.Operator}, offset={offset}, maxLength={maxLength}, value={value}";
		}

		if (node is RuleRef ruleRef)
		{
			return $"RuleRef name={ruleRef.Name}";
		}

		if (node is RuleGroup ruleGroup)
		{
			string baseOffset = ruleGroup.BaseOffset != null ? ruleGroup.BaseOffset.RawExpression : "(none)";
			int itemCount = ruleGroup.Items != null ? ruleGroup.Items.Count : 0;
			return $"RuleGroup match={ruleGroup.Match}, base={baseOffset}, items={itemCount}";
		}

		return node.GetType().Name;
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
		SignaturesTextBox.Text = string.Empty;
	}

	private void OnCloseClick(object? sender, RoutedEventArgs e)
	{
		Close();
	}
}
