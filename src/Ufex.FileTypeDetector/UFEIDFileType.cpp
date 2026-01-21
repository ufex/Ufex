#include "pch.h"

#include "UFEIDFileType.h"

// Contains the definitions for the file types
#include "..\FileTypeManager\UFEFileTypeDefs.h"

namespace UniversalFileExplorer
{

	// Default Constructor
	IDFileType::IDFileType(void)
	{

	}

	String^ IDFileType::GetFileType(String^ filePath, FileStream^ fileStream)
	{
		m_fileStream = fileStream;
		m_filePath = filePath;
		m_fileType = FileTypeUnknown;

		fileSize = m_fileStream->Length;
		
		// Handle 0 byte file
		if(fileSize <= 0)
			return m_fileType;

		m_binaryReader = gcnew BinaryReader(m_fileStream);

		// Read in the first 32 bytes of the file
		if(fileSize < NUM_READ_BYTES)
			head8 = m_binaryReader->ReadBytes(static_cast<int>(fileSize));
		else
			head8 = m_binaryReader->ReadBytes(NUM_READ_BYTES);
		
		// Put the bytes into the WORD header
		head16 = gcnew array<UInt16>(head8->Length / 2);
		for(int i = 0; i < head16->Length; i++)
			head16[i] = (head8[i*2] * 0x0100) + head8[(i*2)+1];
		
		// Put the bytes into the DWORD header
		head32 = gcnew array<UInt32>(head8->Length / 4);
		for(int i = 0; i < head32->Length; i++)
			head32[i] = (head16[i*2] * 0x00010000) + head16[(i*2)+1];
		
		m_fileType = FileTypeUnknown;		


		// Try to identify the file type quicker by sorting out the common extensions

		if(IdGRPH())
			return m_fileType;
		else if(IdDOCS())
			return m_fileType;
		else if(IdEXEC())
			return m_fileType;
		else if(IdAPPS())
			return m_fileType;
		else if(IdARCH())
			return m_fileType;
		else if(IdAUDI())
			return m_fileType;
		else if(IdVIDO())
			return m_fileType;
		else if(IdDATA())
			return m_fileType;

		if(head8[0] == 'U' && head8[1] == 'F' && head8[2] == 'E' && head8[3] == ' ' && head8[4] == 'T' && head8[5] == 'E' && head8[6] == 'S' && head8[7] == 'T' && head8[8] == ' ')
		{
			m_fileType = GetUFETestType();
		}
		else
		{
			if(IsAscii())
				m_fileType = FT_TEXT_ASCII;
		}

		head8 = nullptr;


		return m_fileType;
	}


	bool IDFileType::IsAscii()
	{
		m_fileStream->Position = 0;
		Int64 FileSize = m_fileStream->Length;
		Double dfz = (double)FileSize;
		
		if(FileSize > MAX_FILE_SIZE)
			FileSize = MAX_FILE_SIZE;

		array<Int64>^ Count = gcnew array<Int64>(256);
		int x;
					
		// Character Type Counters
		int UppercaseLetters = 0;
		int LowercaseLetters = 0;
		int Punctuation = 0;
		int Numbers = 0;
		int Symbols = 0;
		int Commands = 0;
		int ForeignChars = 0;
		int Return = 0;
		int Spaces = 0;
		int Tabs = 0;
		int Other = 0;

		for(Int64 i = 0; i < FileSize; i++)
		{
			x = m_fileStream->ReadByte();
			Count[x]++;
						
			if(x >= 0x41 && x <= 0x5A)
				UppercaseLetters++;
			else if(x >= 0x61 && x <= 0x7A)
				LowercaseLetters++;
			else if(x >= 0x80 && x <= 0x91)
				ForeignChars++;
			else if(x >= 0x30 && x <= 0x39)
				Numbers++;
			else if(x == 0x0A || x == 0x0D)
				Return++;
			else if(x == 0x20)
				Spaces++;
			else if(x == 0x09)
				Tabs++;
			else if((x >= 0x23 && x <= 0x26) || (x >= 0x28 && x <= 0x2B) || (x >= 0x2D && x <= 0x2F) || x == 0x3A || (x >= 0x3C && x <= 0x3E) || x == 0x40 || (x >= 0x5B && x <= 0x5F) || (x >= 0x7B && x <= 0x7E))
				Symbols++;
			else if(x == 0x21 || x == 0x22 || x == 0x27 || x == 0x2C || x == 0x2E || x == 0x3B || x == 0x3F || x == 0x60)
				Punctuation++;
			else
				Other++;
					
		}

		double pUppercaseLetters = (double)UppercaseLetters / (double)FileSize;
		double pLowercaseLetters = (double)LowercaseLetters / (double)FileSize;
		double pOther = (double)Other / (double)FileSize;
		double pReturn = (double)Return / (double)FileSize;
		double pSpaces = (double)Spaces / (double)FileSize;
		double pTabs = (double)Tabs / (double)FileSize;
		double pPunctuation = (double)Punctuation / (double)FileSize;
		double pSymbols = (double)Symbols / (double)FileSize;
		double pNumbers = (double)Numbers / (double)FileSize;
							
		if(pOther < .10 && (pNumbers + pUppercaseLetters + pLowercaseLetters) > .30 && (pSymbols + pSpaces + pTabs + pReturn + pPunctuation) > pOther)
			return true;
		else 
			return false;
	}

