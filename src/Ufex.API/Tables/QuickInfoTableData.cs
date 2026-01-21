using System;

namespace Ufex.API.Tables
{
	public class QuickInfoTableData : Ufex.API.Tables.TableData
	{
		struct QuickInfoRow
		{
			public string[] data;
		}

		public QuickInfoTableData()
		{
			AddColumn("Property", 200, ColumnAlignment.Left);
			AddColumn("Value", 200, ColumnAlignment.Left);
		}
		
		public void AddRow(string property, string value)
		{
			// Create a new ROW object
			QuickInfoRow newRow = new QuickInfoRow();

			// Set the row text data
			newRow.data = new string[] { property, value };

			// Add the row to the m_RowData ArrayList
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
}
