using Avalonia.Controls;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using System.Collections.ObjectModel;
using Ufex.API.Validation;

namespace Ufex.Desktop.Views;

/// <summary>
/// Represents a row in the validation data grid.
/// </summary>
public class ValidationEntryRow
{
	public SymbolIcon Icon { get; set; } = null!;
	public string TypeText { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
}

public partial class ValidationTabView : UserControl
{
	private DataGrid? _dataGrid;
	private TextBlock? _summaryText;
	private ObservableCollection<ValidationEntryRow> _entries = new();

	public ValidationTabView()
	{
		InitializeComponent();
		_dataGrid = this.FindControl<DataGrid>("DataGridValidation");
		_summaryText = this.FindControl<TextBlock>("SummaryText");

		if (_dataGrid != null)
		{
			_dataGrid.ItemsSource = _entries;
		}
	}

	/// <summary>
	/// Loads the validation report into the data grid.
	/// </summary>
	/// <param name="report">The validation report to display.</param>
	public void LoadValidationReport(ValidationReport? report)
	{
		_entries.Clear();

		if (report == null || report.Entries.Length == 0)
		{
			UpdateSummary(0, 0, 0);
			return;
		}

		int infoCount = 0;
		int warningCount = 0;
		int errorCount = 0;

		foreach (var entry in report.Entries)
		{
			var row = new ValidationEntryRow
			{
				Message = entry.Message
			};

			switch (entry.Type)
			{
				case ValidationReport.EntryType.Info:
					row.Icon = CreateIcon(Symbol.Info, Brushes.DodgerBlue);
					row.TypeText = "Info";
					infoCount++;
					break;

				case ValidationReport.EntryType.Warning:
					row.Icon = CreateIcon(Symbol.Warning, Brushes.Orange);
					row.TypeText = "Warning";
					warningCount++;
					break;

				case ValidationReport.EntryType.Error:
					row.Icon = CreateIcon(Symbol.ErrorCircle, Brushes.Red);
					row.TypeText = "Error";
					errorCount++;
					break;
			}

			_entries.Add(row);
		}

		UpdateSummary(infoCount, warningCount, errorCount);
	}

	/// <summary>
	/// Creates an icon with the specified symbol and color.
	/// </summary>
	private SymbolIcon CreateIcon(Symbol symbol, IBrush foreground)
	{
		return new SymbolIcon
		{
			Symbol = symbol,
			FontSize = 16,
			Foreground = foreground
		};
	}

	/// <summary>
	/// Updates the summary text at the bottom of the view.
	/// </summary>
	private void UpdateSummary(int infoCount, int warningCount, int errorCount)
	{
		if (_summaryText == null) return;

		int total = infoCount + warningCount + errorCount;

		if (total == 0)
		{
			_summaryText.Text = "No validation results";
		}
		else
		{
			_summaryText.Text = $"{errorCount} error(s), {warningCount} warning(s), {infoCount} info";
		}

		// Set color based on severity
		if (errorCount > 0)
		{
			_summaryText.Foreground = Brushes.Red;
		}
		else if (warningCount > 0)
		{
			_summaryText.Foreground = Brushes.Orange;
		}
		else
		{
			_summaryText.Foreground = Brushes.Green;
		}
	}

	/// <summary>
	/// Clears the validation data grid.
	/// </summary>
	public void Clear()
	{
		_entries.Clear();
		UpdateSummary(0, 0, 0);
	}
}
