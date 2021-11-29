// This is the main DLL file.

#include "pch.h"

#include "UFEIDFileTypeBase.h"

namespace UniversalFileExplorer
{
	// Default Constructor
	IDFileTypeBase::IDFileTypeBase()
	{
		m_FileType = "";
		m_FilePath = "";
		m_FileName = "";
		m_FileExt = "";
	}
	
};