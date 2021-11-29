// UFETableData.cpp
//
//
//

#include "StdAfx.h"
#include ".\UFETableData.h"

namespace UniversalFileExplorer
{
	// Default Constructor
	TableData::TableData(void)
	{
		m_NumRows = 0;	
				
		m_NumColumns = 0;

		m_Columns = gcnew ArrayList();

		m_RowData = gcnew ArrayList();

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
		
		m_TableName = nullptr;
		return;
	}

	// Constructor
	TableData::TableData(int numCols)
	{
		m_NumRows = 0;

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
		
		m_RowData = gcnew ArrayList();
		
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
		
		m_TableName = nullptr;
		return;
	}

	TableData::~TableData()
	{
		// Delete the row data
		if(m_RowData != nullptr)
		{
			m_RowData->Clear();
			m_RowData = nullptr;
		}
		if(m_RowData != nullptr)
		{
			m_Columns->Clear();
			m_Columns = nullptr;
		}
	}

	void TableData::AddColumn(String^ colName)
	{
		AddColumn(colName, DEF_COL_WIDTH, ColumnAlignment::Default);
	}

	void TableData::AddColumn(String^ colName, int colWidth)
	{
		AddColumn(colName, colWidth, ColumnAlignment::Default);
	}
	
	void TableData::AddColumn(String^ colName, int colWidth, ColumnAlignment colAlign)
	{
		COLUMN^ newCol = gcnew COLUMN;
		
		// Set the Column Name
		if(colName != nullptr)
			newCol->name = colName;
		else
			newCol->name = BLANK_STRING;

		// Set the column width
		newCol->width = colWidth;

		// Set the column alignment
		newCol->alignment = colAlign;
		
		// Set the mapping name
		newCol->mappingName = GetColumnMappingName(newCol->name, newCol->width, (int)newCol->alignment);

		m_Columns->Add(newCol);
		m_NumColumns++;
	}

	void TableData::SetColumn(int colNum, String^ colName)
	{
		SetColumn(colNum, colName, DEF_COL_WIDTH, ColumnAlignment::Default);
	}
	
	void TableData::SetColumn(int colNum, String^ colName, int colWidth)
	{	
		SetColumn(colNum, colName, colWidth, ColumnAlignment::Default);
	}

	void TableData::SetColumn(int colNum, String^ colName, int colWidth, ColumnAlignment colAlign)
	{
		if(colNum >= m_NumColumns)
		{
			sizeError = true;
			return;
		}

		COLUMN^ newCol = gcnew COLUMN;

		// Set the Column Name
		if(colName != nullptr)
			newCol->name = colName;
		else
			newCol->name = BLANK_STRING;

		// Set the column width
		newCol->width = colWidth;

		// Set the column alignment
		newCol->alignment = colAlign;

		// Set the mapping name
		newCol->mappingName = GetColumnMappingName(newCol->name, newCol->width, (int)newCol->alignment);

		m_Columns[colNum] = newCol;
	}

	DataTable^ TableData::GetDataTable(NumToString^ nts)
	{
		DataTable^ myData = gcnew DataTable();
		myData->TableName = TABLE_NAME;

		DataColumnCollection^ columns = myData->Columns;
		DataRowCollection^ rows = myData->Rows;
		
		// Add the columns to the table
		for(int c = 0; c < m_NumColumns; c++)
		{
			COLUMN^ tmpCol = (static_cast<COLUMN^>(m_Columns[c]));
			DataColumn^ newCol = gcnew DataColumn(tmpCol->name);
			newCol->ColumnMapping = Data::MappingType::Attribute;
			columns->Add(newCol);
		}
		
		// Add the Data to the DataTable
		DataRow^ myRow;
		array<String^>^ tmpRow;
		for(int r = 0; r < m_NumRows; r++)
		{
			myRow = myData->NewRow();
			tmpRow = GetRow(r, nts);
			for(int c = 0; c < m_NumColumns; c++)
			{
				myRow[columns[c]] = tmpRow[c];
			}
			
			rows->Add(myRow);
		}


		return myData;
	}
	
	DataGridTableStyle^ TableData::GetDataGridTableStyle(int tableWidth)
	{
		DataGridTableStyle^ dgts = gcnew DataGridTableStyle();
		dgts->MappingName = TABLE_NAME;
		dgts->RowHeadersVisible = m_RowHeadersVisible;
		dgts->AlternatingBackColor = m_AlternatingBackColor;
		dgts->BackColor = m_BackColor;
		dgts->ForeColor = m_ForeColor;
		dgts->GridLineColor = m_GridLineColor;
		dgts->HeaderBackColor = m_HeaderBackColor;
		dgts->HeaderForeColor = m_HeaderForeColor;
		dgts->LinkColor = m_LinkColor;
		dgts->SelectionBackColor = m_SelectionBackColor;
		dgts->SelectionForeColor = m_SelectionForeColor;

		// Calculate the actual column widths
		int totalWidth = 0;
		for(int c = 0; c < m_NumColumns; c++)
			totalWidth += static_cast<COLUMN^>(m_Columns[c])->width;

		for(int c = 0; c < m_NumColumns; c++)
		{
			COLUMN^ myCol = static_cast<COLUMN^>(m_Columns[c]);
			myCol->widthPercent = (double)myCol->width / (double)totalWidth;
		}

		for(int c = 0; c < m_NumColumns; c++)
		{
			DataGridColumnStyle^ dgcs = gcnew DataGridTextBoxColumn();
			COLUMN^ myCol = static_cast<COLUMN^>(m_Columns[c]);

			dgcs->MappingName = myCol->name;
			dgcs->HeaderText = myCol->name;
			
			// Set Column Width
			dgcs->Width = (int)(myCol->widthPercent * tableWidth);
			
			// Set Column Text Alignment
			if(myCol->alignment == ColumnAlignment::Left)
				dgcs->Alignment = HorizontalAlignment::Left;
			else if(myCol->alignment == ColumnAlignment::Center)
				dgcs->Alignment = HorizontalAlignment::Center;
			else if(myCol->alignment == ColumnAlignment::Right)
				dgcs->Alignment = HorizontalAlignment::Right;
			else
				dgcs->Alignment = HorizontalAlignment::Left;

			dgts->GridColumnStyles->Add(dgcs);
			dgcs = nullptr;
		}

		return dgts;
	}
	
