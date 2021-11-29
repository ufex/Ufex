// StrTableData.cpp
//
//
// StrTableData is a class for static string table data
//
//

#include "StdAfx.h"
#include ".\StrTableData.h"
#using <mscorlib.dll>


namespace UniversalFileExplorer
{
	StrTableData::StrTableData(void)
	{
		SetIsDynamic(false);
	}
	
	StrTableData::StrTableData(int numCols)
	{
		SetIsDynamic(false);

		m_NumColumns = numCols;
		m_Columns = gcnew ArrayList(numCols);
		for(int c = 0; c < m_NumColumns; c++)
		{
			COLUMN^ newCol = gcnew COLUMN;
			newCol->name = BLANK_STRING;
			newCol->width = DEF_COL_WIDTH;
			newCol->alignment = ColumnAlignment::Default;
			m_Columns->Add(newCol);
		}
	}


	StrTableData::~StrTableData(void)
	{
	}

	void StrTableData::AddRow()
	{
		// Create a new ROW object
		ROW^ newRow = gcnew ROW();

		// Set the number of columns
		newRow->numCols = m_NumColumns;
		
		// Set the row text data
		newRow->data = gcnew array<String^>(m_NumColumns);

		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void StrTableData::AddRow(String^ c1)
	{
		// Create a new ROW object
		ROW^ newRow = gcnew ROW();

		// Set the number of columns
		newRow->numCols = 1;
		
		// Set the row text data
		newRow->data = gcnew array<String^>(1);
		newRow->data[0] = c1;
		
		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void StrTableData::AddRow(String^ c1, String^ c2)
	{
		// Create a new ROW object
		ROW^ newRow = gcnew ROW();

		// Set the number of columns
		newRow->numCols = 2;
		
		// Set the row text data
		newRow->data = gcnew array<String^>(2);
		newRow->data[0] = c1;
		newRow->data[1] = c2;

		
		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void StrTableData::AddRow(String^ c1, String^ c2, String^ c3)
	{
		// Create a new ROW object
		ROW^ newRow = gcnew ROW();

		// Set the number of columns
		newRow->numCols = 3;
		
		// Set the row text data
		newRow->data = gcnew array<String^>(3);
		newRow->data[0] = c1;
		newRow->data[1] = c2;
		newRow->data[2] = c3;

		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	void StrTableData::AddRow(String^ c1, String^ c2, String^ c3, String^ c4)
	{
		// Create a new ROW object
		ROW^ newRow = gcnew ROW();

		// Set the number of columns
		newRow->numCols = 4;
		
		// Set the row text data
		newRow->data = gcnew array<String^>(4);
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
	
	void StrTableData::AddRow(String^ c1, String^ c2, String^ c3, String^ c4, String^ c5)
	{
		// Create a new ROW object
		ROW^ newRow = gcnew ROW();

		// Set the number of columns
		newRow->numCols = 5;
		
		// Set the row text data
		newRow->data = gcnew array<String^>(5);
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

	void StrTableData::AddRowData(array<String^>^ rowData)
	{
		// Create a new ROW object
		ROW^ newRow = gcnew ROW();

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

	void StrTableData::SetItem(int rowNum, int colNum, String^ text)
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
		
		ROW^ r = static_cast<ROW^>(m_RowData[rowNum]);

		if(colNum > r->numCols)
		{
			sizeError = true;
			return;
		}
		r->data[colNum] = text;
		m_RowData[rowNum] = r;
	}

	array<String^>^ StrTableData::GetRow(int r, NumToString^ nts)
	{
		array<String^>^ rowData = gcnew array<String^>(m_NumColumns);
		
		// Create a temporary row
		ROW^ tmpRow = static_cast<ROW^>(m_RowData[r]);
		
		for(int c = 0; c < m_NumColumns; c++)
		{
			if(c < tmpRow->numCols)
				rowData[c] = tmpRow->data[c];
			else
				rowData[c] = BLANK_STRING;
		}		
		return rowData;
	}


};
