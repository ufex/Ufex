using System;
using System.Drawing;
using System.Collections;
using System.Data;
using System.Windows.Forms;
using UniversalFileExplorer;
using System.Text;

namespace Ufex.API.Tables
{
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
		};

		public static int DEF_COL_WIDTH = 100;
		static int INIT_ROWS_ALLOC = 10;
		public static string TABLE_NAME = "Ufex Table";

		// DataTable Default Settings
		static bool DT_ROW_HEADERS_VISIBLE = false;
		static Color DT_ALTERNATING_BACK_COLOR = SystemColors.Window;
		static Color DT_BACK_COLOR = SystemColors.Window;
		static Color DT_FORE_COLOR = Color.Black;
		static Color DT_GRID_LINE_COLOR = Color.RoyalBlue;
		static Color DT_HEADER_BACK_COLOR = Color.MidnightBlue;
		static Color DT_HEADER_FORE_COLOR = Color.LavenderBlush;
		static Color DT_LINK_COLOR = Color.Teal;
		static Color DT_SELECTION_BACK_COLOR = Color.Teal;
		static Color DT_SELECTION_FORE_COLOR = Color.PaleGreen;

		public TableData() : this(0)
		{
		}

		public TableData(int numCols)
		{
			m_NumRows = 0;
			m_NumColumns = numCols;
			m_Columns = new ArrayList(numCols);

			for (int c = 0; c < m_NumColumns; c++)
			{
				Column newCol = new Column();
				newCol.name = "";
				newCol.width = DEF_COL_WIDTH;
				newCol.alignment = ColumnAlignment.Default;
				m_Columns.Add(newCol);
			}

			m_RowData = new ArrayList();

			m_RowHeadersVisible = DT_ROW_HEADERS_VISIBLE;
			m_AlternatingBackColor = DT_ALTERNATING_BACK_COLOR;
			m_BackColor = DT_BACK_COLOR;
			m_ForeColor = DT_FORE_COLOR;
			m_GridLineColor = DT_GRID_LINE_COLOR;
			m_HeaderBackColor = DT_HEADER_BACK_COLOR;
			m_HeaderForeColor = DT_HEADER_FORE_COLOR;
			m_LinkColor = DT_LINK_COLOR;
			m_SelectionBackColor = DT_SELECTION_BACK_COLOR;
			m_SelectionForeColor = DT_SELECTION_FORE_COLOR;

			m_TableName = null;
		}

		// Deconstructor
		~TableData()
		{
			if (m_RowData != null)
			{
				m_RowData.Clear();
				m_RowData = null;
			}
			if (m_Columns != null)
			{
				m_Columns.Clear();
				m_Columns = null;
			}
		}

		public virtual void AddColumn(String colName)
		{
			AddColumn(colName, DEF_COL_WIDTH, ColumnAlignment.Default);
		}
		public virtual void AddColumn(String colName, int colWidth)
		{
			AddColumn(colName, colWidth, ColumnAlignment.Default);
		}
		public virtual void AddColumn(String colName, int colWidth, ColumnAlignment colAlign)
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

			m_Columns.Add(newCol);
			m_NumColumns++;
		}

		public virtual void SetColumn(int colNum, String colName)
		{
			SetColumn(colNum, colName, DEF_COL_WIDTH, ColumnAlignment.Default);
		}
		public virtual void SetColumn(int colNum, String colName, int colWidth)
		{
			SetColumn(colNum, colName, colWidth, ColumnAlignment.Default);
		}

		public virtual void SetColumn(int colNum, String colName, int colWidth, ColumnAlignment colAlign)
		{
			if (colNum >= m_NumColumns)
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

			m_Columns[colNum] = newCol;
		}

		public int NumColumns
		{
			get { return m_NumColumns; }
			set { m_NumColumns = value; }
		}

		public int NumRows
		{
			get { return m_NumRows; }
		}

		public int Capacity
		{
			get { return m_RowData.Capacity; }
			set { m_RowData.Capacity = value; }
		}

		public bool IsDynamic
		{
			get { return m_isDynamic; }
			protected set { m_isDynamic = value; }
		}

		public void SetCapacity(int numRows) 
		{
			m_RowData.Capacity = numRows;
		}

		// Accessors
		public int GetNumRows() 
		{
			return m_NumRows;
		}
		// @deprecated
		public int GetNumColumns() 
		{
			return m_NumColumns;
		}
		// @deprecated
		public bool GetIsDynamic() 
		{
			return m_isDynamic;
		}

		// Mutators

		// @deprecated
		public void SetNumColumns(int numColumns) {
			m_NumColumns = numColumns;
		}
		//void SetNumberToString(NumToString* nts);


		/*****************************************
			Functions for using with DataTable
		*****************************************/
		// Returns a DataTable with the data from the Table
		public virtual DataTable GetDataTable(DataFormatter nts)
		{
			DataTable myData = new DataTable();
			myData.TableName = TABLE_NAME;

			DataColumnCollection columns = myData.Columns;
			DataRowCollection rows = myData.Rows;

			// Add the columns to the table
			for (int c = 0; c < m_NumColumns; c++)
			{
				Column tmpCol = (Column)m_Columns[c];
				DataColumn newCol = new DataColumn(tmpCol.name);
				newCol.ColumnMapping = System.Data.MappingType.Attribute;
				columns.Add(newCol);
			}

			// Add the Data to the DataTable
			DataRow myRow;
			string[] tmpRow;
			for (int r = 0; r < m_NumRows; r++)
			{
				myRow = myData.NewRow();
				tmpRow = GetRow(r, nts);
				for (int c = 0; c < m_NumColumns; c++)
				{
					myRow[columns[c]] = tmpRow[c];
				}

				rows.Add(myRow);
			}

			return myData;
		}

		// Returns a DataGridTableStyle*
		public virtual DataGridTableStyle GetDataGridTableStyle(int tableWidth)
		{
			DataGridTableStyle dgts = new DataGridTableStyle();
			dgts.MappingName = TABLE_NAME;
			dgts.RowHeadersVisible = m_RowHeadersVisible;
			dgts.AlternatingBackColor = m_AlternatingBackColor;
			dgts.BackColor = m_BackColor;
			dgts.ForeColor = m_ForeColor;
			dgts.GridLineColor = m_GridLineColor;
			dgts.HeaderBackColor = m_HeaderBackColor;
			dgts.HeaderForeColor = m_HeaderForeColor;
			dgts.LinkColor = m_LinkColor;
			dgts.SelectionBackColor = m_SelectionBackColor;
			dgts.SelectionForeColor = m_SelectionForeColor;

			// Calculate the actual column widths
			int totalWidth = 0;
			for (int c = 0; c < m_NumColumns; c++)
				totalWidth += ((Column)m_Columns[c]).width;

			for (int c = 0; c < m_NumColumns; c++)
			{
				Column myCol = (Column)m_Columns[c];
				myCol.widthPercent = (double)myCol.width / (double)totalWidth;
			}

			for (int c = 0; c < m_NumColumns; c++)
			{
				DataGridColumnStyle dgcs = new DataGridTextBoxColumn();
				Column myCol = (Column)m_Columns[c];

				dgcs.MappingName = myCol.name;
				dgcs.HeaderText = myCol.name;

				// Set Column Width
				dgcs.Width = (int)(myCol.widthPercent * tableWidth);

				// Set Column Text Alignment
				if (myCol.alignment == ColumnAlignment.Left)
					dgcs.Alignment = HorizontalAlignment.Left;
				else if (myCol.alignment == ColumnAlignment.Center)
					dgcs.Alignment = HorizontalAlignment.Center;
				else if (myCol.alignment == ColumnAlignment.Right)
					dgcs.Alignment = HorizontalAlignment.Right;
				else
					dgcs.Alignment = HorizontalAlignment.Left;

				dgts.GridColumnStyles.Add(dgcs);
				dgcs = null;
			}

			return dgts;
		}

		public virtual void AddDataGridColumns(GridColumnStylesCollection dataGridCSC)
		{
			for (int c = 0; c < m_NumColumns; c++)
			{
				Column myCol = (Column)m_Columns[c];
				if (!dataGridCSC.Contains(myCol.name))
				{
					DataGridColumnStyle dgcs = new DataGridTextBoxColumn();
					dgcs.MappingName = myCol.name;
					dgcs.HeaderText = myCol.name;

					// Set Column Width
					dgcs.Width = myCol.width;

					// Set Column Text Alignment
					if (myCol.alignment == ColumnAlignment.Left)
						dgcs.Alignment = HorizontalAlignment.Left;
					else if (myCol.alignment == ColumnAlignment.Center)
						dgcs.Alignment = HorizontalAlignment.Center;
					else if (myCol.alignment == ColumnAlignment.Right)
						dgcs.Alignment = HorizontalAlignment.Right;
					else
						dgcs.Alignment = HorizontalAlignment.Left;

					dataGridCSC.Add(dgcs);
				}
			}
		}

		/**************************************
			Functions for using with ListView
		**************************************/
		public virtual void GetColumnHeaderCollection(ListView lv)
		{
			lv.Columns.Clear();

			for (int c = 0; c < m_NumColumns; c++)
			{
				ColumnHeader ch = new ColumnHeader();
				Column tmpCol = (Column)m_Columns[c];

				ch.Text = tmpCol.name;
				ch.Width = tmpCol.width;

				if (tmpCol.alignment == ColumnAlignment.Left)
					ch.TextAlign = HorizontalAlignment.Left;
				else if (tmpCol.alignment == ColumnAlignment.Center)
					ch.TextAlign = HorizontalAlignment.Center;
				else if (tmpCol.alignment == ColumnAlignment.Right)
					ch.TextAlign = HorizontalAlignment.Right;
				else
					ch.TextAlign = HorizontalAlignment.Left;

				lv.Columns.Add(ch);
			}

			return;
		}

		public virtual void GetListViewItemCollection(ListView lv, DataFormatter nts)
		{
			lv.Items.Clear();
			for (int r = 0; r < m_NumRows; r++)
			{
				ListViewItem lvItem = new ListViewItem(GetRow(r, nts));
				lv.Items.Add(lvItem);
			}
			return;
		}

		/***************************************
			Functions for getting a text Table
		***************************************/
		public string[] GetTextTable(DataFormatter nts)
		{
			string[] rows = new string[m_NumRows + 4];
			UInt16[] columnWidths = new UInt16[m_NumColumns];
			UInt16 totalWidth = 0;

			// Calculate the column widths in number of chars
			for (int i = 0; i < m_NumColumns; i++)
			{
				Column tmpCol = (Column)m_Columns[i];
				columnWidths[i] = (ushort)(tmpCol.width / 5);
				totalWidth += columnWidths[i];
				totalWidth++;
			}

			StringBuilder columnHeader = new StringBuilder("", totalWidth);

			for (int i = 0; i < m_NumColumns; i++)
			{
				Column tmpCol = (Column)m_Columns[i];
				columnHeader.Append(tmpCol.name.PadRight(columnWidths[i], ' '));
				columnHeader.Append("|");
			}

			rows[1] = columnHeader.ToString();

			StringBuilder columnHeader2 = new StringBuilder("", totalWidth);
			for (int i = 0; i < m_NumColumns; i++)
			{
				for (int c = 0; c < columnWidths[i]; c++)
					columnHeader2.Append("-");

				columnHeader2.Append("+");
			}

			rows[0] = columnHeader2.ToString();
			rows[2] = columnHeader2.ToString();
			rows[m_NumRows + 4 - 1] = columnHeader2.ToString();

			// Add the Data to the Text
			string[] tmpRow;
			for (int r = 0; r < m_NumRows; r++)
			{
				tmpRow = GetRow(r, nts);
				StringBuilder rowText = new StringBuilder("", totalWidth);

				for (int c = 0; c < m_NumColumns; c++)
				{
					rowText.Append(tmpRow[c].PadRight(columnWidths[c], ' '));
					rowText.Append("|");
				}
				rows[r + 3] = rowText.ToString();
			}

			return rows;
		}

		/**************************************
		**************************************/

		protected abstract string[] GetRow(int r, DataFormatter nts);

		protected String GetColumnMappingName(String colName, int colWidth, int colAlign)
		{
			return String.Format("N{0}W{1}A{2}", colName, colWidth, colAlign);
		}

		protected String m_TableName;

		protected int m_NumRows;              // Number of rows in use
		protected int m_NumColumns;           // Number of columns in use

		/****************************
			Table Data
		****************************/
		protected ArrayList m_Columns;		// Column Data
		protected ArrayList m_RowData;		// Row Data

		// Set to true when a size error occurs
		protected bool sizeError;

		private bool m_isDynamic;

		/*****************************
			DataTable Style Options
		******************************/
		private bool m_RowHeadersVisible;
		private Color m_AlternatingBackColor;
		private Color m_BackColor;
		private Color m_ForeColor;
		private Color m_GridLineColor;
		private Color m_HeaderBackColor;
		private Color m_HeaderForeColor;
		private Color m_LinkColor;
		private Color m_SelectionBackColor;
		private Color m_SelectionForeColor;
	}
}
