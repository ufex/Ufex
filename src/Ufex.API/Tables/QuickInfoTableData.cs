using System;

namespace Ufex.API.Tables;

public class QuickInfoTableData : Ufex.API.Tables.TableData
{
	/// <summary>
	/// Structure to hold a single row of Quick Info data.
	/// </summary>
	struct QuickInfoRow
	{
		public string[] data;
	}

	public QuickInfoTableData()
	{
		AddColumn("Property", 200, ColumnAlignment.Left);
		AddColumn("Value", 200, ColumnAlignment.Left);
	}
	
	/// <summary>
	/// Adds a new row to the Quick Info table.
	/// </summary>
	/// <param name="property">The property name to add to the row.</param>
	/// <param name="value">The value corresponding to the property.</param>
	public void AddRow(string property, string value)
	{
		// Create a new ROW object
		QuickInfoRow newRow = new QuickInfoRow();

		// Set the row text data
		newRow.data = [property, value];

		// Add the row to the rowData ArrayList
		rowData.Add(newRow);

		// Increment the number of rows
		numRows++;
		return;
	}

	protected override string[] GetRow(int r, DataFormatter nts)
	{
		return ((QuickInfoRow)rowData[r]).data;
	}
}
