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
using System.Linq;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.API.Format;
using Ufex.Controls.Avalonia;
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
	private ContentControl? _singleVisualContent;
	private TabControl? _visualsTabControl;
	private UfexFileType? _fileType;
	private DataFormatter? _dataFormatter;
	private DesktopSettings? _settings;
	private string? _currentTemplateName;
	private DataGrid? _currentDataGrid;

	public StructureTabView()
	{
		InitializeComponent();
		_treeView = this.FindControl<TreeView>("TreeViewStructure");
		_singleVisualContent = this.FindControl<ContentControl>("SingleVisualContent");
		_visualsTabControl = this.FindControl<TabControl>("VisualsTabControl");

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
		_currentTemplateName = null;
		_currentDataGrid = null;

		if (_treeView != null)
		{
			_treeView.ItemsSource = null;
		}

		ClearVisuals();
	}

	/// <summary>
	/// Clears the visuals area.
	/// </summary>
	public void ClearVisuals()
	{
		if (_singleVisualContent != null)
		{
			_singleVisualContent.Content = null;
			_singleVisualContent.IsVisible = false;
		}

		if (_visualsTabControl != null)
		{
			_visualsTabControl.ItemsSource = null;
			_visualsTabControl.IsVisible = false;
		}

		_currentDataGrid = null;
		_currentTemplateName = null;
	}

	/// <summary>
	/// Clears the data table from the data grid.
	/// </summary>
	[Obsolete("Use ClearVisuals instead")]
	public void ClearTable()
	{
		ClearVisuals();
	}

	/// <summary>
	/// Sets the settings instance for saving/loading column widths.
	/// </summary>
	public void SetSettings(DesktopSettings settings)
	{
		_settings = settings;
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
			Icon = MapTreeViewIcon(node.Icon)
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
	private static Symbol MapTreeViewIcon(TreeViewIcon icon)
	{
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
		if (_fileType == null)
			return;

		var selectedItem = _treeView?.SelectedItem as StructureTreeNode;
		if (selectedItem?.SourceNode == null)
		{
			ClearVisuals();
			return;
		}

		try
		{
			var visuals = selectedItem.SourceNode.Visuals;
			LoadVisuals(visuals);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading data for node: {ex.Message}");
			ClearVisuals();
		}
	}

	/// <summary>
	/// Loads visuals into the right panel.
	/// </summary>
	private void LoadVisuals(Ufex.API.Visual.Visual[] visuals)
	{
		ClearVisuals();

		if (visuals == null || visuals.Length == 0)
		{
			return;
		}

		if (visuals.Length == 1)
		{
			// Single visual: display without tabs
			var control = CreateVisualControl(visuals[0]);
			if (control != null && _singleVisualContent != null)
			{
				_singleVisualContent.Content = control;
				_singleVisualContent.IsVisible = true;
			}
		}
		else
		{
			// Multiple visuals: display in tabs
			var tabItems = new List<TabItem>();
			foreach (var visual in visuals)
			{
				var control = CreateVisualControl(visual);
				if (control != null)
				{
					var tabItem = new TabItem
					{
						Header = visual.Description,
						Content = control
					};
					tabItems.Add(tabItem);
				}
			}

			if (tabItems.Count > 0 && _visualsTabControl != null)
			{
				_visualsTabControl.ItemsSource = tabItems;
				_visualsTabControl.SelectedIndex = 0;
				_visualsTabControl.IsVisible = true;
			}
		}
	}

	/// <summary>
	/// Creates an Avalonia control for the given visual.
	/// </summary>
	private Control? CreateVisualControl(Ufex.API.Visual.Visual visual)
	{
		return visual switch
		{
			DataGridVisual dgv => CreateDataGridControl(dgv),
			HexViewerVisual hvv => CreateHexViewControl(hvv),
			ImageVisual iv => CreateImageViewerControl(iv),
			_ => null
		};
	}

	/// <summary>
	/// Creates a DataGrid control for DataGridVisual.
	/// </summary>
	private DataGrid CreateDataGridControl(DataGridVisual visual)
	{
		var dataGrid = new DataGrid
		{
			AutoGenerateColumns = false,
			IsReadOnly = true,
			CanUserReorderColumns = false,
			CanUserResizeColumns = true,
			GridLinesVisibility = DataGridGridLinesVisibility.All,
			BorderThickness = new Avalonia.Thickness(1),
			FontFamily = new FontFamily("Courier New")
		};

		// Store reference to the current data grid for column width persistence
		_currentDataGrid = dataGrid;

		LoadTableDataIntoGrid(dataGrid, visual.Table);

		return dataGrid;
	}

	/// <summary>
	/// Creates a HexView control for HexViewerVisual.
	/// </summary>
	private HexView CreateHexViewControl(HexViewerVisual visual)
	{
		var hexView = new HexView();
		hexView.LoadStream(visual.Stream);
		return hexView;
	}

	/// <summary>
	/// Creates an ImageViewerControl for ImageVisual (RasterImage or VectorImage).
	/// </summary>
	private ImageViewerControl CreateImageViewerControl(ImageVisual visual)
	{
		var imageViewer = new ImageViewerControl
		{
			SourceImage = visual
		};
		return imageViewer;
	}

	/// <summary>
	/// Loads TableData into a data grid.
	/// </summary>
	private void LoadTableDataIntoGrid(DataGrid dataGrid, TableData? tableData)
	{
		// Unsubscribe from previous column width change events
		UnsubscribeFromColumnWidthChanges(dataGrid);

		if (tableData == null || tableData.NumRows == 0)
		{
			dataGrid.Columns.Clear();
			dataGrid.ItemsSource = null;
			_currentTemplateName = null;
			return;
		}

		var formatter = _dataFormatter ??= new DataFormatter();

		// Get the DataTable
		var dataTable = tableData.GetDataTable(formatter);

		// Store the template name for column width persistence
		_currentTemplateName = !string.IsNullOrEmpty(tableData.TemplateName) ? tableData.TemplateName : null;

		// Try to get saved column widths for this template
		List<double>? savedWidths = null;
		if (_currentTemplateName != null && _settings?.StructureColumnWidths != null)
		{
			_settings.StructureColumnWidths.TryGetValue(_currentTemplateName, out savedWidths);
		}

		// Clear and rebuild columns
		dataGrid.Columns.Clear();

		// Create columns based on the DataTable columns
		string[] colBindings = { "Col0", "Col1", "Col2", "Col3", "Col4", "Col5", "Col6", "Col7" };
		for (int i = 0; i < dataTable.Columns.Count && i < colBindings.Length; i++)
		{
			var column = dataTable.Columns[i];

			// Determine the column width
			DataGridLength width;
			if (savedWidths != null && i < savedWidths.Count && savedWidths[i] > 0)
			{
				// Use saved width
				width = new DataGridLength(savedWidths[i]);
			}
			else if (i == dataTable.Columns.Count - 1)
			{
				// Last column fills remaining space
				width = new DataGridLength(1, DataGridLengthUnitType.Star);
			}
			else
			{
				// Default width
				width = new DataGridLength(150);
			}

			dataGrid.Columns.Add(new DataGridTextColumn
			{
				Header = column.ColumnName,
				Binding = new Binding(colBindings[i]),
				Width = width
			});
		}

		// Subscribe to column width changes if we have a template name
		if (_currentTemplateName != null)
		{
			SubscribeToColumnWidthChanges(dataGrid);
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

		dataGrid.ItemsSource = rows;
	}

	/// <summary>
	/// Loads TableData into the data grid (legacy method for compatibility).
	/// </summary>
	public void LoadTableData(TableData? tableData)
	{
		if (_currentDataGrid == null)
		{
			// Create a new data grid visual and load it
			var visual = tableData != null ? new DataGridVisual(tableData, "Data") : null;
			if (visual != null)
			{
				LoadVisuals(new Ufex.API.Visual.Visual[] { visual });
			}
			else
			{
				ClearVisuals();
			}
			return;
		}

		LoadTableDataIntoGrid(_currentDataGrid, tableData);
	}

	/// <summary>
	/// Subscribes to column width change events for persisting widths.
	/// </summary>
	private void SubscribeToColumnWidthChanges(DataGrid dataGrid)
	{
		foreach (var column in dataGrid.Columns)
		{
			if (column is DataGridColumn dgColumn)
			{
				dgColumn.PropertyChanged += OnColumnPropertyChanged;
			}
		}
	}

	/// <summary>
	/// Unsubscribes from column width change events.
	/// </summary>
	private void UnsubscribeFromColumnWidthChanges(DataGrid dataGrid)
	{
		foreach (var column in dataGrid.Columns)
		{
			if (column is DataGridColumn dgColumn)
			{
				dgColumn.PropertyChanged -= OnColumnPropertyChanged;
			}
		}
	}

	/// <summary>
	/// Handles column property changes to detect width changes.
	/// </summary>
	private void OnColumnPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
	{
		if (e.Property.Name == "ActualWidth" || e.Property.Name == "Width")
		{
			SaveColumnWidths();
		}
	}

	/// <summary>
	/// Saves the current column widths to settings.
	/// </summary>
	private void SaveColumnWidths()
	{
		if (_currentDataGrid == null || _settings == null || string.IsNullOrEmpty(_currentTemplateName))
			return;

		var widths = _currentDataGrid.Columns
			.Select(c => c.ActualWidth)
			.ToList();

		// Only save if we have valid widths (all > 0)
		if (widths.All(w => w > 0))
		{
			_settings.StructureColumnWidths[_currentTemplateName] = widths;
			_settings.Save();
		}
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
			var visuals = selectedItem.SourceNode.Visuals;
			LoadVisuals(visuals);
		}
		catch
		{
			// Ignore errors during refresh
		}
	}
}
