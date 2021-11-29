#include "StdAfx.h"
#include ".\tabledata.h"
#using <mscorlib.dll>

namespace FileTypeClass
{
	// Default Constructor
	TableData::TableData(void)
	{
		m_NumRows = 0;
		m_NumRowsAlloc = INIT_ROWS_ALLOC;		
				
		m_NumColumns = 1;
		m_Columns = gcnew array<COLUMN^>(m_NumColumns);

		m_Data = gcnew array<String^, 2>(m_NumRowsAlloc, m_NumColumns);
		return;
	}

	// Constructor, c = num columns
	TableData::TableData(int numCols)
	{
		m_NumRows = 0;
		m_NumRowsAlloc = INIT_ROWS_ALLOC;

		m_NumColumns = numCols;
		m_Columns = gcnew array<COLUMN^>(m_NumColumns);
		for(int c = 0; c < m_NumColumns; c++)
		{
			m_Columns[c] = gcnew COLUMN;		
			m_Columns[c]->name = "";
			m_Columns[c]->width = DEF_COL_WIDTH;
			m_Columns[c]->alignment = (int)HorizontalAlignment::Left;
		}

		m_Data = gcnew array<String^, 2>(m_NumRowsAlloc, m_NumColumns);
		return;
	}

	// Deconstructor
	TableData::~TableData(void)
	{
		delete m_Columns;
		delete m_Data;
	}

	void TableData::AddRow(String^ c1)
	{
		// Allocate more memory if needed
		if(m_NumRows + 1 > m_NumRowsAlloc)
			TableGrow(GROW_SIZE);

		if(m_NumColumns == 1)
		{
			m_Data[m_NumRows, 0] = c1;
		}
		else if(m_NumColumns < 1)
		{		
			if(m_NumColumns == 0)
			{
				sizeError = true;
			}
		}
		else if(m_NumColumns > 1)
		{
			m_Data[m_NumRows, 0] = c1;
			for(int c = 1; c < m_NumColumns; c++)
			{
				m_Data[m_NumRows, c] = "";
			}
		}

		m_NumRows++;
		return;
	}

	void TableData::AddRow(String^ c1, String^ c2)
	{
		if(m_NumRows + 1 > m_NumRowsAlloc)
			TableGrow(GROW_SIZE);

		// Add the new row to the table
		if(m_NumColumns == 2)
		{
			m_Data[m_NumRows, 0] = c1;
			m_Data[m_NumRows, 1] = c2;
		}
		else if(m_NumColumns < 2)
		{		
			if(m_NumColumns == 1)
			{
				m_Data[m_NumRows, 0] = c1;
			}
		}
		else if(m_NumColumns > 2)
		{
			m_Data[m_NumRows, 0] = c1;
			m_Data[m_NumRows, 1] = c2;
			for(int c = 2; c < m_NumColumns; c++)
			{
				m_Data[m_NumRows, c] = "";
			}
		}

		m_NumRows++;
		return;
	}

	void TableData::AddRow(String^ c1, String^ c2, String^ c3)
	{
		if(m_NumRows + 1 > m_NumRowsAlloc)
			TableGrow(GROW_SIZE);

		// Add the new row to the table
		if(m_NumColumns == 3)
		{
			m_Data[m_NumRows, 0] = c1;
			m_Data[m_NumRows, 1] = c2;
			m_Data[m_NumRows, 2] = c3;
		}
		else if(m_NumColumns < 3)
		{		
			sizeError = true;
			if(m_NumColumns == 1)
			{
				m_Data[m_NumRows, 0] = c1;
			}
			else if(m_NumColumns == 2)
			{
				m_Data[m_NumRows, 0] = c1;
				m_Data[m_NumRows, 1] = c2;
			}
		}
		else if(m_NumColumns > 3)
		{
			m_Data[m_NumRows, 0] = c1;
			m_Data[m_NumRows, 1] = c2;
			m_Data[m_NumRows, 2] = c3;
			for(int c = 3; c < m_NumColumns; c++)
			{
				m_Data[m_NumRows, c] = "";
			}
		}

		m_NumRows++;
		return;
	}

	void TableData::AddRow(String^ c1, String^ c2, String^ c3, String^ c4)
	{
		// Allocate more memory if needed
		if(m_NumRows + 1 > m_NumRowsAlloc)
			TableGrow(GROW_SIZE);

		// Add the new row to the table
		if(m_NumColumns == 4)
		{
			m_Data[m_NumRows, 0] = c1;
			m_Data[m_NumRows, 1] = c2;
			m_Data[m_NumRows, 2] = c3;
			m_Data[m_NumRows, 3] = c4;
		}
		else if(m_NumColumns < 4)
		{		
			sizeError = true;
			if(m_NumColumns == 1)
			{
				m_Data[m_NumRows, 0] = c1;
			}
			else if(m_NumColumns == 2)
			{
				m_Data[m_NumRows, 0] = c1;
				m_Data[m_NumRows, 1] = c2;
			}			
			else if(m_NumColumns == 3)
			{
				m_Data[m_NumRows, 0] = c1;
				m_Data[m_NumRows, 1] = c2;				
				m_Data[m_NumRows, 2] = c3;
			}
		}
		else if(m_NumColumns > 4)
		{
			m_Data[m_NumRows, 0] = c1;
			m_Data[m_NumRows, 1] = c2;
			m_Data[m_NumRows, 2] = c3;
			m_Data[m_NumRows, 3] = c4;

			for(int c = 4; c < m_NumColumns; c++)
			{
				m_Data[m_NumRows, c] = "";
			}
		}

		m_NumRows++;
		return;
	}

