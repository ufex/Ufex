// UFEDataFile.h

#pragma once

using namespace System;
using namespace System::Collections;
using namespace System::Text;
using namespace System::IO;
using namespace System::Globalization;

#define SIGNATURE1 "UFEx"
#define SIGNATURE2 "DATA"
#define SIGNATURE3 0x0A0D1A0A
#define SIGNATURE4 "2005"

#define UFEDATTYPE_BYTE		0x01
#define UFEDATTYPE_UINT16	0x02
#define UFEDATTYPE_INT16	0x03
#define UFEDATTYPE_UINT32	0x04
#define UFEDATTYPE_INT32	0x05
#define UFEDATTYPE_UINT64	0x06
#define UFEDATTYPE_INT64	0x07
#define UFEDATTYPE_STRING	0x10

typedef unsigned char       BYTE;
typedef unsigned short      WORD;
typedef unsigned long       DWORD;

#define UFE_STRUCT_PROPLIST		0x02

namespace UniversalFileExplorer
{

	private ref struct UFEDAT_HEADER
	{
		array<Byte>^ signature1;		// "UFEx"
		array<Byte>^ signature2;		// "DATA"
		array<Byte>^ signature3;		
		array<Byte>^ signature4;		// Year
		UInt16 dataFileType;
		UInt16 dataFileID;
		UInt16 fileVersion;
		UInt16 progVersMaj;
		UInt16 progVersMin;
		UInt16 progVersBuild;
		UInt16 progVersRev;
		UInt16 flags;
		UInt64 fileSize;
		UInt64 stringKey;
		UInt64 numberKey;
		UInt32 reserved;
		UInt32 checksum;
	};

	private ref struct UFEDAT_ENTRY
	{
		String^ name;
		UInt32 type;
		UInt32 size;
		UInt64 offset;

	};
	
	public ref struct UFE_PROPERTY
	{
		String^ name;
		UInt16 number;
	};

	public ref struct UFE_PROP_U16
	{
		String^ name;
		UInt16 number;
	};

	public ref struct UFE_PROP_U32
	{
		String^ name;
		UInt32 number;
	};

	public ref struct UFE_PROPERTY_LIST
	{
		UInt32 numItems;
		array<UFE_PROP_U16^>^ items;
	};

	private ref struct UFEDAT_PROPFILE
	{
		UInt32 numRows;
		UInt32 numColumns;
		array<Byte>^ colTypes;
		array<UFE_PROPERTY^>^ rowData;
	};

	private ref struct UFEDAT_VARIABLE
	{
		UInt16 id;
		UInt16 size;
		Byte type;
		bool array;
	};

	private ref struct INPUT_FILE
	{
		String^ fileName;
		UInt32 type;
	};

	private ref struct CONFIG_FILE
	{
		String^ outFile;
		int numInputFiles;
		array<INPUT_FILE^>^ inputFiles;
	};

	private ref struct UFEDAT_STRUCTURE
	{
		UInt16 id;		// Structure ID
		UInt32 size;	// Size of the Structure
		UInt16 numVars;	// The number of variables in the structure
		array<UFEDAT_VARIABLE^>^ variables;
	};

	private ref struct UFEDAT_FILE
	{
		UFEDAT_HEADER^ header;
	};


	public ref class ReadUFEFile
	{
		public:
			ReadUFEFile(void);
			ReadUFEFile(String^);
			ReadUFEFile(String^, String^);
			array<UFE_PROPERTY^>^ GetProperties();
			array<UFE_PROPERTY^>^ GetProperties(String^);
			ArrayList^ GetPropertiesCSV();
			String^ m_FileName;

		private:
			UFEDAT_HEADER^ ReadHeader();
			
			// Signature Constants
			static array<Byte>^ SIGN1 = {0x55, 0x46, 0x46, 0x78};
			static array<Byte>^ SIGN2 = {0x44, 0x41, 0x54, 0x41};
			static array<Byte>^ SIGN3 = {0x0A, 0x0D, 0x1A, 0x0A};
			static array<Byte>^ SIGN4 = {0x32, 0x30, 0x30, 0x35};

			String^ m_FilePath;
			FileStream^ m_FileStream;
			BinaryReader^ m_BinRead;
			UFEDAT_HEADER^ m_Header;
			ArrayList^ m_Structures;
			ArrayList^ m_Data;
	};

	public ref class WriteUFEFile
	{
		public:
			WriteUFEFile(void);
			bool ConvertCSV2UFE(String^ , String^);

			bool NewUFEFile(String^ configFile);

		private:
			bool WriteHeader(UFEDAT_HEADER^);
			bool WriteProperties(UFEDAT_PROPFILE^);

			CONFIG_FILE^ ReadConfigFile(String^ configFile);

			// Signature Constants
			static array<Byte>^ SIGN1 = { 0x55, 0x46, 0x46, 0x78 };
			static array<Byte>^ SIGN2 = { 0x44, 0x41, 0x54, 0x41 };
			static array<Byte>^ SIGN3 = { 0x0A, 0x0D, 0x1A, 0x0A };
			static array<Byte>^ SIGN4 = { 0x32, 0x30, 0x30, 0x35 };
			String^ m_FileName;
			FileStream^ m_FileStream;
			BinaryWriter^ bw;
	};
};
