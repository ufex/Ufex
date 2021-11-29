// UFEIDFileTypeBase.h
#pragma once

using namespace System;
using namespace System::IO;

namespace UniversalFileExplorer
{

	public ref class IDFileTypeBase abstract
	{
	public:
		IDFileTypeBase(void);

		virtual String^ GetFileType(String^ fp) = 0;

		String^ m_FileType;
		String^	m_FilePath;
		String^ m_FileName;
		String^ m_FileExt;

	protected:
		FileTypeDb^ m_FTDB;


	};
};