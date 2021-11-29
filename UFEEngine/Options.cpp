#include "StdAfx.h"
#include "Options.h"

namespace UniversalFileExplorer
{
	void Options::SetControls()
	{
		// Technical View Page
		 
		// Default Number Format
		String^ defNumFormat = Settings::GetSetting(L"NumFormat", L"Default");
		if(defNumFormat != nullptr)
		{
			defNumFormat = defNumFormat->ToUpper();
			if(defNumFormat->EndsWith(L"HEX"))
				cmbDefNumFormat->Text = L"Hexadecimal";
			else if(defNumFormat->Equals(L"DEC"))
				cmbDefNumFormat->Text = L"Decimal";
		}
	}
};