	int IDFileType::OfficeFileType()
	{
		/**********************************************
		Current strategy:
			Search for certain strings in the document that
			idenify its type;
		***********************************************/
		bool isWord = true;
		bool isExcel = true;
		bool isPowerpoint = false;

		array<String^>^ MSWORD = {"Microsoft Office Word Document",
							"MSWordDoc",
							"Word.Document"};
		array<String^>^ MSEXCEL = {"Microsoft Excel"};

		// Microsoft Word
		for(int i = 0; i < MSWORD->Length; i++)
		{
			if(!StringExists(MSWORD[i]))
				isWord = false;
		}

		// Microsoft Excel
		for(int i = 0; i < MSEXCEL->Length; i++)
		{
			if(!StringExists(MSEXCEL[i]))
				isExcel = false;
		}

		int msType = 0;
//		if(isWord && !(isExcel && isPowerpoint))
//			msType = FT_DOCS_MSWORD;
//		else if(isExcel && !(isWord && isPowerpoint))
//			msType = FT_TABL_MSXLS;
		return msType;
	}

	bool IDFileType::StringExists(String^ aString)
	{
		bool eof = false;
		bool exists = false;
		Int64 i = 0;
		Int64 flen = m_fileStream->Length;
		int slen = aString->Length;
		Int64 maxlen = flen - slen;
		Byte b;			
		Encoding^ encodingASCII = gcnew ASCIIEncoding();
		array<Byte>^ Bytes = encodingASCII->GetBytes(aString);
		
		m_fileStream->Position = 0;
		while(i < maxlen && !exists)
		{
			b = m_fileStream->ReadByte();
			if(b == Bytes[0])
			{
				exists = true;
				for(int j = 1; j < slen; j++)
				{
					if(Bytes[j] != m_fileStream->ReadByte())
					{
						exists = false;
					}
				}
				m_fileStream->Position = i++;
			}
			i++;
		}

		return exists;
	}

	bool IDFileType::IdAPPS()
	{
		// Microsoft Compound Document File Format
		if(fileSize >= 8 && head32[0] == 0xD0CF11E0 && head32[1] == 0xA1B11AE1)
		{
			m_fileType = "BASE_MSCMPD";
			//int officeType = OfficeFileType();
			//if(officeType != FT_UNKNOWN)
			//	m_FileType = officeType;
		}
		// Windows shortcut file
		else if(fileSize >= 6 && head8[0] == 'L' && head8[1] == 0x00 && head8[2] == 0x00 && head8[3] == 0x00 && head8[4] == 0x01 && head8[5] == 0x14)
			m_fileType = "APPS_WINSHORT";
		// TI-83 Calculator file
		else if(fileSize >= 16 && head32[0] == 0x2A2A5449 && head32[1] == 0x38332A2A)
			m_fileType = "APPS_TI83";
		else
			return false;

		return true;
	}

	bool IDFileType::IdARCH()
	{
		// ZIP Archive
		if(fileSize > 8 && head32[0] == SIGN_ZIP)
			m_fileType = "ARCH_ZIP";
		// RAR Archive
		else if(fileSize > 8 && head32[0] == SIGN_RAR)
			m_fileType = "ARCH_RAR";
		// Microsoft CAB Archive
		else if(fileSize > 8 && head32[0] == SIGN_CAB)
			m_fileType = "ARCH_CAB";
		// GZIP
		else if(fileSize >= 16 && head16[0] == SIGN_GZIP && head8[2] == 0x08)
			m_fileType = "ARCH_GZIP";
		// BZip2
		else if(fileSize > 16 && head32[0] == 0x425A6839 && head8[4] == 0x31)
			m_fileType = "ARCH_BZIP2";
		// Unknown File Type
		else
			return false;

		return true;
	}