	void TableData::AddColumn(String^ colName)
	{
		array<COLUMN^>^ oldColumns = m_Columns;
		//
		m_Columns = gcnew array<COLUMN^>(m_NumColumns + 1);

		for(int c = 0; c < m_NumColumns; c++)
		{
			m_Columns[c] = oldColumns[c];
		}
		
		m_NumColumns++;
		SetColumn(m_NumColumns - 1, colName);
		return;
	}

	void TableData::AddColumn(String^ colName, int colWidth)		// AddColumn( String* ColumnName, int ColumnWidth );
	{
		array<COLUMN^>^ oldColumns = m_Columns;
		//
		m_Columns = gcnew array<COLUMN^>(m_NumColumns + 1);

		for(int c = 0; c < m_NumColumns; c++)
		{
			m_Columns[c] = oldColumns[c];
		}
		
		m_NumColumns++;
		SetColumn(m_NumColumns - 1, colName, colWidth);
		return;
	}

	void TableData::SetColumn(int colNum, String^ colName)
	{
		if(m_NumColumns <= colNum)
			return;

		if(m_Columns[colNum] == nullptr)
		{
			m_Columns[colNum] = gcnew COLUMN;
			m_Columns[colNum]->width = DEF_COL_WIDTH;			
			m_Columns[colNum]->alignment = (int)HorizontalAlignment::Left;
		}

		m_Columns[colNum]->name = colName;

		return;
	}
	
	void TableData::SetColumn(int colNum, String^ colName, int colWidth)
	{
		if(m_NumColumns < colNum)
			return;

		if(m_Columns[colNum] == nullptr)
		{
			m_Columns[colNum] = gcnew COLUMN;
			m_Columns[colNum]->alignment = (int)HorizontalAlignment::Left;
		}

		m_Columns[colNum]->width = colWidth;
		m_Columns[colNum]->name = colName;
		return;
	}

	void TableData::SetCapacity(int numRows)
	{
		TableGrow(numRows);
	}

	void TableData::TableGrow(int numRows)
	{
		array<String^, 2>^ tempData = m_Data;
		m_Data = gcnew array<String^, 2>(m_NumRows + numRows, m_NumColumns);
		
		// Copy old table
		for(int r = 0; r < m_NumRows; r++)
		{
			for(int c = 0; c < m_NumColumns; c++)
			{
				m_Data[r, c] = tempData[r, c];
			}
		}
		m_NumRowsAlloc = m_NumRows + numRows;
		return;
	}

	DataTable^ TableData::GetDataTable()
	{
		DataTable^ myData = gcnew DataTable();
		myData->TableName = TABLE_NAME;

		DataColumnCollection^ columns = myData->Columns;
		DataRowCollection^ rows = myData->Rows;
		
		// Add the columns to the table
		for(int c = 0; c < m_NumColumns; c++)
		{
			columns->Add(m_Columns[c]->name);
		}
		
		// Add the Data to the DataTable
		DataRow^ myRow;
		for(int r = 0; r < m_NumRows; r++)
		{
			myRow = myData->NewRow();
			for(int c = 0; c < m_NumColumns; c++)
			{
				myRow[columns[c]] = m_Data[r, c];
			}
			rows->Add(myRow);
		}


		return myData;
	}
	
	DataGridTableStyle^ TableData::GetDataGridTableStyle()
	{
		DataGridTableStyle^ dgts = gcnew DataGridTableStyle();
		dgts->MappingName = TABLE_NAME;
		
		for(int c = 0; c < m_NumColumns; c++)
		{
			DataGridColumnStyle^ dgcs = gcnew DataGridTextBoxColumn();
			dgcs->MappingName = m_Columns[c]->name;
			dgcs->HeaderText = m_Columns[c]->name;
			// Set Column Width
			dgcs->Width = m_Columns[c]->width;
			
			// Set Column Text Alignment
			if(m_Columns[c]->alignment == ALIGN_LEFT)
				dgcs->Alignment = HorizontalAlignment::Left;
			else if(m_Columns[c]->alignment == ALIGN_CENTER)
				dgcs->Alignment = HorizontalAlignment::Center;
			else if(m_Columns[c]->alignment == ALIGN_RIGHT)
				dgcs->Alignment = HorizontalAlignment::Right;

			dgts->GridColumnStyles->Add(dgcs);
		}

		return dgts;
	}


};