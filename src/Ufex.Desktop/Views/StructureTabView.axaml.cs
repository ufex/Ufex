using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using UfexFileType = Ufex.API.FileType;

namespace Ufex.Desktop.Views;

/// <summary>
/// View model for tree nodes displayed in the structure tree view.
/// </summary>
public class StructureTreeNode
{
	public string Text { get; set; } = string.Empty;
	public TreeNode SourceNode { get; set; } = null!;
	public Symbol Icon { get; set; } = Symbol.Document;
	public ObservableCollection<StructureTreeNode> Children { get; } = new();
}

/// <summary>
/// Represents a row in the structure data grid.
/// </summary>
public class StructureDataRow
{
	private readonly Dictionary<string, string> _values = new();

	public string this[string columnName]
	{
		get => _values.TryGetValue(columnName, out var value) ? value : string.Empty;
		set => _values[columnName] = value;
	}

	public string Col0 { get; set; } = string.Empty;
	public string Col1 { get; set; } = string.Empty;
	public string Col2 { get; set; } = string.Empty;
	public string Col3 { get; set; } = string.Empty;
	public string Col4 { get; set; } = string.Empty;
	public string Col5 { get; set; } = string.Empty;
	public string Col6 { get; set; } = string.Empty;
	public string Col7 { get; set; } = string.Empty;
}

public partial class StructureTabView : UserControl
{
	private TreeView? _treeView;
	private DataGrid? _dataGrid;
	private UfexFileType? _fileType;
	private DataFormatter? _dataFormatter;

	public StructureTabView()
	{
		InitializeComponent();
		_treeView = this.FindControl<TreeView>("TreeViewStructure");
		_dataGrid = this.FindControl<DataGrid>("DataGridStructure");

		if (_treeView != null)
		{
			_treeView.SelectionChanged += OnTreeSelectionChanged;

			// Set up the item template for the tree view
			_treeView.ItemTemplate = new FuncTreeDataTemplate<StructureTreeNode>(
				(node, _) =>
				{
					var panel = new StackPanel
					{
						Orientation = Orientation.Horizontal,
						Spacing = 6
					};

					var icon = new SymbolIcon
					{
						Symbol = node.Icon,
						FontSize = 16
					};
					panel.Children.Add(icon);

					var textBlock = new TextBlock
					{
						Text = node.Text,
						VerticalAlignment = VerticalAlignment.Center
					};
					panel.Children.Add(textBlock);

					return panel;
				},
				node => node.Children
			);
		}
	}

	/// <summary>
	/// Clears all content from the structure view.
	/// </summary>
	public void Clear()
	{
		_fileType = null;
		_dataFormatter = null;

		if (_treeView != null)
		{
			_treeView.ItemsSource = null;
		}

		if (_dataGrid != null)
		{
			_dataGrid.Columns.Clear();
			_dataGrid.ItemsSource = null;
		}
	}

	/// <summary>
	/// Sets the file type instance and data formatter for this view.
	/// </summary>
	public void SetFileType(UfexFileType? fileType, DataFormatter? dataFormatter = null)
	{
		_fileType = fileType;
		_dataFormatter = dataFormatter ?? new DataFormatter();
	}

	/// <summary>
	/// Sets the number format on the current formatter and refreshes the selected node.
	/// </summary>
	public void SetNumberFormat(NumberFormat numberFormat)
	{
		_dataFormatter ??= new DataFormatter();
		_dataFormatter.NumFormat = numberFormat;
		RefreshSelectedNode();
	}

	/// <summary>
	/// Loads the tree structure from the file type's TreeNodes collection.
	/// </summary>
	public void LoadTreeNodes(TreeNodeCollection? treeNodes)
	{
		if (_treeView == null || treeNodes == null)
		{
			if (_treeView != null)
				_treeView.ItemsSource = null;
			return;
		}

		var rootNodes = new ObservableCollection<StructureTreeNode>();

		foreach (var node in treeNodes)
		{
			var viewNode = ConvertToViewNode(node);
			rootNodes.Add(viewNode);
		}

		_treeView.ItemsSource = rootNodes;
	}

	/// <summary>
	/// Converts a TreeNode to a StructureTreeNode for display.
	/// </summary>
	private StructureTreeNode ConvertToViewNode(TreeNode node)
	{
		var viewNode = new StructureTreeNode
		{
			Text = node.Text,
			SourceNode = node,
			Icon = MapTreeViewIcon(node.ImageIndex)
		};

		// Recursively add children
		foreach (var child in node.Nodes)
		{
			viewNode.Children.Add(ConvertToViewNode(child));
		}

		return viewNode;
	}

