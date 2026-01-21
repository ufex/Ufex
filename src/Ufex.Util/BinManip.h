#pragma once

#ifndef BYTE
#	define BYTE		unsigned char
#endif

#ifndef WORD
#	define WORD		unsigned short
#endif

#ifndef DWORD
#	define DWORD	unsigned long
#endif

#ifndef QWORD
#	define QWORD	unsigned __int64
#endif


template <typename T>
inline bool GetBit(T v, int position) { return ((v & (T)(1 << position)) != 0); };
//inline static bool GetBit(Byte v, int p) { return ((v & (Byte)(1 << p)) != 0); };
//inline static bool GetBit(UInt16 v, int p) { return ((v & (UInt16)(1 << p)) != 0); };

template <typename T>
inline BYTE GetByte(T value, int position) { return (BYTE)((value & (T)(0xFF << (position * 8))) >> (position * 8)); };

// Get the high Byte from a Word
inline BYTE GetHighByte(WORD x)		{ return static_cast<BYTE>(x >> 8); };

// Get the low Byte from a Word
inline BYTE GetLowByte(WORD x)		{ return static_cast<BYTE>(x & 0x00FF); };	

// Get the high Word from a DWord
inline WORD GetHighWord(DWORD x)	{ return (WORD)(x >> 16); };

// Get the low Word from a DWord
inline WORD GetLowWord(DWORD x)		{ return (WORD)(x & 0x0000FFFF); };

// Get the high DWord from a QWord
inline DWORD GetHighDword(QWORD x)	{ return (DWORD)(x >> 32); }

// Get the low DWord from a QWord
inline DWORD GetLowDword(QWORD x)	{ return (DWORD)(x & 0x00000000FFFFFFFF); }


// Get a specific byte from a DWORD
inline BYTE GetByteFromDWORD(DWORD v, int p) { return (BYTE)((v & (DWORD)(0xFF << (p * 8))) >> (p * 8)); };


// Creates a WORD from 2 BYTEs
inline WORD MakeWord(BYTE high, BYTE low) { return (WORD)((high * 0x0100) + low); };


