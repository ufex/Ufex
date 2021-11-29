#include "StdAfx.h"
#include ".\quickinfotable.h"
#using <mscorlib.dll>

namespace UniversalFileExplorer
{
	QuickInfoTable::QuickInfoTable(void)
	{
		AddColumn("Property", 200, ColumnAlignment::Left);
		AddColumn("Value", 200, ColumnAlignment::Left);
	}

	QuickInfoTable::~QuickInfoTable(void)
	{

	}

	void QuickInfoTable::AddRow(String^ property, String^ value)
	{
		// Create a new ROW object
		QI_ROW^ newRow = gcnew QI_ROW();

		// Set the row text data
		newRow->data = gcnew array<String^>(NUM_COLS);
		newRow->data[0] = property;
		newRow->data[1] = value;
		
		// Add the row to the m_RowData ArrayList
		m_RowData->Add(newRow);

		// Increment the number of rows
		m_NumRows++;
		return;
	}

	array<String^>^ QuickInfoTable::GetRow(int r, NumToString^ nts)
	{
		return (static_cast<QI_ROW^>(m_RowData[r]))->data;
	}
};