	/// <summary>
	/// Maps TreeViewIcon enum values to FluentIcons Symbol values.
	/// </summary>
	private static Symbol MapTreeViewIcon(int iconIndex)
	{
		var icon = (TreeViewIcon)iconIndex;
		return icon switch
		{
			TreeViewIcon.NullIcon => Symbol.Document,
			TreeViewIcon.Section => Symbol.DocumentBulletList,
			TreeViewIcon.Properties => Symbol.Settings,
			TreeViewIcon.Header => Symbol.DocumentHeader,
			TreeViewIcon.Comment => Symbol.Comment,
			TreeViewIcon.Text => Symbol.TextDescription,
			TreeViewIcon.Table => Symbol.Table,
			TreeViewIcon.Image => Symbol.Image,
			TreeViewIcon.FolderOpen => Symbol.FolderOpen,
			TreeViewIcon.FolderClosed => Symbol.Folder,
			TreeViewIcon.Document => Symbol.Document,
			TreeViewIcon.Object => Symbol.Cube,
			TreeViewIcon.Gear => Symbol.Settings,
			TreeViewIcon.Binary => Symbol.Code,
			TreeViewIcon.Information => Symbol.Info,
			TreeViewIcon.Palette => Symbol.Color,
			_ => Symbol.Document
		};
	}

	/// <summary>
	/// Handles tree view selection changes to load the corresponding data.
	/// </summary>
	private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (_fileType == null || _dataGrid == null)
			return;

		var selectedItem = _treeView?.SelectedItem as StructureTreeNode;
		if (selectedItem?.SourceNode == null)
		{
			_dataGrid.Columns.Clear();
			_dataGrid.ItemsSource = null;
			return;
		}

		try
		{
			// Call GetData on the file type with the selected node
			var tableData = _fileType.GetData(selectedItem.SourceNode);
			LoadTableData(tableData);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading data for node: {ex.Message}");
			_dataGrid.Columns.Clear();
			_dataGrid.ItemsSource = null;
		}
	}

	/// <summary>
	/// Loads TableData into the data grid.
	/// </summary>
	public void LoadTableData(TableData? tableData)
	{
		if (_dataGrid == null)
			return;

		if (tableData == null || tableData.NumRows == 0)
		{
			_dataGrid.Columns.Clear();
			_dataGrid.ItemsSource = null;
			return;
		}

		var formatter = _dataFormatter ??= new DataFormatter();

		// Get the DataTable
		var dataTable = tableData.GetDataTable(formatter);

		// Clear and rebuild columns
		_dataGrid.Columns.Clear();

		// Create columns based on the DataTable columns
		string[] colBindings = { "Col0", "Col1", "Col2", "Col3", "Col4", "Col5", "Col6", "Col7" };
		for (int i = 0; i < dataTable.Columns.Count && i < colBindings.Length; i++)
		{
			var column = dataTable.Columns[i];
			_dataGrid.Columns.Add(new DataGridTextColumn
			{
				Header = column.ColumnName,
				Binding = new Binding(colBindings[i]),
				Width = i == dataTable.Columns.Count - 1
					? new DataGridLength(1, DataGridLengthUnitType.Star)
					: new DataGridLength(150)
			});
		}

		// Convert rows to StructureDataRow objects
		var rows = new List<StructureDataRow>();
		foreach (System.Data.DataRow row in dataTable.Rows)
		{
			var dataRow = new StructureDataRow();

			for (int i = 0; i < dataTable.Columns.Count && i < colBindings.Length; i++)
			{
				var value = row[i]?.ToString() ?? string.Empty;
				switch (i)
				{
					case 0: dataRow.Col0 = value; break;
					case 1: dataRow.Col1 = value; break;
					case 2: dataRow.Col2 = value; break;
					case 3: dataRow.Col3 = value; break;
					case 4: dataRow.Col4 = value; break;
					case 5: dataRow.Col5 = value; break;
					case 6: dataRow.Col6 = value; break;
					case 7: dataRow.Col7 = value; break;
				}
			}

			rows.Add(dataRow);
		}

		_dataGrid.ItemsSource = rows;
	}

	/// <summary>
	/// Updates the data formatter and refreshes the current data display.
	/// </summary>
	public void SetDataFormatter(DataFormatter dataFormatter)
	{
		_dataFormatter = dataFormatter;
		RefreshSelectedNode();
	}

	private void RefreshSelectedNode()
	{
		// Re-load the currently selected node's data with the current formatter
		if (_fileType == null)
			return;
		if (_treeView?.SelectedItem is not StructureTreeNode selectedItem)
			return;

		try
		{
			var tableData = _fileType.GetData(selectedItem.SourceNode);
			LoadTableData(tableData);
		}
		catch
		{
			// Ignore errors during refresh
		}
	}
}
