using System;
using System.Collections;
using Ufex.API.Format;

namespace Ufex.API.Tables;

/// <summary>
/// A table data structure that supports values changing based on the `Ufex.API.DataFormatter`.
/// </summary>
public class DynamicTableData : Ufex.API.Tables.TableData
{
	/// <summary>
	/// Structure to hold a single row of dynamic table data.
	/// </summary>
	struct ObjRow
	{
		public int numCols;
		public object[] data;
	}

	public DynamicTableData()
	{
		IsDynamic = true;
	}

	public DynamicTableData(int numCols)
	{
		NumColumns = numCols;
		columns = new ArrayList(numCols);

		IsDynamic = true;

		for(int c = 0; c < NumColumns; c++)
		{
			TableData.Column newCol = new TableData.Column();
			newCol.name = "";
			newCol.width = TableData.DEF_COL_WIDTH;
			newCol.alignment = ColumnAlignment.Default;
			columns.Add(newCol);
		}
		return;
	}

	public DynamicTableData(int numCols, string templateName) : this(numCols)
	{
		TemplateName = templateName;
	}

	public void AddRow()
	{
		AddRowData(new object[] { });
	}

	public void AddRow(Object? c1)
	{
		AddRowData(new object[] { c1 });
	}

	public void AddRow(Object? c1, Object? c2)
	{
		AddRowData(new object[] { c1, c2 });
	}

	public void AddRow(Object? c1, Object? c2, Object? c3)
	{
		AddRowData(new object[] { c1, c2, c3 });
	}

	public void AddRow(Object? c1, Object? c2, Object? c3, Object? c4)
	{
		AddRowData(new object[] { c1, c2, c3, c4 });
	}

	public void AddRow(Object? c1, Object? c2, Object? c3, Object? c4, Object? c5)
	{
		AddRowData(new object[] { c1, c2, c3, c4, c5 });
	}

	/// <summary>
	/// Adds a new row to the Dynamic table.
	/// </summary>
	/// <param name="rowData">An array of objects representing the data for each column in the new row.</param>
	public void AddRowData(Object[] rowData)
	{
		// Create a new ROW object
		ObjRow newRow = new ObjRow();

		// Set the number of columns
		newRow.numCols = rowData.Length;

		// Set the row text data
		newRow.data = rowData;

		// Add the row to the rowData ArrayList
		base.rowData.Add(newRow);

		// Increment the number of rows
		NumRows++;
		return;
	}

	public void AddRows(object[][] rows)
	{
		foreach(var row in rows)
		{
			AddRowData(row);
		}
	}

	public void SetItem(int rowNum, int colNum, String text)
	{
		if (rowNum >= NumRows)
		{
			sizeError = true;
			return;
		}
		if (colNum >= NumColumns)
		{
			sizeError = true;
			return;
		}
		ObjRow r = (ObjRow)rowData[rowNum];
		if (colNum > r.numCols)
		{
			sizeError = true;
			return;
		}
		r.data[colNum] = text;
		rowData[rowNum] = r;
	}

	/// <summary>
	/// Gets the row data for the specified row, formatted using the provided DataFormatter.
	/// </summary>
	/// <param name="r">The row index.</param>
	/// <param name="nts">The data formatter to use.</param>
	/// <returns>An array of formatted strings representing the row data.</returns>
	protected override string[] GetRow(int r, DataFormatter nts)
	{
		string[] rowData = new string[NumColumns];

		// Create a temporary row
		ObjRow tmpRow = (ObjRow)base.rowData[r];

		for(int c = 0; c < NumColumns; c++)
		{
			if(c < tmpRow.numCols)
			{
				rowData[c] = nts.Object(tmpRow.data[c]);
			}
			else
			{
				rowData[c] = "";
			}
		}
		return rowData;
	}

}
