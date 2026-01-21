#include "pch.h"
#pragma unmanaged
#include ".\QuickIdEngine.h"

#include <string.h>

QuickIdEngine::QuickIdEngine(void)
{

}

QuickIdEngine::~QuickIdEngine(void)
{

}

bool QuickIdEngine::GetFileType(const char* filePath, char* fileType)
{
	m_fileType = fileType;


	if(strcmp(m_fileType, "FT_UNKNOWN") == 0)
		return false;
	else
		return true;
}

