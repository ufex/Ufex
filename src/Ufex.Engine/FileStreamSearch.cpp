#include ".\FileStreamSearch.h"
#using <mscorlib.dll>


namespace UniversalFileExplorer
{
	FileStreamSearch::FileStreamSearch(void)
	{
		m_fileStream = nullptr;
		m_searchBytes = nullptr;
	}

	FileStreamSearch::FileStreamSearch(FileStream^ fs)
	{
		m_fileStream = fs;		
		m_searchBytes = nullptr;
	}

	FileStreamSearch::FileStreamSearch(FileStream^ fs, String^ searchText)
	{
		m_fileStream = fs;
		m_searchBytes = Encoding::ASCII->GetBytes(searchText);
	}

	FileStreamSearch::FileStreamSearch(FileStream^ fs, array<Byte>^ searchBytes)
	{
		m_fileStream = fs;
		m_searchBytes = searchBytes;
	}


	FileStreamSearch::~FileStreamSearch(void)
	{		
		m_fileStream = nullptr;
		m_searchBytes = nullptr;
	}
	
	Int64 FileStreamSearch::FindNext()
	{
		if(m_searchBytes == nullptr || m_fileStream == nullptr)
			return -1;

		m_fileStream->Position = m_currentPosition;

		while(m_fileStream->Length > m_fileStream->Position)
		{
			if(m_fileStream->ReadByte() == m_searchBytes[0])
			{
				bool foundResult = true;
				Int64 oldPosition = m_fileStream->Position;
				for(int i = 1; i < m_searchBytes->Length && foundResult; i++)
				{
					if(m_fileStream->ReadByte() != m_searchBytes[i])
						foundResult = false;
				}

				if(foundResult)
				{
					m_currentPosition = m_fileStream->Position + 1;
					return oldPosition - 1;
				}
				m_fileStream->Position = oldPosition;
			}
		}
		return -1;
	}



};