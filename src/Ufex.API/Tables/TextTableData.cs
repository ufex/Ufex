using System;
using System.Collections;

namespace Ufex.API.Tables
{
	public class TextTableData : Ufex.API.Tables.TableData
	{
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

			numColumns = numCols;
			columns = new ArrayList(numCols);
			for (int c = 0; c < numColumns; c++)
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
			AddRowData(new string[] { });
		}

		public void AddRow(string c1)
		{
			AddRowData(new string[] { c1 });
		}

		public void AddRow(string c1, string c2)
		{
			AddRowData(new string[] { c1, c2 });
		}

		public void AddRow(string c1, string c2, string c3)
		{
			AddRowData(new string[] { c1, c2, c3 });
		}

		public void AddRow(string c1, string c2, string c3, string c4)
		{
			AddRowData(new string[] { c1, c2, c3, c4 });
		}

		public void AddRow(string c1, string c2, string c3, string c4, string c5)
		{
			AddRowData(new string[] { c1, c2, c3, c4, c5 });
		}

		public void AddRowData(string[] rowData)
		{
			// Create a new ROW object
			Row newRow = new Row();

			// Set the number of columns
			newRow.numCols = rowData.Length;

			// Set the row text data
			newRow.data = rowData;

			// Add the row to the m_RowData ArrayList
			base.rowData.Add(newRow);

			// Increment the number of rows
			numRows++;
		}

		public void SetItem(int rowNum, int colNum, string text)
		{
			if (rowNum >= numRows)
			{
				sizeError = true;
				return;
			}
			if (colNum >= numColumns)
			{
				sizeError = true;
				return;
			}

			Row r = (Row)(rowData[rowNum]);

			if (colNum > r.numCols)
			{
				sizeError = true;
				return;
			}
			r.data[colNum] = text;
			rowData[rowNum] = r;
		}

		protected override string[] GetRow(int r, DataFormatter nts)
		{
			string[] rowData = new string[numColumns];

			// Create a temporary row
			Row tmpRow = (Row)base.rowData[r];

			for (int c = 0; c < numColumns; c++)
			{
				if (c < tmpRow.numCols)
					rowData[c] = tmpRow.data[c];
				else
					rowData[c] = "";
			}
			return rowData;
		}

	}
}
