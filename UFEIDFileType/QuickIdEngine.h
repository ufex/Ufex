#pragma once

typedef unsigned __int8		BYTE;
typedef unsigned __int16	WORD;
typedef unsigned __int32	DWORD;
typedef unsigned __int64	QWORD;

class QuickIdEngine
{
public:

	QuickIdEngine();

	~QuickIdEngine(void);

	bool GetFileType(const char* filePath, char* fileType);

	bool IdGRPH();
	bool IdDOCS();
	bool IdEXEC();
	bool IdAPPS();
	bool IdARCH();
	bool IdAUDI();
	bool IdVIDO();
	bool IdDATA();
	bool IdRareTypes();

private:


	char* m_fileType;

	// Header arrays (all point to the same array)

	BYTE* head8;
	WORD* head16;
	DWORD* head32;
	QWORD* head64;
};