	bool IDFileType::IdAUDI()
	{
		// MIDI
		if(fileSize >= 4 && head8[0] == 0x4D && head8[1] == 0x54 && head8[2] == 0x68 && head8[3] == 0x64 )
			m_fileType = FT_SOUN_MIDI;
		else if(fileSize >= 12 && head32[0] == SIGN_RIFF)
		{
			if(fileSize > 16 && head8[8] == 'W' && head8[9] == 'A' && head8[10] == 'V' && head8[11] == 'E')
				m_fileType = "AUDI_WAVE";
			else if(fileSize > 16 && head8[8] == 'A' && head8[9] == 'V' && head8[10] == 'I' && head8[11] == ' ')
				m_fileType = "VIDO_AVI";
			else if(fileSize > 16 && head8[8] == 'A' && head8[9] == 'C' && head8[10] == 'O' && head8[11] == 'N')
				m_fileType = "GRPH_ANICUR";
			else if(fileSize > 16 && head8[8] == 's' && head8[9] == 'f' && head8[10] == 'b' && head8[11] == 'k')
				m_fileType = "AUDI_SF2";
			else
				m_fileType = "BASE_RIFF";
		}
		// ASF: 30 26 B2 75 8E 66 CF 11 A6 D9 00 AA 00 62 CE 6C 
		else if(fileSize > 100 && head32[0] == 0x3026B275 && head32[1] == 0x8E66CF11 && head32[2] == 0xA6D900AA && head32[3] == 0x0062CE6C)
			m_fileType = "BASE_ASF";		
		// MP3 with ID3 tag
		else if(fileSize > 16 && head8[0] == 'I' && head8[1] == 'D' && head8[2] == '3')
			m_fileType = "AUDI_MP3";
		// MP3 with no ID3
		//else if(fileSize >= 6 && head8[0] == 0xFF && head8[1] >= 0xE0 && head8[2] <= 0xF0)
		//	m_FileType = FT_SOUN_MP3;
		// OGG Vorbis
		else if(fileSize > 25 && head8[0] == 'O' && head8[1] == 'g' && head8[3] == 'g')
			m_fileType = "AUDI_OGGVORB";
		else
			return false;

		return true;
	}

	bool IDFileType::IdDATA()
	{
		// Access Database - 00 01 00 00 53 74 61 6E 64 61 72 64 20 4A 65 74 20 44 42 00
		if(fileSize >= 20 && head32[0] == 0x00010000 && head8[4] == 'S' && head8[5] == 't' && head8[6] == 'a' && head8[7] == 'n' && head8[8] == 'd' && head8[9] == 'a' && head8[10] == 'r' && head8[11] == 'd' && head8[12] == ' ' && head8[13] == 'J' && head8[14] == 'e' && head8[15] == 't')
			m_fileType = FT_DATA_ACCESS;
		else
			return false;

		return true;
	}

	bool IDFileType::IdDOCS()
	{
		// Adobe Portable Document Format
		if(fileSize > 8 && head32[0] == 0x25504446)
			m_fileType = "DOCS_PORTDOCFORM";
		// Windows Help Format
		else if(fileSize > 8 && head32[0] == 0x3F5F0300)
			m_fileType = "DOCS_WINHELP";
		// Compiled Help Format
		else if(fileSize > 8 && head32[0] == 0x49545346)
			m_fileType = "DOCS_CHM";
		else
			return false;

		return true;
	}

	bool IDFileType::IdEXEC()
	{
		// Executable
		if(fileSize > 16 && head8[0] == 'M' && head8[1] == 'Z')
		{
			if(m_fileStream->Length > 60)
			{
				m_fileStream->Position = 60;
				UInt32 offset = m_binaryReader->ReadUInt32();
				if(m_fileStream->Length > offset)
				{
					m_fileStream->Position = offset;
					UInt32 headerSign = m_binaryReader->ReadUInt32();
					if(headerSign == 0x4550)
						m_fileType = "EXEC_PE";
					if(headerSign == 0x454E)
						m_fileType = "EXEC_NE";
				}
			}
			else
			{
				m_fileType = "EXEC_MZ";
			}
		}
		// Linux ELF Executables
		else if(fileSize > 16 && head8[0] == 0x7F && head8[1] == 'E' && head8[2] == 'L' && head8[3] == 'F')
			m_fileType = "EXEC_ELF";
		// Shockwave files
		else if(fileSize > 16 && head8[0] == 'F' && head8[1] == 'W' && head8[2] == 'S')
			m_fileType = "EXEC_SHOCKWAVE";
		// Super Nintendo ROM
//		else if(fileSize >= 12 && head8[8] == 0xAA && head8[9] == 0xBB && head8[10] == 0x04)
//			m_fileType = FT_EXEC_SNES;		
		// Java Class	0xCAFEBABE
		else if(fileSize > 8 && head32[0] == 0xCAFEBABE)
			m_fileType = "EXEC_JAVACLASS";
		else
			return false;

		return true;
	}