	void TableData::AddDataGridColumns(GridColumnStylesCollection^ dataGridCSC)
	{
		for(int c = 0; c < m_NumColumns; c++)
		{
			COLUMN^ myCol = static_cast<COLUMN^>(m_Columns[c]);
			if(!dataGridCSC->Contains(myCol->name))
			{
				DataGridColumnStyle^ dgcs = gcnew DataGridTextBoxColumn();		
				dgcs->MappingName = myCol->name;
				dgcs->HeaderText = myCol->name;
				
				// Set Column Width
				dgcs->Width = myCol->width;
				
				// Set Column Text Alignment
				if(myCol->alignment == ColumnAlignment::Left)
					dgcs->Alignment = HorizontalAlignment::Left;
				else if(myCol->alignment == ColumnAlignment::Center)
					dgcs->Alignment = HorizontalAlignment::Center;
				else if(myCol->alignment == ColumnAlignment::Right)
					dgcs->Alignment = HorizontalAlignment::Right;
				else
					dgcs->Alignment = HorizontalAlignment::Left;

				dataGridCSC->Add(dgcs);		
			}
		}
	}

	void TableData::GetColumnHeaderCollection(ListView^ lv)
	{
		lv->Columns->Clear();

		for(int c = 0; c < m_NumColumns; c++)
		{
			ColumnHeader^ ch = gcnew ColumnHeader();
			COLUMN^ tmpCol = static_cast<COLUMN^>(m_Columns[c]);

			ch->Text = tmpCol->name;
			ch->Width = tmpCol->width;

			if(tmpCol->alignment == ColumnAlignment::Left)
				ch->TextAlign = HorizontalAlignment::Left;
			else if(tmpCol->alignment == ColumnAlignment::Center)
				ch->TextAlign = HorizontalAlignment::Center;
			else if(tmpCol->alignment == ColumnAlignment::Right)
				ch->TextAlign = HorizontalAlignment::Right;
			else
				ch->TextAlign = HorizontalAlignment::Left;
			
			lv->Columns->Add(ch);
		}

		return;
	}
		
	void TableData::GetListViewItemCollection(ListView^ lv, NumToString^ nts)
	{
		lv->Items->Clear();
		for(int r = 0; r < m_NumRows; r++)
		{
			ListViewItem^ lvItem = gcnew ListViewItem(GetRow(r, nts));
			lv->Items->Add(lvItem);
		}
		return;
	}
	
	array<String^>^ TableData::GetTextTable(NumToString^ nts)
	{
		array<String^>^ rows = gcnew array<String^>(m_NumRows + 4);
		array<UInt16>^ columnWidths = gcnew array<UInt16>(m_NumColumns);
		UInt16 totalWidth = 0;
		
		// Calculate the column widths in number of chars
		for(int i = 0; i < m_NumColumns; i++)
		{
			COLUMN^ tmpCol = (static_cast<COLUMN^>(m_Columns[i]));
			columnWidths[i] = tmpCol->width / 5;
			totalWidth += columnWidths[i];
			totalWidth++;
		}

		StringBuilder^ columnHeader = gcnew StringBuilder("", totalWidth);

		for(int i = 0; i < m_NumColumns; i++)
		{
			COLUMN^ tmpCol = (static_cast<COLUMN^>(m_Columns[i]));
			columnHeader->Append(tmpCol->name->PadRight(columnWidths[i], ' '));
			columnHeader->Append("|");
		}
	
		rows[1] = columnHeader->ToString();

		StringBuilder^ columnHeader2 = gcnew StringBuilder("", totalWidth);
		for(int i = 0; i < m_NumColumns; i++)
		{
			for(int c = 0; c < columnWidths[i]; c++)
				columnHeader2->Append("-");

			columnHeader2->Append("+");
		}

		rows[0] = columnHeader2->ToString();
		rows[2] = columnHeader2->ToString();
		rows[m_NumRows + 4 - 1] = columnHeader2->ToString();
		
		// Add the Data to the Text
		array<String^>^ tmpRow;
		for(int r = 0; r < m_NumRows; r++)
		{
			tmpRow = GetRow(r, nts);
			StringBuilder^ rowText = gcnew StringBuilder("", totalWidth);

			for(int c = 0; c < m_NumColumns; c++)
			{
				rowText->Append(tmpRow[c]->PadRight(columnWidths[c], ' '));
				rowText->Append("|");
			}
			rows[r + 3] = rowText->ToString();
		}


		return rows;
	}

	String^ TableData::GetColumnMappingName(String^ colName, int colWidth, int colAlign)
	{
		return String::Format("N{0}W{1}A{2}", colName, colWidth, colAlign);
	}

};