using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Ufex.API;
using Ufex.API.Tables;
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

public partial class InfoTabView : UserControl
{
	private DataGrid? _dataGridInfo;

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
		TextFileType.Text = string.Empty;
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
	/// Sets the file type description.
	/// </summary>
	public void SetFileType(string? fileType)
	{
		TextFileType.Text = fileType ?? "Unknown File Type";
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
		TextFileType.Text = fileType ?? "Unknown File Type";
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
	/// <param name="quickInfo">The QuickInfoTableData from FileType.GetQuickInfo().</param>
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
