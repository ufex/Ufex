using System;
using System.Collections;
using Ufex.API.Format;

namespace Ufex.API.Tables;

/// <summary>
/// Table data structure for displaying static text tables.
/// </summary>
public class TextTableData : Ufex.API.Tables.TableData
{
	/// <summary>
	/// Defines a row in the text table.
	/// </summary>
	struct Row
	{
		public int numCols;
		public string[] data;
	}

	// Constructors
	public TextTableData() : this(0)
	{
	}

	public TextTableData(int numCols)
	{
		IsDynamic = false;

		NumColumns = numCols;
		columns = new ArrayList(numCols);
		for (int c = 0; c < NumColumns; c++)
		{
			Column newCol = new Column();
			newCol.name = "";
			newCol.width = DEF_COL_WIDTH;
			newCol.alignment = ColumnAlignment.Default;
			columns.Add(newCol);
		}
	}

	// Destructor
	~TextTableData() 
	{
	}

	// Functions for adding rows
	public void AddRow()
	{
		AddRowData([]);
	}

	public void AddRow(string c1)
	{
		AddRowData([c1]);
	}

	public void AddRow(string c1, string c2)
	{
		AddRowData([c1, c2]);
	}

	public void AddRow(string c1, string c2, string c3)
	{
		AddRowData([c1, c2, c3]);
	}

	public void AddRow(string c1, string c2, string c3, string c4)
	{
		AddRowData([c1, c2, c3, c4]);
	}

	public void AddRow(string c1, string c2, string c3, string c4, string c5)
	{
		AddRowData([c1, c2, c3, c4, c5]);
	}

	public void AddRowData(string[] rowData)
	{
		// Create a new Row object
		Row newRow = new Row();

		// Set the number of columns
		newRow.numCols = rowData.Length;

		// Set the row text data
		newRow.data = rowData;

		// Add the row to the rowData ArrayList
		base.rowData.Add(newRow);

		// Increment the number of rows
		NumRows++;
	}

	public void SetItem(int rowNum, int colNum, string text)
	{
		if(rowNum >= NumRows)
		{
			sizeError = true;
			return;
		}
		if(colNum >= NumColumns)
		{
			sizeError = true;
			return;
		}

		Row r = (Row)rowData[rowNum];

		if(colNum > r.numCols)
		{
			sizeError = true;
			return;
		}
		r.data[colNum] = text;
		rowData[rowNum] = r;
	}

	/// <summary>
	/// Retrieves the data for a specific row as an array of strings.
	/// </summary>
	/// <param name="r">The row index.</param>
	/// <param name="nts">The DataFormatter - ignored for TextTableData</param>
	/// <returns>The array of strings representing the row data.</returns>
	protected override string[] GetRow(int r, DataFormatter nts)
	{
		string[] rowData = new string[NumColumns];

		// Create a temporary row
		Row tmpRow = (Row)base.rowData[r];

		for (int c = 0; c < NumColumns; c++)
		{
			if (c < tmpRow.numCols)
				rowData[c] = tmpRow.data[c];
			else
				rowData[c] = "";
		}
		return rowData;
	}

}
