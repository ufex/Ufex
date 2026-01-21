// This is the main DLL file.

#include "Stdafx.h"
#include "StringPack.h"

namespace UniversalFileExplorer
{

	StringPack::StringPack()
		: m_stringKey(0),
		m_numberKey(0)

	{

	}
	
	StringPack::~StringPack()
	{
	}
	
	bool StringPack::Unpack(BYTE in[], char* &out, int size)
	{
		// Unpack the bytes		
		int dataLenBytes = size;
		int stringLen = ((dataLenBytes * 8) / 6);
		//BYTE* asciiChars = new BYTE[stringLen];
		out = new char[stringLen];
		for(int i = 0; i < dataLenBytes; i += 3)
		{
			BYTE encBytes[3];
			BYTE decBytes[4];

			for(int eb = 0; eb < 3; eb++)
			{
				encBytes[eb] = in[i + eb];
			}
			// Unpack Byte0 = ABCDEFxx xxxxxxxx xxxxxxxx
			decBytes[0] = encBytes[0] >> 2;
			// Unpack Byte1 = xxxxxxAB CDEFxxxx xxxxxxxx
			decBytes[1] = encBytes[1] >> 4;
			setBit(decBytes[1], 5, getBit(encBytes[0], 1));
			setBit(decBytes[1], 4, getBit(encBytes[0], 0));
			// Unpack Byte2 = xxxxxxxx xxxxABCD EFxxxxxx
			decBytes[2] = encBytes[1] << 2;
			setBit(decBytes[2], 7, false);
			setBit(decBytes[2], 6, false);
			setBit(decBytes[2], 1, getBit(encBytes[2], 7));
			setBit(decBytes[2], 0, getBit(encBytes[2], 6));
			// Unpack Byte3 = xxxxxxxx xxxxxxxx xxABCDEF
			decBytes[3] = encBytes[2] << 2;
			decBytes[3] = decBytes[3] >> 2;

			for(int db = 0; db < 4; db++)
			{
				out[((i / 3) * 4) + db] = (char)GetCodeB(decBytes[db]);
			}
		}
		return true;
	}

	/**********************************
	Converts a string of 8 bit characters to 6 bit chars
	***********************************/
	bool StringPack::Pack(char* in, BYTE out[], int size)
	{
		int stringLen = size;
		BYTE* tmpasciiChars = new BYTE[stringLen];
		
		// Get the Bytes
		for(int i = 0; i < stringLen; i++)
		{
			tmpasciiChars[i] = (BYTE)in[i];
		}	

		int dataLenBits = stringLen * 6;
		int extrabytes = 0;

		// Add Padding so that it fits
		while(dataLenBits % 8 != 0)
		{
			dataLenBits += 6;
			extrabytes++;
		}
		int dataLenBytes = dataLenBits / 8;

		BYTE* asciiChars = new BYTE[stringLen + extrabytes];
		int asciiCharsLen = stringLen + extrabytes;
		for(int i = 0; i < asciiCharsLen; i++)
		{
			if(i < stringLen)
				asciiChars[i] = tmpasciiChars[i];
			else
				asciiChars[i] = 0x00;
		}
		//No// 1 Byte added for the string size
		BYTE* output = new BYTE[dataLenBytes];
		//output[1] = (Byte)(dataLenBytes / 3);
		// Package the bytes
		for(int i = 0; i < stringLen; i += 4)
		{
			BYTE encBytes[3];
			BYTE decBytes[4];
			for(int db = 0; db < 4; db++)
			{
				decBytes[db] = GetCodeA(asciiChars[i + db]);
			}
			// Pack Byte0 = 00ABCDEF 00GHxxxx 00xxxxxx 00xxxxxx
			encBytes[0] = decBytes[0] << 2;
			setBit(encBytes[0], 1, getBit(decBytes[1], 5));
			setBit(encBytes[0], 0, getBit(decBytes[1], 4));
			// Pack Byte1 = 00xxxxxx 00xxABCD 00EFGHxx 00xxxxxx
			encBytes[1] = decBytes[1] << 4;
			setBit(encBytes[1], 3, getBit(decBytes[2], 5));
			setBit(encBytes[1], 2, getBit(decBytes[2], 4));
			setBit(encBytes[1], 1, getBit(decBytes[2], 3));
			setBit(encBytes[1], 0, getBit(decBytes[2], 2));
			// Pack Byte2 = 00xxxxxx 00xxxxxx 00xxxxAB 00CDEFGH
			encBytes[2] = decBytes[2] << 6;
			setBit(encBytes[2], 5, getBit(decBytes[3], 5));
			setBit(encBytes[2], 4, getBit(decBytes[3], 4));
			setBit(encBytes[2], 3, getBit(decBytes[3], 3));
			setBit(encBytes[2], 2, getBit(decBytes[3], 2));
			setBit(encBytes[2], 1, getBit(decBytes[3], 1));
			setBit(encBytes[2], 0, getBit(decBytes[3], 0));

			for(int eb = 0; eb < 3; eb++)
			{
				output[((i/4) * 3) + eb] = encBytes[eb];
			}
		}
		out = output;
		return true;
	}
	
	// Converts the ASCII Char Code to a 6-Bit Char Code
	BYTE StringPack::GetCodeA(BYTE code)
	{
		if(code >= 0x41 && code <= 0x5A)
		{
			// Upper Case Letters
			return code - 64;	
		}
		else if(code >= 0x61 && code <= 0x7A)
		{
			// Lower Case Letters
			return code - 70;	
		}
		else if(code >= 0x30 && code <= 0x39)
		{
			// Numeric Chars
			return code + 5;
		}
		else if(code == 0x5F)
		{
			// Underscore
			return 63;
		}
		else if(code == 0x20)
		{
			// Space
			return 63;
		}
		else
		{
			// Unknown char
			return 0x00;
		}
	}

	BYTE StringPack::GetCodeB(BYTE code)
	{
		if(code >= 0x01 && code <= 0x1A)
		{
			// Upper case letters
			return code + 64;	
		}
		else if(code >= 0x1B && code <= 0x34)
		{
			// Lower case letters
			return code + 70;	
		}
		else if(code >= 0x35 && code <= 0x3E)
		{
			// Numbers
			return code - 5;
		}
		else if(code == 0x00)
		{
			// Null Character
			return 0x00;
		}
		else if(code == 63)
		{
			// Underscore
			return 0x5F;
		}
		else
		{
			// Null Character
			return 0x00;
		}
	}

	bool StringPack::getBit(BYTE b, BYTE p)
	{
		return ((b & (BYTE)(1 << p)) != 0);
	}

	BYTE StringPack::setBit(BYTE &b, BYTE i, bool v)
	{
		BYTE bitMask = 1;
		bitMask <<= i;
		if(v)
		{
			b |= bitMask;
		}
		else
		{
			bitMask = ~bitMask ;  // Flip bits
			b &= bitMask ; 
		}
		return b;
	}

};