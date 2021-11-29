// UFEDataTable.h

#pragma once

#include ".\UFENumToString.h"
#include ".\UFETableData.h"

using namespace System;
using namespace System::Data;
using namespace System::IO;
using namespace System::Windows::Forms;
using namespace System::Drawing;
using namespace System::Collections;

#define DEF_COL_WIDTH	100
#define INIT_ROWS_ALLOC	10
#define TABLE_NAME		"UFE Table"

#define BLANK_STRING	""

namespace UniversalFileExplorer
{
// Alignment Constants
#define ALIGN_LEFT		1
#define ALIGN_CENTER	2
#define ALIGN_RIGHT		3
#define ALIGN_DEFAULT	ALIGN_LEFT


// DataTable Default Settings
#define DT_ROW_HEADERS_VISIBLE		false
#define DT_ALTERNATING_BACK_COLOR	SystemColors::Window
#define DT_BACK_COLOR				SystemColors::Window
#define DT_FORE_COLOR				Color::Black
#define DT_GRID_LINE_COLOR			Color::RoyalBlue
#define DT_HEADER_BACK_COLOR		Color::MidnightBlue
#define DT_HEADER_FORE_COLOR		Color::LavenderBlush
#define DT_LINK_COLOR				Color::Teal
#define DT_SELECTION_BACK_COLOR		Color::Teal
#define DT_SELECTION_FORE_COLOR		Color::PaleGreen
/*
	__gc struct COLUMN
	{
		String* name;
		int width;
		int alignment;
	};
*/
	ref struct OBJROW
	{
		int numCols;
		array<Object^>^ data;
	};

	public ref class DynTableData : public TableData
	{
	public:
		DynTableData(void);
		DynTableData(int numCols);

		// Deconstructor
		virtual ~DynTableData();

		void AddRow();
		void AddRow(Object^ c1);
		void AddRow(Object^ c1, Object^ c2);
		void AddRow(Object^ c1, Object^ c2, Object^ c3);
		void AddRow(Object^ c1, Object^ c2, Object^ c3, Object^ c4);
		void AddRow(Object^ c1, Object^ c2, Object^ c3, Object^ c4, Object^ c5);

		void AddRowData(array<Object^>^ rowData);

		void SetItem(int rowNum, int colNum, String^ text);
		
	protected:
		array<String^>^ GetRow(int r, NumToString^ nts) override;
	
	private:

		
		//void TableGrow(int numRows);

	};
};
