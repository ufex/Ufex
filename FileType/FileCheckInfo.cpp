#include "StdAfx.h"
#include ".\filecheckinfo.h"

namespace UniversalFileExplorer
{
	// Constructor
	FileCheckInfo::FileCheckInfo(void)
	{
		// Initialize ArrayList
		m_FileCheckData = gcnew ArrayList(1);
	}

	// Destructor
	FileCheckInfo::~FileCheckInfo(void)
	{
		m_FileCheckData = nullptr;
	}

	// Add a new Message
	void FileCheckInfo::NewMessage(String^ message)
	{
		m_FileCheckData->Add(message);
	}

	// Add a new Warning
	void FileCheckInfo::NewWarning(String^ message)
	{
		m_FileCheckData->Add(String::Concat("Warning: ", message));
		m_NumWarnings++;
	}

	// Add a new Error
	void FileCheckInfo::NewError(String^ message)
	{
		m_FileCheckData->Add(String::Concat("Error: ", message));	
		m_NumErrors++;
	}

	// Returns an array of strings containing the FileCheckInfo
	array<String^>^ FileCheckInfo::GetInfo()
	{
		array<String^>^ info = gcnew array<String^>(m_FileCheckData->Count + 1);
		for(int i = 0; i < m_FileCheckData->Count; i++)
		{
			info[i] = static_cast<String^>(m_FileCheckData[i]);
		}

		// Add a summary line 
		info[m_FileCheckData->Count] = String::Concat(m_NumErrors.ToString(), " error(s), ", m_NumWarnings.ToString(), " warning(s)"); 
		return info;
	}
};