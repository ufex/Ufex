using Ufex.API.Tables;

namespace Ufex.API.Visual;

/// <summary>
/// A visual representation of tabular data for display in a data grid.
/// </summary>
public class DataGridVisual : Visual
{
	public TableData Table { get; set; }

	public DataGridVisual(TableData table, string description) : base(description)
	{
		Table = table;
	}
}