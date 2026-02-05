#pragma once
using namespace System;
using namespace System::IO;
using namespace System::Text;


// Max number of bytes to be processed by IsAscii Function
#define MAX_FILE_SIZE		0x00200000	// 2 MB

#define NUM_READ_BYTES		0x00000040	// 64 Bytes


// Signatures
#define SIGN_EMF			0x20454D46	// " EMF"
#define SIGN_RIFF			0x52494646	// "RIFF"
#define SIGN_ANICUR			0x41434F4E	// "ACON"
#define SIGN_PNG1			0x89504E47	// "%PNG"
#define SIGN_PNG2			0x0D0A1A0A 
#define SIGN_WBMP			0x424D		// "BM"
#define SIGN_POWERTAB		0x70746162	// "ptab"
#define SIGN_GZIP			0x1F8B
#define SIGN_ZIP			0x504B0304	// "PK\03\04"
#define SIGN_RAR			0x52617221	// "Rar!"
#define SIGN_CAB			0x4D534346	// "MSCF"


namespace UniversalFileExplorer
{
	public ref class IDFileType : public FileTypeClassifier
	{
	public:
		IDFileType(void);
		String^ GetFileType(String^ fp, FileStream^ fs) override;
	
	private:
		// Specific Type Identification		
		bool IsAscii();
		int OfficeFileType();

		String^ GetUFETestType();
		
		bool IdGRPH();
		bool IdDOCS();
		bool IdEXEC();
		bool IdAPPS();
		bool IdARCH();
		bool IdAUDI();
		bool IdVIDO();
		bool IdDATA();
		bool IdRareTypes();

		bool IdRiff();
		bool IdAsf();

		// Returns true if aString exists in the current file stream
		bool StringExists(String^ aString);
		String^ m_filePath;
		String^ m_fileType;
		FileStream^ m_fileStream;
		BinaryReader^ m_binaryReader;

		// The file size
		Int64 fileSize;

		// The header in different formats
		array<Byte>^ head8;
		array<UInt16>^ head16;
		array<UInt32>^ head32;
	};
};