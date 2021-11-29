// DynTableData.cpp
//
//
//
//

#include "StdAfx.h"
#include ".\DynTableData.h"


namespace UniversalFileExplorer
{
	// Default Constructor
	DynTableData::DynTableData(void)
	{
		SetIsDynamic(true);
		return;
	}

	// Constructor
	DynTableData::DynTableData(int numCols)
	{
		m_NumColumns = numCols;
		m_Columns = gcnew ArrayList(numCols);
		
		SetIsDynamic(true);

		for(int c = 0; c < m_NumColumns; c++)
		{
			COLUMN^ newCol = gcnew COLUMN;
			newCol->name = BLANK_STRING;
			newCol->width = DEF_COL_WIDTH;
			newCol->alignment = ColumnAlignment::Default;
			m_Columns->Add(newCol);
		}
		return;
	}

	DynTableData::~DynTableData()
	{

	}

	// AddRow
	//
	void DynTableData::AddRow()
	{
		// Create a new ROW object
		OBJROW^ newRow = gcnew OBJROW();

		// Set the number of columns
		newRow->numCols = m_NumColumns;
		
		// Set the row text data
		newRow->data = gcnew array<Object^>(m_NumColumns);

		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void DynTableData::AddRow(Object^ c1)
	{
		// Create a new ROW object
		OBJROW^ newRow = gcnew OBJROW();

		// Set the number of columns
		newRow->numCols = 1;
		
		// Set the row text data
		newRow->data = gcnew array<Object^>(1);
		newRow->data[0] = c1;
		
		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void DynTableData::AddRow(Object^ c1, Object^ c2)
	{
		// Create a new ROW object
		OBJROW^ newRow = gcnew OBJROW();

		// Set the number of columns
		newRow->numCols = 2;
		
		// Set the row text data
		newRow->data = gcnew array<Object^>(2);
		newRow->data[0] = c1;
		newRow->data[1] = c2;

		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void DynTableData::AddRow(Object^ c1, Object^ c2, Object^ c3)
	{
		// Create a new ROW object
		OBJROW^ newRow = gcnew OBJROW();

		// Set the number of columns
		newRow->numCols = 3;
		
		// Set the row text data
		newRow->data = gcnew array<Object^>(3);
		newRow->data[0] = c1;
		newRow->data[1] = c2;
		newRow->data[2] = c3;

		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void DynTableData::AddRow(Object^ c1, Object^ c2, Object^ c3, Object^ c4)
	{
		// Create a new ROW object
		OBJROW^ newRow = gcnew OBJROW();

		// Set the number of columns
		newRow->numCols = 4;
		
		// Set the row text data
		newRow->data = gcnew array<Object^>(4);
		newRow->data[0] = c1;
		newRow->data[1] = c2;
		newRow->data[2] = c3;
		newRow->data[3] = c4;
		
		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}
	
	void DynTableData::AddRow(Object^ c1, Object^ c2, Object^ c3, Object^ c4, Object^ c5)
	{
		// Create a new ROW object
		OBJROW^ newRow = gcnew OBJROW();

		// Set the number of columns
		newRow->numCols = 5;
		
		// Set the row text data
		newRow->data = gcnew array<Object^>(5);
		newRow->data[0] = c1;
		newRow->data[1] = c2;
		newRow->data[2] = c3;
		newRow->data[3] = c4;
		newRow->data[4] = c5;
	
		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void DynTableData::AddRowData(array<Object^>^ rowData)
	{
		// Create a new ROW object
		OBJROW^ newRow = gcnew OBJROW();

		// Set the number of columns
		newRow->numCols = rowData->Length;
		
		// Set the row text data
		newRow->data = rowData;
		
		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void DynTableData::SetItem(int rowNum, int colNum, String^ text)
	{
		if(rowNum >= m_NumRows)
		{
			sizeError = true;
			return;
		}
		if(colNum >= m_NumColumns)
		{
			sizeError = true;
			return;
		}
		OBJROW^ r = static_cast<OBJROW^>(m_RowData[rowNum]);
		if(colNum > r->numCols)
		{
			sizeError = true;
			return;
		}
		r->data[colNum] = text;
		m_RowData[rowNum] = r;
	}

	array<String^>^ DynTableData::GetRow(int r, NumToString^ nts)
	{
		array<String^>^ rowData = gcnew array<String^>(m_NumColumns);
		
		// Create a temporary row
		OBJROW^ tmpRow = static_cast<OBJROW^>(m_RowData[r]);
		
		for(int c = 0; c < m_NumColumns; c++)
		{
			if(c < tmpRow->numCols)
			{
				rowData[c] = nts->GetStrObject(tmpRow->data[c]);
			}
			else
			{
				rowData[c] = BLANK_STRING;
			}
		}		
		return rowData;
	}
};