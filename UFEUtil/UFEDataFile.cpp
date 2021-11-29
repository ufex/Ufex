// This is the main DLL file.

#include "stdafx.h"

#include "UFEDataFile.h"
#include "StringPack.h"

namespace UniversalFileExplorer
{
#pragma managed
	ReadUFEFile::ReadUFEFile(void)
	{
		m_FilePath = "";
		m_Header = gcnew UFEDAT_HEADER;
		m_Structures = gcnew ArrayList();
		m_Data = gcnew ArrayList();

		return;
	}
	
	ReadUFEFile::ReadUFEFile(String^ fp)		// fp is the file path (usually the data path)
	{
		m_FilePath = fp;
		m_Header = gcnew UFEDAT_HEADER;
		m_Structures = gcnew ArrayList();
		m_Data = gcnew ArrayList();
	}

	ReadUFEFile::ReadUFEFile(String^ fp, String^ fn)
	{
		m_FilePath = fp;
		m_FileName = fn;
		m_Header = gcnew UFEDAT_HEADER;
		m_Structures = gcnew ArrayList();
		m_Data = gcnew ArrayList();
	}

	// Obsolete
	array<UFE_PROPERTY^>^ ReadUFEFile::GetProperties()
	{
		Encoding^ asciiEncoding = gcnew ASCIIEncoding();
		array<UFE_PROPERTY^>^ functions = gcnew array<UFE_PROPERTY^>(79);
		FileStream^ fs = gcnew FileStream( String::Concat( m_FilePath, "\\WMF_Functions.dat" ), FileMode::Open, FileAccess::Read );
		BinaryReader^ br = gcnew BinaryReader(fs, asciiEncoding);
		for(int i = 0; i < 79; i++)
		{	
			functions[i] = gcnew UFE_PROPERTY;
			// Read the name
			String^ tmpName = "";
			__wchar_t curChar = 0xFF;
			while( curChar != 0x00 )
			{
				curChar = br->ReadChar();				
				if( curChar != 0x00 )
					tmpName = String::Concat(tmpName, gcnew String(&curChar));
			}
			functions[i]->name = tmpName;

			String^ tmpNumber = "";
			__wchar_t curDigit = 0xFF;
			while(curDigit != 0x00)
			{
				curDigit = br->ReadChar();
				if(curDigit != 0x00)
					tmpNumber = String::Concat(tmpNumber, gcnew String(&curDigit));
			}
			functions[i]->number = Convert::ToUInt16(tmpNumber);
		}
		return functions;
	}

	array<UFE_PROPERTY^>^ ReadUFEFile::GetProperties(String^ fileName)
	{
		Encoding^ asciiEncoding = Encoding::ASCII;
		m_FileStream = gcnew FileStream(String::Concat(m_FilePath, String::Concat("\\", fileName)), FileMode::Open, FileAccess::Read);
		m_FileStream->Position = 0;
		m_BinRead = gcnew BinaryReader(m_FileStream, asciiEncoding);
		m_Header = ReadHeader();
		UFEDAT_PROPFILE^ propFile = gcnew UFEDAT_PROPFILE;
		propFile->numRows = m_BinRead->ReadUInt32();
		propFile->numColumns = m_BinRead->ReadUInt32();
		propFile->colTypes = gcnew array<Byte>(propFile->numColumns);
		
		// Read in the column types
		for(UInt16 i = 0; i < propFile->numColumns; i++)
		{
			propFile->colTypes[i] = m_BinRead->ReadByte();
		}

		propFile->rowData = gcnew array<UFE_PROPERTY^>(propFile->numRows);

		StringPack* sc = new StringPack();

		for(unsigned int i = 0; i < propFile->numRows; i++)
		{	
			try
			{
				propFile->rowData[i] = gcnew UFE_PROPERTY;	
				// Read the String data
				Byte dataLen = m_BinRead->ReadByte();
				array<BYTE>^ gcData = m_BinRead->ReadBytes(dataLen);
				BYTE* nogcData = new BYTE[dataLen];
				for(int y = 0; y < dataLen; y++)
				{
					nogcData[y] = gcData[y];
				}
				char* tmpName = NULL;
				sc->Unpack(nogcData, tmpName, dataLen);
				propFile->rowData[i]->name = gcnew String(tmpName);
				// Read the number data
				propFile->rowData[i]->number = m_BinRead->ReadUInt16();
			}
			catch(IO::EndOfStreamException^)
			{
				return propFile->rowData;
			}
		}
		m_BinRead->Close();
		m_FileStream->Close();
		return propFile->rowData;
	}

