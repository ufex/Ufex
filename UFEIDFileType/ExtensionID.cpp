
#include "pch.h"

#include "ExtensionID.h"


namespace UniversalFileExplorer
{

	// Default Constructor
	ExtensionID::ExtensionID(void)
	{

	}

	String^ ExtensionID::GetFileType(String^ fp, FileStream^ fs)
	{
		assert(fp != nullptr);

		// Get the extension of the file
		String^ fileExt = Path::GetExtension(fp);
		
		// Make sure the file has an extension
		if(fileExt->Length == 0)
			return FileTypeUnknown;
		
		// Remove the . from the prefix of the extension
		if(fileExt->StartsWith("."))
		{
			if(fileExt->Length > 1)
				fileExt = fileExt->Substring(1);
			else
				return FileTypeUnknown;
		}

		// Convert to all upper case
		fileExt = fileExt->ToUpper();

		// Get all file types with the specified extension
		array<FILETYPE^>^ fileTypes = FileTypes->GetFileTypesByExtension(fileExt);

		// If only one file type was returned, return it
		if(fileTypes->Length == 1)
			return fileTypes[0]->id;

		return FileTypeUnknown;
	}


};