// UFEDataTable.h

#pragma once

#include ".\UFENumToString.h"

using namespace System;
using namespace System::Data;
using namespace System::IO;
using namespace System::Windows::Forms;
using namespace System::Drawing;
using namespace System::Collections;
using namespace System::Text;

#define DEF_COL_WIDTH	100
#define INIT_ROWS_ALLOC	10
#define TABLE_NAME		"UFE Table"

#define BLANK_STRING	""

namespace UniversalFileExplorer
{

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

	public enum class ColumnAlignment : int
	{
		Default =	0x0000,
		Left =		0x0001,
		Center =	0x0002,
		Right =		0x0003
	};


	value struct COLUMN
	{
		String^ name;
		int width;
		ColumnAlignment alignment;
		String^ mappingName;
		double widthPercent;
	};


	public ref class TableData abstract
	{
	public:
		TableData(void);
		TableData(int numCols);

		// Deconstructor
		virtual ~TableData();

		virtual void AddColumn(String^ colName);
		virtual void AddColumn(String^ colName, int colWidth);		
		virtual void AddColumn(String^ colName, int colWidth, ColumnAlignment colAlign);

		virtual void SetColumn(int colNum, String^ colName);
		virtual void SetColumn(int colNum, String^ colName, int colWidth);
		virtual void SetColumn(int colNum, String^ colName, int colWidth, ColumnAlignment colAlign);

		void SetCapacity(int numRows)	{ m_RowData->Capacity = numRows; };

		// Accessors
		int GetNumRows() { return m_NumRows; };
		int GetNumColumns() { return m_NumColumns; };
		bool GetIsDynamic() { return m_isDynamic; };

		// Mutators
		void SetNumColumns(int numColumns) { m_NumColumns = numColumns; };
		//void SetNumberToString(NumToString* nts);


		/*****************************************
			Functions for using with DataTable
		*****************************************/
		// Returns a DataTable* with the data from the Table
		virtual DataTable^ GetDataTable(NumToString^ nts);

		// Returns a DataGridTableStyle*
		virtual DataGridTableStyle^ GetDataGridTableStyle(int tableWidth);

		virtual void AddDataGridColumns(GridColumnStylesCollection^ dataGridCSC);

		/**************************************
			Functions for using with ListView
		**************************************/	
		virtual void GetColumnHeaderCollection(ListView^ lv);
		
		virtual void GetListViewItemCollection(ListView^ lv, NumToString^ nts);

		/***************************************
			Functions for getting a text Table
		***************************************/
		array<String^>^ GetTextTable(NumToString^ nts);

		/**************************************
		**************************************/


	protected:

		virtual array<String^>^ GetRow(int r, NumToString^ nts) = 0;

		String^ GetColumnMappingName(String^ colName, int colWidth, int colAlign);

		String^ m_TableName;

		int m_NumRows;				// Number of rows in use
		int m_NumColumns;			// Number of columns in use

		/****************************
			Table Data
		****************************/
		ArrayList^ m_Columns;		// Column Data
		ArrayList^ m_RowData;		// Row Data

		void SetIsDynamic(bool isDynamic) { m_isDynamic = isDynamic; };

		// Set to true when a size error occurs
		bool sizeError;

	private:

		bool m_isDynamic;

		/*****************************
			DataTable Style Options
		******************************/
		bool m_RowHeadersVisible;
		Color m_AlternatingBackColor;
		Color m_BackColor;
		Color m_ForeColor;
		Color m_GridLineColor;
		Color m_HeaderBackColor;
		Color m_HeaderForeColor;
		Color m_LinkColor;
		Color m_SelectionBackColor;
		Color m_SelectionForeColor;

	};
};
