using System;
using System.Collections;
using System.Data;
using System.Text;

namespace Ufex.API.Tables;

public enum ColumnAlignment : int
{
	Default = 0x0000,
	Left = 0x0001,
	Center = 0x0002,
	Right = 0x0003
}

public abstract class TableData
{
	public struct Column
	{
		public string name;
		public int width;
		public ColumnAlignment alignment;
		public string mappingName;
		public double widthPercent;
	}

	public static int DEF_COL_WIDTH = 100;

	private int _numRows;              // Number of rows in use
	private int _numColumns;           // Number of columns in use

	/****************************
		Table Data
	****************************/
	protected ArrayList columns;        // Columns
	protected ArrayList rowData;        // Row Data

	// Set to true when a size error occurs
	protected bool sizeError;

	private bool _isDynamic;

	public int NumColumns
	{
		get { return _numColumns; }
		set { _numColumns = value; }
	}

	public int NumRows
	{
		get { return _numRows; }
		protected set { _numRows = value; }
	}

	public int Capacity
	{
		get { return rowData.Capacity; }
		set { rowData.Capacity = value; }
	}

	public bool IsDynamic { get; protected set; }

	public string TemplateName { get; protected set; } = string.Empty;

	public TableData() : this(0)
	{
	}

	public TableData(int numCols)
	{
		_numRows = 0;
		_numColumns = numCols;
		columns = new ArrayList(numCols);

		for(int c = 0; c < _numColumns; c++)
		{
			Column newCol = new Column();
			newCol.name = "";
			newCol.width = DEF_COL_WIDTH;
			newCol.alignment = ColumnAlignment.Default;
			columns.Add(newCol);
		}

		rowData = new ArrayList();
	}

	public virtual void AddColumn(string colName)
	{
		AddColumn(colName, DEF_COL_WIDTH, ColumnAlignment.Default);
	}
	
	public virtual void AddColumn(string colName, int colWidth)
	{
		AddColumn(colName, colWidth, ColumnAlignment.Default);
	}

	public virtual void AddColumn(string colName, int colWidth, ColumnAlignment colAlign)
	{
		Column newCol = new Column();

		// Set the Column Name
		if (colName != null)
			newCol.name = colName;
		else
			newCol.name = "";

		// Set the column width
		newCol.width = colWidth;

		// Set the column alignment
		newCol.alignment = colAlign;

		// Set the mapping name
		newCol.mappingName = GetColumnMappingName(newCol.name, newCol.width, (int)newCol.alignment);

		columns.Add(newCol);
		_numColumns++;
	}

	public virtual void SetColumn(int colNum, string colName)
	{
		SetColumn(colNum, colName, DEF_COL_WIDTH, ColumnAlignment.Default);
	}
	public virtual void SetColumn(int colNum, string colName, int colWidth)
	{
		SetColumn(colNum, colName, colWidth, ColumnAlignment.Default);
	}

	public virtual void SetColumn(int colNum, string colName, int colWidth, ColumnAlignment colAlign)
	{
		if (colNum >= _numColumns)
		{
			sizeError = true;
			return;
		}

		Column newCol = new Column();

		// Set the Column Name
		newCol.name = colName != null ? colName : "";

		// Set the column width
		newCol.width = colWidth;

		// Set the column alignment
		newCol.alignment = colAlign;

		// Set the mapping name
		newCol.mappingName = GetColumnMappingName(newCol.name, newCol.width, (int)newCol.alignment);

		columns[colNum] = newCol;
	}


	/*****************************************
		Functions for using with DataTable
	*****************************************/
	// Returns a DataTable with the data from the Table
	public virtual DataTable GetDataTable(DataFormatter df)
	{
		DataTable myData = new DataTable();
		//myData.TableName = TABLE_NAME;

		DataColumnCollection columns = myData.Columns;
		DataRowCollection rows = myData.Rows;

		// Add the columns to the table
		for(int c = 0; c < _numColumns; c++)
		{
			Column tmpCol = (Column)this.columns[c];
			DataColumn newCol = new DataColumn(tmpCol.name);
			newCol.ColumnMapping = System.Data.MappingType.Attribute;
			columns.Add(newCol);
		}

		// Add the Data to the DataTable
		DataRow myRow;
		string[] tmpRow;
		for(int r = 0; r < _numRows; r++)
		{
			myRow = myData.NewRow();
			tmpRow = GetRow(r, df);
			for (int c = 0; c < _numColumns; c++)
			{
				myRow[columns[c]] = tmpRow[c];
			}

			rows.Add(myRow);
		}

		return myData;
	}

	/***************************************
		Functions for getting a text Table
	***************************************/

	/// <summary>
	/// Gets the table data as an array of strings formatted as a text table (suitable for CLI output).
	/// </summary>
	/// <param name="df"></param>
	/// <returns></returns>
	public string[] GetTextTable(DataFormatter df)
	{
		string[] rows = new string[_numRows + 4];
		UInt16[] columnWidths = new UInt16[_numColumns];
		UInt16 totalWidth = 0;

		// Calculate the column widths in number of chars
		for (int i = 0; i < _numColumns; i++)
		{
			Column tmpCol = (Column)columns[i];
			columnWidths[i] = (ushort)(tmpCol.width / 5);
			totalWidth += columnWidths[i];
			totalWidth++;
		}

		StringBuilder columnHeader = new StringBuilder("", totalWidth);

		for (int i = 0; i < _numColumns; i++)
		{
			Column tmpCol = (Column)columns[i];
			columnHeader.Append(tmpCol.name.PadRight(columnWidths[i], ' '));
			columnHeader.Append("|");
		}

		rows[1] = columnHeader.ToString();

		StringBuilder columnHeader2 = new StringBuilder("", totalWidth);
		for (int i = 0; i < _numColumns; i++)
		{
			for (int c = 0; c < columnWidths[i]; c++)
				columnHeader2.Append("-");

			columnHeader2.Append("+");
		}

		rows[0] = columnHeader2.ToString();
		rows[2] = columnHeader2.ToString();
		rows[_numRows + 4 - 1] = columnHeader2.ToString();

		// Add the Data to the Text
		string[] tmpRow;
		for (int r = 0; r < _numRows; r++)
		{
			tmpRow = GetRow(r, df);
			StringBuilder rowText = new StringBuilder("", totalWidth);

			for (int c = 0; c < _numColumns; c++)
			{
				rowText.Append(tmpRow[c].PadRight(columnWidths[c], ' '));
				rowText.Append("|");
			}
			rows[r + 3] = rowText.ToString();
		}

		return rows;
	}

	protected abstract string[] GetRow(int r, DataFormatter df);

	protected string GetColumnMappingName(string colName, int colWidth, int colAlign)
	{
		return String.Format("N{0}W{1}A{2}", colName, colWidth, colAlign);
	}
}
