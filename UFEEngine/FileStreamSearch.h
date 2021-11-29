#pragma once
#include "stdafx.h"

using namespace System;
using namespace System::IO;
using namespace System::Collections;
using namespace System::Text;


namespace UniversalFileExplorer
{
	public ref class FileStreamSearch
	{
	public:
		FileStreamSearch(void);
		FileStreamSearch(FileStream^ fs);
		FileStreamSearch(FileStream^ fs, String^ searchText);
		FileStreamSearch(FileStream^ fs, array<Byte>^ searchBytes);

		virtual ~FileStreamSearch(void);

		void NewSearch(String^ searchText) { m_searchBytes = Encoding::ASCII->GetBytes(searchText); };
		void NewSearch(array<Byte>^ searchBytes) { m_searchBytes = searchBytes; };

		Int64 FindNext();

		UInt32 GetSearchDataLen() { return m_searchBytes->Length; };


	private:
		FileStream^ m_fileStream;
		array<Byte>^ m_searchBytes;
		Int64 m_currentPosition;

	};


};