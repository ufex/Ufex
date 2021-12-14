using System;
using System.Collections;

namespace Ufex.API.Tables
{
	public class DynamicTableData : Ufex.API.Tables.TableData
	{
		struct ObjRow
		{
			public int numCols;
			public Object[] data;
		}

		public DynamicTableData()
		{
			IsDynamic = true;
		}

		public DynamicTableData(int numCols)
		{
			m_NumColumns = numCols;
			m_Columns = new ArrayList(numCols);

			IsDynamic = true;

			for (int c = 0; c < m_NumColumns; c++)
			{
				TableData.Column newCol = new TableData.Column();
				newCol.name = "";
				newCol.width = TableData.DEF_COL_WIDTH;
				newCol.alignment = ColumnAlignment.Default;
				m_Columns.Add(newCol);
			}
			return;
		}

		public void AddRow()
		{
			AddRowData(new object[] { });
		}

		public void AddRow(Object c1)
		{
			AddRowData(new object[] { c1 });
		}

		public void AddRow(Object c1, Object c2)
		{
			AddRowData(new object[] { c1, c2 });
		}

		public void AddRow(Object c1, Object c2, Object c3)
		{
			AddRowData(new object[] { c1, c2, c3 });
		}

		public void AddRow(Object c1, Object c2, Object c3, Object c4)
		{
			AddRowData(new object[] { c1, c2, c3, c4 });
		}

		public void AddRow(Object c1, Object c2, Object c3, Object c4, Object c5)
		{
			AddRowData(new object[] { c1, c2, c3, c4, c5 });
		}

		public void AddRowData(Object[] rowData)
		{
			// Create a new ROW object
			ObjRow newRow = new ObjRow();

			// Set the number of columns
			newRow.numCols = rowData.Length;

			// Set the row text data
			newRow.data = rowData;

			// Add the row to the m_RowData ArrayList
			m_RowData.Add(newRow);

			// Increment the number of rows
			m_NumRows++;
			return;
		}

		public void SetItem(int rowNum, int colNum, String text)
		{
			if (rowNum >= m_NumRows)
			{
				sizeError = true;
				return;
			}
			if (colNum >= m_NumColumns)
			{
				sizeError = true;
				return;
			}
			ObjRow r = (ObjRow)m_RowData[rowNum];
			if (colNum > r.numCols)
			{
				sizeError = true;
				return;
			}
			r.data[colNum] = text;
			m_RowData[rowNum] = r;
		}

		protected override string[] GetRow(int r, DataFormatter nts)
		{
			string[] rowData = new string[m_NumColumns];

			// Create a temporary row
			ObjRow tmpRow = (ObjRow)m_RowData[r];

			for (int c = 0; c < m_NumColumns; c++)
			{
				if (c < tmpRow.numCols)
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
}
