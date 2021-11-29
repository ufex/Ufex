#pragma once
#using <mscorlib.dll>

using namespace System;
using namespace System::Data;
using namespace System::IO;
using namespace System::Windows::Forms;

#define GROW_SIZE		10
#define DEF_COL_WIDTH	100
#define INIT_ROWS_ALLOC	10
#define TABLE_NAME		"UFE Table"

#define ALIGN_LEFT		1
#define ALIGN_CENTER	2
#define ALIGN_RIGHT		3

namespace FileTypeClass
{
	value struct COLUMN
	{
		String^ name;
		int width;
		int alignment;
	};

	public ref class TableData
	{
	public:
		TableData(void);
		TableData(int);
		virtual ~TableData();

		void AddRow(String^ c1);
		void AddRow(String^ c1, String^ c2);
		void AddRow(String^ c1, String^ c2, String^ c3);
		void AddRow(String^ c1, String^ c2, String^ c3, String^ c4);

		void AddColumn(String^ colName);
		void AddColumn(String^ colName, int colWidth);		

		void SetColumn(int colNum, String^ colName);
		void SetColumn(int colNum, String^ colName, int colWidth);

		void SetCapacity(int numRows);
		
		// Returns a DataTable* with the data from the Table
		DataTable^ GetDataTable();

		// Returns a DataGridTableStyle*
		DataGridTableStyle^ GetDataGridTableStyle();
			
	private:
		void TableGrow(int numRows);			

		int m_NumRows;			// Number of rows in use
		int m_NumColumns;

		int m_NumRowsAlloc;		// Number of rows allocated in memory

		array<COLUMN^>^ m_Columns;
		array<String^,2>^ m_Data;

		bool sizeError;
	};
};
