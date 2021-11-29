#pragma once

#include ".\UFENumToString.h"
#include ".\UFETableData.h"

using namespace System;
using namespace System::Data;
using namespace System::IO;
using namespace System::Windows::Forms;
using namespace System::Drawing;
using namespace System::Collections;


namespace UniversalFileExplorer
{
	
	ref struct ROW
	{
		int numCols;
		array<String^>^ data;
	};


	public ref class StrTableData : public TableData
	{
	public:

		// Constructors
		StrTableData(void);
		StrTableData(int numCols);

		// Destructor
		virtual ~StrTableData(void);

		// Functions for adding rows
		void AddRow();
		void AddRow(String^ c1);
		void AddRow(String^ c1, String^ c2);
		void AddRow(String^ c1, String^ c2, String^ c3);
		void AddRow(String^ c1, String^ c2, String^ c3, String^ c4);
		void AddRow(String^ c1, String^ c2, String^ c3, String^ c4, String^ c5);

		void AddRowData(array<String^>^ rowData);

		void SetItem(int rowNum, int colNum, String^ text);

	protected:
		
		array<String^>^ GetRow(int r, NumToString^ nts) override;

	};



};