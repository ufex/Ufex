#pragma once

#include ".\UFETableData.h"

namespace UniversalFileExplorer
{
	ref struct QI_ROW
	{
		array<String^>^ data;
	};

	public ref class QuickInfoTable : public TableData
	{
	public:
		QuickInfoTable(void);
		virtual ~QuickInfoTable(void);

		void AddRow(String^ property, String^ value);
	
	protected:
		array<String^>^ GetRow(int r, NumToString^ nts) override;

	private:
		static const int NUM_COLS = 2;

	};

};