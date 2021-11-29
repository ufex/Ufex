#pragma once
using namespace System;

namespace UniversalFileExplorer
{
	public ref class DataManip
	{
	public:

		//template <typename T>
		//inline bool GetBit(T v, int position) { return ((v & (T)(1 << position)) != 0); };

		inline static bool GetBit(Byte v, int p) { return ((v & (Byte)(1 << p)) != 0); };
		inline static bool GetBit(UInt16 v, int p) { return ((v & (UInt16)(1 << p)) != 0); };
		inline static bool GetBit(UInt32 v, int p) { return ((v & (UInt32)(1 << p)) != 0); };
		inline static bool GetBit(UInt64 v, int p) { return ((v & (UInt64)(1 << p)) != 0); };
		
		inline static Byte GetByte(UInt16 value, int position) { return (BYTE)((value & (UInt16)(0xFF << (position * 8))) >> (position * 8)); };
		inline static Byte GetByte(UInt32 value, int position) { return (BYTE)((value & (UInt32)(0xFF << (position * 8))) >> (position * 8)); };
		inline static Byte GetByte(UInt64 value, int position) { return (BYTE)((value & (UInt64)(0xFF << (position * 8))) >> (position * 8)); };

		// Get a BYTE from a WORD
		inline static Byte GetHighByte(UInt16 x) { return static_cast<Byte>(x >> 8); };
		inline static Byte GetLowByte(UInt16 x) { return static_cast<Byte>(x & 0x00FF); };	
		
		// Get a WORD from a DWORD
		inline static UInt16 GetHighWord(UInt32 x) { return static_cast<UInt16>(x >> 16); };
		inline static UInt16 GetLowWord(UInt32 x) { return static_cast<UInt16>(x & 0x0000FFFF); };

		// Get a DWORD from a QWORD
		inline static UInt32 GetHighDword(UInt64 x) { return static_cast<UInt32>(x >> 32); }
		inline static UInt32 GetLowDword(UInt64 x) { return static_cast<UInt32>(x & 0x00000000FFFFFFFF); }

		// Get a specific byte from a DWORD
		inline static Byte GetByteFromDWORD(UInt32 v, int p) { return ((v & (UInt32)(0xFF << (p * 8))) >> (p * 8)); };
	};
};