	ArrayList^ ReadUFEFile::GetPropertiesCSV() 
	{
		Encoding^ asciiEncoding = gcnew ASCIIEncoding();
		ArrayList^ functions = gcnew ArrayList();
		FileStream^ fs = gcnew FileStream(m_FilePath, FileMode::Open, FileAccess::Read);
		BinaryReader^ br = gcnew BinaryReader(fs, asciiEncoding);
		bool eof = false;
		while(!eof && (fs->Position + 1) < fs->Length)
		{
			UFE_PROPERTY^ tempProp = gcnew UFE_PROPERTY;
			// Read the name
			String^ tmpName = "";
			__wchar_t curChar = 0xFF;
			while(curChar != ','&& (fs->Position + 1) < fs->Length)
			{
				curChar = br->ReadChar();				
				if(curChar != 0x22)
					tmpName = String::Concat(tmpName, gcnew String(&curChar));
			}
			tempProp->name = tmpName;

			String^ tmpNumber = "";
			__wchar_t curDigit = 0xFF;
			while(curDigit != 0x0A && (fs->Position) < fs->Length)
			{
				curDigit = br->ReadChar();
				if(curDigit != 0x0A)
					tmpNumber = String::Concat(tmpNumber, gcnew String(&curDigit));
			}
			if((fs->Position + 1) < fs->Length)
				Byte retcar = br->ReadByte();
			else
				eof = true;
			CultureInfo^ myCI = gcnew CultureInfo("en-US", false);
			NumberFormatInfo^ nfi = myCI->NumberFormat;
			tempProp->number = System::Convert::ToInt16(tmpNumber, nfi);
			functions->Add(tempProp);
		}
		br->Close();
		fs->Close();
		return functions;
	}
	
	UFEDAT_HEADER^ ReadUFEFile::ReadHeader()
	{
		UFEDAT_HEADER^ tmpHeader = gcnew UFEDAT_HEADER;
		tmpHeader->signature1 = m_BinRead->ReadBytes(4);
		tmpHeader->signature2 = m_BinRead->ReadBytes(4);
		tmpHeader->signature3 = m_BinRead->ReadBytes(4);
		tmpHeader->signature4 = m_BinRead->ReadBytes(4);
		tmpHeader->dataFileType = m_BinRead->ReadUInt16();
		tmpHeader->dataFileID = m_BinRead->ReadUInt16();
		tmpHeader->fileVersion = m_BinRead->ReadUInt16();
		tmpHeader->progVersMaj = m_BinRead->ReadUInt16();
		tmpHeader->progVersMin = m_BinRead->ReadUInt16();
		tmpHeader->progVersBuild = m_BinRead->ReadUInt16();
		tmpHeader->progVersRev = m_BinRead->ReadUInt16();
		tmpHeader->flags = m_BinRead->ReadUInt16();
		tmpHeader->fileSize = m_BinRead->ReadUInt64();
		tmpHeader->stringKey = m_BinRead->ReadUInt64();
		tmpHeader->numberKey = m_BinRead->ReadUInt64();
		tmpHeader->reserved = m_BinRead->ReadUInt32();
		tmpHeader->checksum = m_BinRead->ReadUInt32();
		return tmpHeader;
	}


