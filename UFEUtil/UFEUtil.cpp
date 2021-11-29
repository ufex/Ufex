// This is the main DLL file.

#include "stdafx.h"

#include "UFEUtil.h"

namespace UniversalFileExplorer
{	

	bool CompareArrays(const array<Byte>^ a1, const array<Byte>^ a2)
	{
		// Return false if the arrays are not the same size
		if(a1->Length != a2->Length)
			return false;

		// Compare the elements in each array
		for(int i = 0; i < a1->Length; i++)
		{
			if(a1[i] != a2[i])
				return false;
		}
		return true;
	}
/*
	// Returns true if bit p in Byte b is set
	bool GetBit(Byte b, Byte p)
	{
		return ((b & (Byte)(1 << p)) != 0);
	}

	// Returns the High Byte in an Unsigned 16-bit Integer
	Byte GetHighByte(const UInt16 x)
	{
		return static_cast<Byte>(x >> 8);
	}

	// Returns the Low Byte in an Unsigned 16-bit Integer	
	Byte GetLowByte(const UInt16 x)
	{
		return static_cast<Byte>(x & 0x00FF);
	}	

	// Returns the high word in a dword 
	UInt16 GetHighWord(UInt32 x)
	{
		return static_cast<UInt16>(x >> 16);
	}

	// Returns the low word in a dword
	UInt16 GetLowWord(UInt32 x)
	{
		return static_cast<UInt16>(x & 0x0000FFFF);
	}

	UInt32 GetHighDword(UInt64 x)
	{
		return static_cast<UInt32>(x >> 32);
	}


	UInt32 GetLowDword(UInt64 x)
	{
		return static_cast<UInt32>(x & 0x00000000FFFFFFFF);
	}

*/
};