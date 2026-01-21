// UFEUtil.h
#pragma once

using namespace System;


namespace UniversalFileExplorer
{
	bool CompareArrays(const array<Byte>^ a1, const array<Byte>^ a2);
	/*
	// Math Stuff
	bool GetBit(Byte b, Byte p); //{ return ((b & (Byte)(1 << p)) != 0); };

	Byte GetLowByte(const UInt16 x); //{ return static_cast<Byte>(x & 0x00FF); };
	Byte GetHighByte(const UInt16 x); //{ return static_cast<Byte>(x >> 8); };


	UInt16 GetLowWord(UInt32 x);
	UInt16 GetHighWord(UInt32 x);	

	UInt32 GetHighDword(UInt64 x);
	UInt32 GetLowDword(UInt64 x);
*/

};
