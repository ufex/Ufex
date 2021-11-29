#include "StdAfx.h"
#include ".\GUIUtil.h"
#using <mscorlib.dll>

#ifdef SizeF
#	undef SizeF
#endif

#ifdef MeasureString
#	undef MeasureString
#endif

namespace UniversalFileExplorer
{

	void GUIUtil::ResizeColumnsToFit(DataGrid^ dataGrid)
	{
		int nRowsToScan = 200;

		// Create graphics object for measuring widths.
		Graphics^ g = dataGrid->CreateGraphics();

		// Define new table style.
		DataGridTableStyle^ tableStyle = dataGrid->TableStyles[0];

		try
		{
			DataTable^ dataTable = static_cast<DataTable^>(dataGrid->DataSource);
	  
			if(dataTable == nullptr)
				return;

			if (nRowsToScan == -1)
				nRowsToScan = dataTable->Rows->Count;
			else
				nRowsToScan = System::Math::Min(nRowsToScan, dataTable->Rows->Count);

			// Now create the column styles within the table style.
			DataGridTextBoxColumn^ columnStyle;
			int iWidth;
			for (int iCurrCol = 0; iCurrCol < dataTable->Columns->Count; iCurrCol++)
			{
				DataColumn^ dataColumn = dataTable->Columns[iCurrCol];
	    
				columnStyle = static_cast<DataGridTextBoxColumn^>(tableStyle->GridColumnStyles[dataColumn->ColumnName]);


				// Set width to header text width.
				//System::Drawing::SizeF strSize = g->MeasureString(columnStyle->HeaderText, dataGrid->Font);
				iWidth = (int)(g->MeasureString(columnStyle->HeaderText, dataGrid->Font).Width);

				// Change width, if data width is wider than header text width.
				// Check the width of the data in the first X rows.
				DataRow^ dataRow;
				for (int iRow = 0; iRow < nRowsToScan; iRow++)
				{
					dataRow = dataTable->Rows[iRow];
					if (dataRow[dataColumn->ColumnName] != nullptr)
					{
						int iColWidth = (int)(g->MeasureString(dataRow->ItemArray[iCurrCol]->ToString(), dataGrid->Font).Width);
						iWidth = (int)System::Math::Max(iWidth, iColWidth);
					}
				}
				columnStyle->Width = iWidth + 4;
			}    
		}
		catch(Exception^)
		{
			throw;
		}
	}
};
