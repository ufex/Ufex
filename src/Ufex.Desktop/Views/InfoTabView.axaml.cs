using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Format;
using Ufex.FileType;
using UfexFileInfo = Ufex.API.FileInfo;

namespace Ufex.Desktop.Views;

/// <summary>
/// Represents a row in the QuickInfo data grid.
/// </summary>
public class QuickInfoRow
{
	public string Property { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Represents an item in the file type dropdown.
/// <summary>
/// Represents an item in the file type dropdown.
/// </summary>
public class FileTypeDropdownItem
{
	public DetectionMatch Match { get; set; }
	public string Description { get; set; } = "";
	public string MatchMethodLabel { get; set; } = "";
	public string MatchDetailsText { get; set; } = "";
}

public partial class InfoTabView : UserControl
{
	private DataGrid? _dataGridInfo;
	private bool _suppressFileTypeChanged;
	private readonly Logger _logger = new Logger("Desktop_InfoTabView.log");

	public event EventHandler<DetectionMatch?>? FileTypeChanged;

	public InfoTabView()
	{
		InitializeComponent();
		_dataGridInfo = this.FindControl<DataGrid>("DataGridInfo");
	}

	/// <summary>
	/// Clears all file information from the view.
	/// </summary>
	public void Clear()
	{
		_suppressFileTypeChanged = true;
		ComboFileType.ItemsSource = null;
		ComboFileType.SelectedIndex = -1;
		ComboFileType.IsEnabled = true;
		_suppressFileTypeChanged = false;
		BtnMatchInfo.IsVisible = false;
		TextFilePath.Text = string.Empty;
		TextFileName.Text = string.Empty;
		TextFileExtension.Text = string.Empty;
		TextFileSize.Text = string.Empty;

		ChkReadOnly.IsChecked = false;
		ChkHidden.IsChecked = false;
		ChkArchive.IsChecked = false;
		ChkNormal.IsChecked = false;
		ChkTemporary.IsChecked = false;
		ChkSystem.IsChecked = false;
		ChkEncrypted.IsChecked = false;
		ChkCompressed.IsChecked = false;
		ChkSparseFile.IsChecked = false;

		TextCreated.Text = string.Empty;
		TextModified.Text = string.Empty;
		TextAccessed.Text = string.Empty;

		if (_dataGridInfo != null)
		{
			_dataGridInfo.Columns.Clear();
			_dataGridInfo.ItemsSource = null;
		}
	}

	/// <summary>
	/// Sets the file type description (fallback for when no detection result is available).
	/// </summary>
	public void SetFileType(string? fileType)
	{
		_suppressFileTypeChanged = true;
		try
		{
			var items = new List<FileTypeDropdownItem>
			{
				new FileTypeDropdownItem
				{
					Description = fileType ?? "Unknown File Type",
					MatchMethodLabel = "",
					MatchDetailsText = "",
				}
			};
			ComboFileType.ItemsSource = items;
			ComboFileType.SelectedIndex = 0;
			ComboFileType.IsEnabled = true;
			BtnMatchInfo.IsVisible = false;
		}
		finally
		{
			_suppressFileTypeChanged = false;
		}
	}

	/// <summary>
	/// Sets the detection result with all detected file types.
	/// </summary>
	public void SetDetectionResult(DetectionResult result, int selectedIndex = 0)
	{
		if (result == null || result.Count == 0)
		{
			SetFileType(null);
			return;
		}

		_suppressFileTypeChanged = true;
		try
		{
			var items = new List<FileTypeDropdownItem>();
			foreach (var match in result.Matches)
			{
				items.Add(new FileTypeDropdownItem
				{
					Match = match,
					Description = match.FileType.Description,
					MatchMethodLabel = DetectionResultFormatter.FormatMatchMethodLabel(match.Method),
					MatchDetailsText = DetectionResultFormatter.FormatMatchDetails(match),
				});
			}

			ComboFileType.ItemsSource = items;
			ComboFileType.SelectedIndex = Math.Min(selectedIndex, items.Count - 1);
			ComboFileType.IsEnabled = true;
			BtnMatchInfo.IsVisible = true;
		}
		finally
		{
			_suppressFileTypeChanged = false;
		}
	}

	private void OnFileTypeSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (_suppressFileTypeChanged)
			return;

		if (ComboFileType.SelectedItem is FileTypeDropdownItem item)
		{
			FileTypeChanged?.Invoke(this, item.Match);
		}
	}

	private async void OnMatchInfoClick(object? sender, RoutedEventArgs e)
	{
		if (ComboFileType.SelectedItem is FileTypeDropdownItem item)
		{
			try
			{
				var detailsWindow = new MatchDetailsWindow(item.Description, item.MatchDetailsText);

				if (TopLevel.GetTopLevel(this) is Window ownerWindow)
				{
					await detailsWindow.ShowDialog(ownerWindow);
				}
				else
				{
					detailsWindow.Show();
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "InfoTabView.OnMatchInfoClick: Failed to show match details window");
			}
		}
	}

	/// <summary>
	/// Loads file information into the view using Ufex.API.FileInfo.
	/// </summary>
	/// <param name="fileInfo">The FileInfo instance from Ufex.API.</param>
	public void LoadFileInfo(UfexFileInfo fileInfo)
	{
		if (fileInfo == null)
		{
			Clear();
			return;
		}

		// Basic file info
		TextFilePath.Text = fileInfo.FilePath;
		TextFileName.Text = fileInfo.FileName;
		TextFileExtension.Text = fileInfo.FileExtension;
		TextFileSize.Text = FormatFileSize(long.Parse(fileInfo.FileSize));

		// Load attributes based on file system type
		LoadAttributes(fileInfo);

		// Timestamps - use System.IO.FileInfo for now until Ufex.API.FileInfo exposes these
		var sysFileInfo = new System.IO.FileInfo(fileInfo.FilePath);
		TextCreated.Text = sysFileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
		TextModified.Text = sysFileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
		TextAccessed.Text = sysFileInfo.LastAccessTime.ToString("yyyy-MM-dd HH:mm:ss");
	}

	/// <summary>
	/// Loads file information using a file path (fallback method).
	/// </summary>
	/// <param name="filePath">Full path to the file.</param>
	/// <param name="fileType">Detected file type description.</param>
	public void LoadFileInfo(string filePath, string? fileType = null)
	{
		if (!File.Exists(filePath))
		{
			Clear();
			return;
		}

		var sysFileInfo = new System.IO.FileInfo(filePath);

		// Basic file info
		SetFileType(fileType);
		TextFilePath.Text = filePath;
		TextFileName.Text = sysFileInfo.Name;
		TextFileExtension.Text = sysFileInfo.Extension;
		TextFileSize.Text = FormatFileSize(sysFileInfo.Length);

		// File attributes
		var attrs = sysFileInfo.Attributes;
		ChkReadOnly.IsChecked = attrs.HasFlag(FileAttributes.ReadOnly);
		ChkHidden.IsChecked = attrs.HasFlag(FileAttributes.Hidden);
		ChkArchive.IsChecked = attrs.HasFlag(FileAttributes.Archive);
		ChkNormal.IsChecked = attrs.HasFlag(FileAttributes.Normal);
		ChkTemporary.IsChecked = attrs.HasFlag(FileAttributes.Temporary);
		ChkSystem.IsChecked = attrs.HasFlag(FileAttributes.System);
		ChkEncrypted.IsChecked = attrs.HasFlag(FileAttributes.Encrypted);
		ChkCompressed.IsChecked = attrs.HasFlag(FileAttributes.Compressed);
		ChkSparseFile.IsChecked = attrs.HasFlag(FileAttributes.SparseFile);

		// Timestamps
		TextCreated.Text = sysFileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
		TextModified.Text = sysFileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
		TextAccessed.Text = sysFileInfo.LastAccessTime.ToString("yyyy-MM-dd HH:mm:ss");
	}

	private void LoadAttributes(UfexFileInfo fileInfo)
	{
		// Reset all checkboxes first
		ChkReadOnly.IsChecked = false;
		ChkHidden.IsChecked = false;
		ChkArchive.IsChecked = false;
		ChkNormal.IsChecked = false;
		ChkTemporary.IsChecked = false;
		ChkSystem.IsChecked = false;
		ChkEncrypted.IsChecked = false;
		ChkCompressed.IsChecked = false;
		ChkSparseFile.IsChecked = false;

		// Load attributes based on file system type
		if (fileInfo is Ufex.API.FileSystem.NtfsFileInfo ntfsInfo)
		{
			ChkReadOnly.IsChecked = ntfsInfo.ReadOnly;
			ChkHidden.IsChecked = ntfsInfo.Hidden;
			ChkArchive.IsChecked = ntfsInfo.Archive;
			ChkNormal.IsChecked = ntfsInfo.Normal;
			ChkTemporary.IsChecked = ntfsInfo.Temporary;
			ChkSystem.IsChecked = ntfsInfo.System;
			ChkEncrypted.IsChecked = ntfsInfo.Encrypted;
			ChkCompressed.IsChecked = ntfsInfo.Compressed;
			ChkSparseFile.IsChecked = ntfsInfo.SparseFile;
		}
		else if (fileInfo is Ufex.API.FileSystem.Fat32FileInfo fat32Info)
		{
			ChkReadOnly.IsChecked = fat32Info.ReadOnly;
			ChkHidden.IsChecked = fat32Info.Hidden;
			ChkArchive.IsChecked = fat32Info.Archive;
			ChkSystem.IsChecked = fat32Info.System;
		}
		// Add other file systems as needed
	}

	/// <summary>
	/// Sets the custom data table content.
	/// </summary>
	/// <param name="items">Collection of items to display in the data grid.</param>
	public void SetDataTable<T>(IEnumerable<T> items)
	{
		if (_dataGridInfo != null)
			_dataGridInfo.ItemsSource = items;
	}

	/// <summary>
	/// Loads QuickInfo data from the file type handler into the data grid.
	/// </summary>
	/// <param name="quickInfo">The QuickInfoTableData from FileType.QuickInfo.</param>
	/// <param name="dataFormatter">Optional data formatter for number formatting.</param>
	public void LoadQuickInfo(QuickInfoTableData? quickInfo, DataFormatter? dataFormatter = null)
	{
		if (_dataGridInfo == null)
			return;

		if (quickInfo == null || quickInfo.NumRows == 0)
		{
			_dataGridInfo.Columns.Clear();
			_dataGridInfo.ItemsSource = null;
			return;
		}

		// Use default formatter if none provided
		var formatter = dataFormatter ?? new DataFormatter();

		// Get the DataTable and convert to a list of QuickInfoRow objects
		var dataTable = quickInfo.GetDataTable(formatter);
		var rows = new List<QuickInfoRow>();

		foreach (System.Data.DataRow row in dataTable.Rows)
		{
			rows.Add(new QuickInfoRow
			{
				Property = row[0]?.ToString() ?? string.Empty,
				Value = row[1]?.ToString() ?? string.Empty
			});
		}

		// Set up columns if not already configured
		if (_dataGridInfo.Columns.Count == 0)
		{
			_dataGridInfo.Columns.Add(new DataGridTextColumn
			{
				Header = "Property",
				Binding = new Avalonia.Data.Binding("Property"),
				Width = new DataGridLength(200)
			});
			_dataGridInfo.Columns.Add(new DataGridTextColumn
			{
				Header = "Value",
				Binding = new Avalonia.Data.Binding("Value"),
				Width = new DataGridLength(1, DataGridLengthUnitType.Star)
			});
		}

		_dataGridInfo.ItemsSource = rows;
	}

	private static string FormatFileSize(long bytes)
	{
		string[] sizes = { "B", "KB", "MB", "GB", "TB" };
		int order = 0;
		double size = bytes;

		while (size >= 1024 && order < sizes.Length - 1)
		{
			order++;
			size /= 1024;
		}

		return $"{size:N2} {sizes[order]} ({bytes:N0} bytes)";
	}
}