	bool IDFileType::IdGRPH()
	{		
		// JFIF
		if(fileSize >= 10 && head8[0] == 0xFF && ((head8[6] == 'J' && head8[7] == 'F' && head8[8] == 'I' && head8[9] == 'F') || (head8[6] == 'J' && head8[7] == 'F' && head8[8] == 'X' && head8[9] == 'X') ||(head8[6] == 'E' && head8[7] == 'x' && head8[8] == 'i' && head8[9] == 'f')))
			m_fileType = FT_RAST_JPEG;		
		// Graphic Interchange Format
		else if(fileSize >= 8 && head8[0] == 'G' && head8[1] == 'I' && head8[2] == 'F' && head8[3] == 0x38 && (head8[4] == 0x37 || head8[4] == 0x39) && head8[5] == 0x61)
			m_fileType = FT_RAST_GIF;
		// Portable Network Graphic
		else if(fileSize >= 16 && head32[0] == SIGN_PNG1 && head32[1] == SIGN_PNG2)
			m_fileType = FT_RAST_PNG;		
		// Windows Bitmap - First two bytes must be the signature
		else if(fileSize >= MINSZ_RAST_WINBMP && head16[0] == SIGN_WBMP)
			m_fileType = FT_RAST_WINBMP;
		// Windows Metafile Placeable - First four bytes must be the signature
		else if(fileSize >= MINSZ_VECT_WMF && head32[0] == 0xD7CDC69A)
			m_fileType = "GRPH_WMF";
		// Windows Metafile
		else if(fileSize >= MINSZ_VECT_WMF && (head8[0] == 0x01 || head8[0] == 0x02) && head8[1] == 0x00 && head8[2] == 0x09 && head8[16] == 0x00 && head8[17] == 0x00)
			m_fileType = "GRPH_WMF";
		// Enhanced Metafile
		else if(fileSize >= MINSZ_VECT_EMF && head32[10] == SIGN_EMF && head32[0] == 0x01000000)
			m_fileType = "GRPH_EMF";
		// Windows Cursor File
		else if(fileSize > 16 && head32[0] == 0x00000200)
			m_fileType = "GRPH_WINCUR";
		// Windows Icon File
		else if(fileSize > 16 && head32[0] == 0x00000100)
			m_fileType = "GRPH_WINICON";
		// Windows Animated Cursor
		else if(fileSize > 16 && head32[0] == SIGN_RIFF && head32[1] == SIGN_ANICUR)
			m_fileType = "GRPH_ANICUR";
		else
			return false;

		return true;
	}

	bool IDFileType::IdVIDO()
	{		
		return false;
	}
	
	bool IDFileType::IdRareTypes()
	{
		// Power Tab File
		if(fileSize > 25 && head32[0] == SIGN_POWERTAB && head8[4] <= 0x04 && head8[5] == 0x00)
			m_fileType = "APPS_PTAB";
		// Guitar Pro
		else if(fileSize > 64 && head32[0] == 0x18464943 && head32[1] == 0x48494952 && head32[2] == 0x20475549 && head32[3] == 0x54415220 && head32[4] == 0x50524F20)
			m_fileType = "APPS_GUITARPRO";
		// Macro Express Script
		else if(fileSize > 16 && head8[0] == 'M' && head8[1] == 'E' && head16[1] == 0x0000 && head8[4] == 0xBB && head8[5] == 0x0B)
			m_fileType = "APPS_MESCRIPT";
		// Unreal Tournament Package
		else if(fileSize > 16 && head32[0] == 0xC1832A9E)
			m_fileType = "APPS_UNREALPACK";
		else
			return false;

		return true;
	}
		
	bool IDFileType::IdRiff()
	{

		return false;
	}
		
	bool IDFileType::IdAsf()
	{

		return false;
	}
	
	String^ IDFileType::GetUFETestType()
	{
		if(head8[9] == 'V' && head8[10] == 'C' && head8[11] == 'S')
		{
			if(head8[12] == '7' && head8[13] == '1')
				m_fileType = "TEST_VCS_71";
			else if(head8[12] == '8' && head8[13] == '0')
				m_fileType = "TEST_VCS_80";
		}
		else if(head8[9] == 'V' && head8[10] == 'B')
		{
			if(head8[11] == '7' && head8[12] == '1')
				m_fileType = "TEST_VB_71";
		}
		else if(head8[9] == 'V' && head8[10] == 'C' && head8[11] == 'P' && head8[12] == 'P')
		{
			if(head8[13] == '7' && head8[14] == '1')
				m_fileType = "TEST_VCPP_71";
			else if(head8[13] == '8' && head8[14] == '0')
				m_fileType = "TEST_VCPP_80";
		}
		else if(head8[9] == 'V' && head8[10] == 'J' && head8[11] == 'S')
		{
			if(head8[12] == '7' && head8[13] == '1')
				m_fileType = "TEST_VJS_71";
			else if(head8[12] == '8' && head8[13] == '0')
				m_fileType = "TEST_VJS_80";
		}
		return m_fileType;
	}
};