	WriteUFEFile::WriteUFEFile()
	{


	}

	
	bool WriteUFEFile::ConvertCSV2UFE(String^ sourceCSV, String^ destinationUFE)
	{
		//FileStream* fsIN = new FileStream(sourceCSV, IO::FileMode::Open, IO::FileAccess::Read);
		m_FileStream = gcnew FileStream(destinationUFE, IO::FileMode::OpenOrCreate, IO::FileAccess::Write);
		bw = gcnew BinaryWriter(m_FileStream);

		UInt64 fileSize = 64;
		ReadUFEFile^ csvData = gcnew ReadUFEFile(sourceCSV);
		ArrayList^ props = csvData->GetPropertiesCSV();
		UFEDAT_PROPFILE^ myFile = gcnew UFEDAT_PROPFILE;
		myFile->numRows = props->Count;
		myFile->numColumns = 2;
		myFile->rowData = gcnew array<UFE_PROPERTY^>(myFile->numRows);

		// Transfer data from ArrayList of Properties to Array of UFE_PROPERTY* objects
		for(int i = 0; i < props->Count; i++)
		{
			myFile->rowData[i] = static_cast<UFE_PROPERTY^>(props[i]);
		}
		UFEDAT_HEADER^ header = gcnew UFEDAT_HEADER;

		bool retval = WriteHeader(header);
		retval = WriteProperties(myFile);
		header->signature1 = SIGN1;
		header->signature2 = SIGN2;
		header->signature3 = SIGN3;
		header->signature4 = SIGN4;
		header->fileSize = m_FileStream->Length;
		header->flags = 0x0000;
		header->reserved = 0x00000000;
		retval = WriteHeader(header);

		m_FileStream->Close();
		bw = nullptr;
		return true;
	}
	
	bool WriteUFEFile::WriteHeader(UFEDAT_HEADER^ header)
	{
		m_FileStream->Position = 0;

		// Write the header
		bw->Write(header->signature1);
		bw->Write(header->signature2);
		bw->Write(header->signature3);
		bw->Write(header->signature4);
		bw->Write(header->dataFileType);
		bw->Write(header->dataFileID);
		bw->Write(header->fileVersion);
		bw->Write(header->progVersMaj);
		bw->Write(header->progVersMin);
		bw->Write(header->progVersBuild);
		bw->Write(header->progVersRev);
		bw->Write(header->flags);
		bw->Write(header->fileSize);
		bw->Write(header->stringKey);
		bw->Write(header->numberKey);
		bw->Write(header->reserved);
		bw->Write(header->checksum);
		return true;
	}

	bool WriteUFEFile::WriteProperties(UFEDAT_PROPFILE^ propFile)
	{
		m_FileStream->Position = 64;
		
		bw->Write(propFile->numRows);
		bw->Write(propFile->numColumns);
		Byte x = UFEDATTYPE_STRING;
		bw->Write(x);
		x = UFEDATTYPE_UINT16;
		bw->Write(x);

//		Byte space = 0x00;
		StringPack* sc = new StringPack;
		for(int i = 0; i < propFile->rowData->Length; i++)
		{
			array<Byte>^ encName;
			int tmpStrLen = propFile->rowData[i]->name->Length;
			char* tmpName = new char[tmpStrLen];
			for(int y = 0; y < tmpStrLen; y++)
			{
				tmpName[y] = (char)propFile->rowData[i]->name[y];
			}
			//sc->Pack(tmpName, encName, propFile->rowData[i]->name->get_Length());
			Byte dataLen = encName->Length;
			bw->Write(dataLen);
			bw->Write(encName);
			bw->Write(propFile->rowData[i]->number);
		}
		return true;
	}
			
	bool WriteUFEFile::NewUFEFile(String^ configFile)
	{
		CONFIG_FILE^ myConfig = ReadConfigFile(configFile);
		
		return false;
	}

	CONFIG_FILE^ WriteUFEFile::ReadConfigFile(String^ configFile)
	{
		CONFIG_FILE^ myConfig = gcnew CONFIG_FILE;

		return myConfig;
	}

/*
	String* StringPack::GetUniStr(unsigned char asciiBytes __gc[])
	{	
		int numNullChars = 0;
		Byte asciiBytes2 __gc[];
		bool moreNullChars = true;

		while (moreNullChars)
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
		moreNullChars = NULL;

		asciiBytes2 = new Byte[asciiBytes->Count - numNullChars];
		numNullChars = NULL;
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
		unicodeString = new String( unicodeChars );
		unicodeChars->Finalize();
		unicodeChars = NULL;
		return unicodeString;
	}
*/
};