// FileType.cpp

#include "StdAfx.h"
#include ".\FileType.h"


namespace UniversalFileExplorer
{
	
	// Constructor
	FileType::FileType(void)
	{		
		m_debug = gcnew UFEDebug();

		m_DebugText = "";
		
		// Initialize FileCheckInfo
		m_fileCheckInfo = gcnew FileCheckInfo();

		// Create the ArrayToNum instance
		m_atn = gcnew ArrayToNum();

		// Create an instance of the NumToString Class
		m_nts = gcnew NumToString();

		// Set the number format for the NumToString
		m_nts->SetNumFormat(NumberFormat::Default);

		m_Icons = gcnew ArrayList();
		
		m_useIcons = false;
		//LoadIcons();
		
		// By default these tabs are hidden
		m_showTechnical = false;
		m_showGraphic = false;
		m_showFileCheck = false;

		m_rootTreeNode = gcnew TreeNode("ROOT");
	}

	// Deconstructor
	FileType::~FileType(void)
	{		
		m_DebugText = nullptr;

		// Free the file stream
		if(m_FileStream != nullptr)
		{
			m_FileStream->Close();
			delete m_FileStream;
			m_FileStream = nullptr;
		}

		// Delete the trees
		if(m_rootTreeNode != nullptr)
		{
			//m_rootTreeNode->Nodes->Clear();
			m_rootTreeNode = nullptr;
		}
	}
	
	void FileType::DebugOut(String^ NewText)
	{
		if(!m_DebugText->Equals(""))
			m_DebugText = String::Concat(m_DebugText, "\r\n", NewText);
		else
			m_DebugText = NewText;

		m_debug->NewInfo(NewText);
	}

	// ExceptionOut(Exception* e)
	//		Adds an exception to the DebugInfo
	void FileType::ExceptionOut(Exception^ e)
	{
		m_debug->NewException(e);
	}
	
	void FileType::ExceptionOut(Exception^ e, String^ className)
	{
		m_debug->NewException(e, className);
	}

	void FileType::ExceptionOut(Exception^ e, String^ className, String^ funcName)
	{
		m_debug->NewException(e, className, funcName);
	}
/*
	String* FileType::GetUniStr(unsigned char asciiBytes __gc[])
	{	
		int numNullChars = 0;
		Byte asciiBytes2 __gc[];
		bool moreNullChars = true;

		while(moreNullChars)
		{
			if((asciiBytes->Count - 1 - numNullChars) >= 0)
			{
				if(asciiBytes[asciiBytes->Count - 1 - numNullChars] == 0x00)
					numNullChars++;
				else
					moreNullChars = false;
			}
			else
				moreNullChars = false;
		}

		asciiBytes2 = new Byte[asciiBytes->Count - numNullChars];

		for(int i = 0; i < asciiBytes2->Count; i++)
		{
			asciiBytes2[i] = asciiBytes[i];
		}
		
		String* unicodeString = S"";

		// Create two different encodings.
		Encoding* ascii = Encoding::ASCII;
		Encoding* unicode = Encoding::Unicode;
		
		// Perform the conversion from one encoding to the other.
		Byte unicodeBytes[] = Encoding::Convert(ascii, unicode, asciiBytes2);
		asciiBytes2->Finalize();
		asciiBytes2 = NULL;
		ascii->Finalize();
		ascii = NULL;
		Char unicodeChars[] = new Char[unicode->GetCharCount(unicodeBytes, 0, unicodeBytes -> Length)];
		unicode->GetChars(unicodeBytes, 0, unicodeBytes->Length, unicodeChars, 0);
		unicodeBytes->Finalize();
		unicodeBytes = NULL;
		unicode->Finalize();
		unicode = NULL;
		unicodeString = new String(unicodeChars);
		unicodeChars->Finalize();
		unicodeChars = NULL;
		return unicodeString;
	}
	*/
	void FileType::LoadIcons()
	{
		/*
		String* iconNames[] = {S"Null.ico", S"Circle.ico", S"Square.ico",
							 S"Table.ico", S"Properties.ico",
							 S"Text.ico", S"Script.ico",
							 S"Book.ico",
							 S"Objects.ico",
							 S"FolderOpen.ico", S"FolderClosed.ico"};
		*/
		array<String^>^ iconNames = gcnew array<String^>(11);
		iconNames[0] = "Null.ico";
		iconNames[1] = "Circle.ico";
		iconNames[2] = "Square.ico";
		iconNames[3] = "Table.ico";
		iconNames[4] = "Properties.ico";
		iconNames[5] = "Text.ico";
		iconNames[6] = "Script.ico";
		iconNames[7] = "Book.ico";
		iconNames[8] = "Objects.ico";
		iconNames[9] = "FolderOpen.ico";
		iconNames[10] = "FolderClosed.ico";

		
		ResourceManager^ resourceManager = gcnew ResourceManager("FileType.ResourceFiles", Assembly::GetExecutingAssembly());
		try
		{
			for (int i = 0; i < iconNames->Length; i++) {
				AddIcon(static_cast<Icon^>(resourceManager->GetObject(iconNames[i])));
				DebugOut(iconNames[i]);
			}
		}
		catch(Exception^ e)
		{
			DebugOut(e->Message);
			m_useIcons = false;
		}
	}

	bool FileType::ProcessFile()
	{
		return true;
	}
	
	TableData^ FileType::GetData(TreeNode^ tn)
	{
		return nullptr;
	}

	QuickInfoTable^ FileType::GetQuickInfo()
	{
		return nullptr;
	}

	Image^ FileType::GetImage()
	{
		return nullptr;
	}

	String^ FileType::GetFileCreator()
	{
		return "";
	}

};