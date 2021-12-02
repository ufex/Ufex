// This is the main DLL file.

#include "stdafx.h"

#include "UFENumToString.h"


inline bool GETBIT(unsigned char b, unsigned char p) { return ((b & (unsigned char)(1 << p)) != 0); }
inline bool GETBITS(char b, char p) { return ((b & (char)(1 << p)) != 0); }

inline String^ GETBITSTR(unsigned char b, unsigned char p) { if(GETBIT(b, p)) return "1"; else return "0"; }

namespace UniversalFileExplorer
{	
	// Default Constructor
	NumToString::NumToString()
	{
		m_CI = gcnew CultureInfo("en-US", false);
		m_NFI = m_CI->NumberFormat;
		
		// Set the seperator
		m_NFI->NumberGroupSeparator = DEF_SEP;
		m_NFI->NumberDecimalDigits = 0;

		// Default Options
		m_HexShowLeadX = true;
		m_HexShowLeadZeros = true;
		m_HexCaps = true;
		m_DecCommas = false;
	}

	String^ NumToString::GetStrObject(Object^ x)
	{
		Type^ myType = x->GetType();

		if(myType->Equals(String::typeid))
		{
			return static_cast<String^>(x);
		}
		else if(myType->Equals(Byte::typeid))
		{
			return GetStrByte(static_cast<Byte>(x));
		}
		else if(myType->Equals(UInt16::typeid))
		{
			return GetStrUInt16(static_cast<UInt16>(x));
		}
		else if(myType->Equals(UInt32::typeid))
		{
			return GetStrUInt32(static_cast<UInt32>(x));
		}
		else if(myType->Equals(UInt64::typeid))
		{
			return GetStrUInt64(static_cast<UInt64>(x));
		}
		else if(myType->Equals(SByte::typeid))
		{
			return GetStrSByte(static_cast<SByte>(x));
		}
		else if(myType->Equals(Int16::typeid))
		{
			return GetStrInt16(static_cast<Int16>(x));
		}
		else if(myType->Equals(Int32::typeid))
		{
			return GetStrInt32(static_cast<Int32>(x));
		}
		else if(myType->Equals(Int64::typeid))
		{
			return GetStrInt64(static_cast<Int64>(x));
		}
		else if(myType->Equals((Object::typeid->MakePointerType()->MakeArrayType())))
		{
			return GetStrObjectArray(static_cast<array<Object^>^>(x));
		}
		else if(myType->Equals(Byte::typeid->MakeArrayType()))
		{
			return GetStrU8Array(static_cast<array<Byte>^>(x));
		}
		else if(myType->Equals(UInt16::typeid->MakeArrayType()))
		{
			return GetStrU16Array(static_cast<array<UInt16>^>(x));
		}
		else if(myType->Equals(UInt32::typeid->MakeArrayType()))
		{
			return GetStrU32Array(static_cast<array<UInt32>^>(x));
		}
		else if(myType->Equals(SByte::typeid->MakeArrayType()))
		{
			return GetStrSByteArray(static_cast<array<SByte>^>(x));
		}
		else if(myType->Equals(Guid::typeid))
		{
			return x->ToString(); 
		}
		else
		{
			return x->ToString();
		}
	}

	String^ NumToString::GetStrObjectArray(array<Object^>^ x)
	{
		StringBuilder^ sb = gcnew StringBuilder();

		for(int i = 0; i < x->Length; i++)
		{
			sb->Append(GetStrObject(x[i]));
		}

		return sb->ToString();
	}

	//------------------------------------------------
	//	Number->String Conversion Functions
	//		GetStrU8Array
	//		GetStrU8
	//		GetStrU16
	//		GetStrU32
	//		GetStrU64
	//------------------------------------------------
	String^ NumToString::GetStrCharArray(array<char>^ x)
	{
		if(x == nullptr)
			return "NULL";

		if(x->Length == 0)
			return "{}";

		String^ retVal = "{";
		
		for(int i = 0; i < x->Length - 1; i++)
			retVal = String::Concat(retVal, GetStrSByte(x[i]), ", ");
		
		retVal = String::Concat(retVal, GetStrSByte(x[x->Length - 1]), "}");

		return retVal;
	}

	String^ NumToString::GetStrU8Array(array<Byte>^ x)
	{		
		if(x == nullptr)
			return "NULL";

		if(x->Length == 0)
			return "{}";

		String^ retVal = "{";
		
		for(int i = 0; i < x->Length - 1; i++)
			retVal = String::Concat(retVal, GetStrByte(x[i]), ", ");
		
		retVal = String::Concat(retVal, GetStrByte(x[x->Length - 1]), "}");

		return retVal;
	}
	
	String^ NumToString::GetStrU16Array(array<UInt16>^ x)
	{		
		if(x == nullptr)
			return "NULL";

		if(x->Length == 0)
			return "{}";

		String^ retVal = "{";
		
		for(int i = 0; i < x->Length - 1; i++)
			retVal = String::Concat(retVal, GetStrUInt16(x[i]), ", ");
		
		retVal = String::Concat(retVal, GetStrUInt16(x[x->Length - 1]), "}");

		return retVal;
	}
	
	String^ NumToString::GetStrU32Array(array<UInt32>^ x)
	{		
		if(x == nullptr)
			return "NULL";

		if(x->Length == 0)
			return "{}";

		String^ retVal = "{";
		
		for(int i = 0; i < x->Length - 1; i++)
			retVal = String::Concat(retVal, GetStrUInt32(x[i]), ", ");
		
		retVal = String::Concat(retVal, GetStrUInt32(x[x->Length - 1]), "}");

		return retVal;
	}

	String^ NumToString::GetStrU64Array(array<UInt64>^ x)
	{		
		if(x == nullptr)
			return "NULL";

		if(x->Length == 0)
			return "{}";

		String^ retVal = "{";
		
		for(int i = 0; i < x->Length - 1; i++)
			retVal = String::Concat(retVal, GetStrUInt64(x[i]), ", ");
		
		retVal = String::Concat(retVal, GetStrUInt64(x[x->Length - 1]), "}");

		return retVal;
	}
		
	String^ NumToString::GetStrSByteArray(array<SByte>^ x)
	{		
		if(x == nullptr)
			return "NULL";

		if(x->Length == 0)
			return "{}";

		String^ retVal = "{";
		
		for(int i = 0; i < x->Length - 1; i++)
			retVal = String::Concat(retVal, GetStrSByte(x[i]), ", ");

		retVal = String::Concat(retVal, GetStrSByte(x[x->Length - 1]), "}");

		return retVal;
	}

	String^ NumToString::GetStrASCIIString(array<char>^ x)
	{
		if(m_numFormat == NumberFormat::Default)
			return gsASCA(x);
		else if(m_numFormat == NumberFormat::Hexadecimal)
			return gsASCH(x);
		else if(m_numFormat == NumberFormat::Decimal)
			return gsASCD(x);
		else if(m_numFormat == NumberFormat::Binary)
			return gsASCA(x);
		else if(m_numFormat == NumberFormat::Ascii)
			return gsASCA(x);
		else
			return x->ToString();

	}
	
	String^ NumToString::GetStrBool(bool x)
	{
		if(m_numFormat == NumberFormat::Default)
		{
			if(x)
				return "True";
			else
				return "False";
		}
		else if(m_numFormat == NumberFormat::Hexadecimal)
		{
			if(x)
				return "0x1";
			else
				return "0x0";
		}
		else if(m_numFormat == NumberFormat::Decimal)
		{	
			if(x)
				return "1";
			else
				return "0";
		}
		else if(m_numFormat == NumberFormat::Binary)
		{	
			if(x)
				return "1";
			else
				return "0";
		}

		if(x)
			return "True";
		else
			return "False";
	}

	String^ NumToString::GetStrByte(Byte x)
	{	
		if(m_numFormat == NumberFormat::Default)
			return gsU08H(x);
		else if(m_numFormat == NumberFormat::Hexadecimal)
			return gsU08H(x);
		else if(m_numFormat == NumberFormat::Decimal)
			return gsU08D(x);
		else if(m_numFormat == NumberFormat::Binary)
			return gsU08B(x);
		else if(m_numFormat == NumberFormat::Ascii)
			return gsU08A(x);
		else
			return x.ToString();	
	}
	
	String^ NumToString::GetStrUInt8(Byte x)
	{
		return this->GetStrByte(x);
	}

	String^ NumToString::GetStrUInt16(UInt16 x)
	{
		if(m_numFormat == NF_DEF)
			return gsU16H(x);
		else if(m_numFormat == NumberFormat::Hexadecimal)
			return gsU16H(x);
		else if(m_numFormat == NumberFormat::Decimal)
			return gsU16D(x);
		else if(m_numFormat == NumberFormat::Binary)
			return gsU16B(x);
		else if(m_numFormat == NumberFormat::Ascii)
			return gsU16A(x);	
		else
			return x.ToString();
	}

	String^ NumToString::GetStrUInt32(UInt32 x)
	{
		if(m_numFormat == NF_DEF)
			return gsU32H(x);
		else if(m_numFormat == NumberFormat::Hexadecimal)
			return gsU32H(x);
		else if(m_numFormat == NF_DEC)
			return gsU32D(x);
		else if(m_numFormat == NF_BIN)
			return gsU32B(x);
		else if(m_numFormat == NF_ASC)
		{
			return String::Concat("\"", 
				Convert::ToChar(GetHighByte(GetHighWord(x))).ToString(), 
				Convert::ToChar(GetLowByte(GetHighWord(x))).ToString(), 
				Convert::ToChar(GetHighByte(GetLowWord(x))).ToString(), 
				Convert::ToChar(GetLowByte(GetLowWord(x))).ToString(), 
				"\"");
		}
		else
		{
			return x.ToString();
		}
	}

	String^ NumToString::GetStrUInt64(UInt64 x)
	{
		if(m_numFormat == NumberFormat::Hexadecimal)
		{
			String^ prefix = "";
			if(m_HexShowLeadX)
				prefix = "0x";
			
			if(m_HexShowLeadZeros)
			{
				if(x < 0xF)
					prefix = String::Concat(prefix, "000000000000000");
				else if(x < 0xFF)
					prefix = String::Concat(prefix, "00000000000000");
				else if(x < 0xFFF)
					prefix = String::Concat(prefix, "0000000000000");
				else if(x < 0xFFFF)
					prefix = String::Concat(prefix, "000000000000");
				else if(x < 0xFFFFF)
					prefix = String::Concat(prefix, "00000000000");
				else if(x < 0xFFFFFF)
					prefix = String::Concat(prefix, "0000000000");
				else if(x < 0xFFFFFFF)
					prefix = String::Concat(prefix, "000000000");
				else if(x < 0xFFFFFFFF)
					prefix = String::Concat(prefix, "00000000");
				else if(x < 0xFFFFFFFFF)
					prefix = String::Concat(prefix, "0000000");
				else if(x < 0xFFFFFFFFFF)
					prefix = String::Concat(prefix, "000000");
				else if(x < 0xFFFFFFFFFFFF)
					prefix = String::Concat(prefix, "00000");
				else if(x < 0xFFFFFFFFFFFFF)
					prefix = String::Concat(prefix, "0000");
				else if(x < 0xFFFFFFFFFFFFFF)
					prefix = String::Concat(prefix, "000");
				else if(x < 0xFFFFFFFFFFFFFFF)
					prefix = String::Concat(prefix, "00");
				else if(x < 0xFFFFFFFFFFFFFFFF)
					prefix = String::Concat(prefix, "0");
			}

			if(m_HexCaps)
				return String::Concat(prefix, x.ToString("X", m_NFI));
			else
				return String::Concat(prefix, x.ToString("x", m_NFI));
		}
		else if(m_numFormat == NF_DEC)
			return gsU64D(x);
		else if(m_numFormat == NF_BIN)
			return gsU64B(x);
		else
			return x.ToString();
	}
	
	String^ NumToString::GetStrSByte(SByte x)
	{	
		if(m_numFormat == NF_DEF)
			return gsS08H(x);
		else if(m_numFormat == NumberFormat::Hexadecimal)
			return gsS08H(x);
		else if(m_numFormat == NumberFormat::Decimal)
			return gsS08D(x);
		else if(m_numFormat == NF_BIN)
			return gsS08B(x);
		else if(m_numFormat == NF_ASC)
			return gsS08A(x);
		else
			return x.ToString();
	}


	String^ NumToString::GetStrInt16(Int16 x)
	{
		if(m_numFormat == NF_DEF)
			return gsS16H(x);
		else if(m_numFormat == NumberFormat::Hexadecimal)
			return gsS16H(x);
		else if(m_numFormat == NumberFormat::Decimal)
			return gsS16D(x);
		else if(m_numFormat == NF_BIN)
			return gsS16B(x);
		else if(m_numFormat == NF_ASC)
			return x.ToString();
		
		return x.ToString();
	}

	String^ NumToString::GetStrInt32(Int32 x)
	{
		if(m_numFormat == NumberFormat::Hexadecimal)
			return gsS32H(x);
		else if(m_numFormat == NumberFormat::Decimal)
			return gsS32D(x);
		else if(m_numFormat == NumberFormat::Binary)
			return x.ToString();
		else if(m_numFormat == NumberFormat::Ascii)
			return x.ToString();

		return x.ToString();
	}

	String^ NumToString::GetStrInt64(Int64 x)
	{
		if(m_numFormat == NumberFormat::Hexadecimal)
			gsS64H(x);
		else if(m_numFormat == NumberFormat::Decimal)
			gsS64D(x);
		else if(m_numFormat == NumberFormat::Binary)
			return x.ToString();
		else if(m_numFormat == NumberFormat::Ascii)
			return x.ToString();

		return x.ToString();
	}

	String^ NumToString::GetStrGuid(Guid x)
	{
		if(m_numFormat == NumberFormat::Hexadecimal)
			return gsGuidH(x);
		else if(m_numFormat == NumberFormat::Decimal)
			return gsGuidD(x);
		else if(m_numFormat == NumberFormat::Binary)
			return x.ToString();
		else if(m_numFormat == NumberFormat::Ascii)
			return x.ToString();
		else
			return x.ToString();

	}

	String^ NumToString::AsciiBytesToString(array<Byte>^ asciiBytes)
	{
		int numNullChars = 0;
		bool moreNullChars = true;
		while(moreNullChars)
		{
			if((asciiBytes->Length - 1 - numNullChars) >= 0)
			{
				if(asciiBytes[asciiBytes->Length - 1 - numNullChars] == 0x00)
					numNullChars++;
				else
					moreNullChars = false;
			}
			else
				moreNullChars = false;
		}
		return Encoding::ASCII->GetString(asciiBytes, 0, asciiBytes->Length - numNullChars);
	}

	String^ NumToString::gsU08H(Byte x)
	{	
		String^ prefix = ""; 

		if(m_HexShowLeadX)
			prefix = "0x";
		
		Byte n1 = x / 16;
		Byte n2 = x % 16;

		array<String^>^ chars;

		if(m_HexCaps)
			chars = hexCharsU;
		else
			chars = hexCharsL;

		if(x < 16 && !m_HexShowLeadZeros)
			return String::Concat(prefix, chars[n2]);
		else
			return String::Concat(prefix, chars[n1], chars[n2]);
	}

	String^ NumToString::gsU16H(UInt16 x)
	{
		String^ prefix = ""; 

		if(m_HexShowLeadX)
			prefix = "0x";

		UInt16 t = x;
		Byte n1 = t % 16;
		t /= 16;
		Byte n2 = t % 16;
		t /= 16;
		Byte n3 = t % 16;
		t /= 16;
		Byte n4 = t % 16;
		if(m_HexCaps)
		{
			if(!m_HexShowLeadZeros)
			{
				if(x < 16)
					return String::Concat(prefix, hexCharsU[n1]); 
				else if(x < 256)
					return String::Concat(prefix, hexCharsU[n2], hexCharsU[n1]); 
				else if(x < 4096)
					return String::Concat(String::Concat(prefix, hexCharsU[n3]), String::Concat(hexCharsU[n2], hexCharsU[n1]));
				else
					return String::Concat(String::Concat(prefix, hexCharsU[n4]), String::Concat(hexCharsU[n3], hexCharsU[n2], hexCharsU[n1])); 
			}
			else
			{
				return String::Concat(String::Concat(prefix, hexCharsU[n4]), String::Concat(hexCharsU[n3], hexCharsU[n2], hexCharsU[n1])); 
			}
		}
		else
		{
			if(!m_HexShowLeadZeros)
			{
				if(x < 16)
					return String::Concat(prefix, hexCharsL[n1]); 
				else if(x < 256)
					return String::Concat(prefix, hexCharsL[n2], hexCharsL[n1]); 
				else if(x < 4096)
					return String::Concat(String::Concat(prefix, hexCharsL[n3]), String::Concat(hexCharsL[n2], hexCharsL[n1]));
				else
					return String::Concat(String::Concat(prefix, hexCharsL[n4]), String::Concat(hexCharsL[n3], hexCharsL[n2], hexCharsL[n1])); 
			}
			else
			{
				return String::Concat(String::Concat(prefix, hexCharsL[n4]), String::Concat(hexCharsL[n3], hexCharsL[n2], hexCharsL[n1])); 
			}
		}
	}

	String^ NumToString::gsU32H(UInt32 x)
	{
		String^ prefix = ""; 

		if(m_HexShowLeadX)
			prefix = "0x";

		if(m_HexShowLeadZeros)
		{
			if(x < 0x10)
				prefix = String::Concat(prefix, "0000000");
			else if(x < 0x100)
				prefix = String::Concat(prefix, "000000");
			else if(x < 0x1000)
				prefix = String::Concat(prefix, "00000");
			else if(x < 0x10000)
				prefix = String::Concat(prefix, "0000");
			else if(x < 0x100000)
				prefix = String::Concat(prefix, "000");
			else if(x < 0x1000000)
				prefix = String::Concat(prefix, "00");
			else if(x < 0100000000)
				prefix = String::Concat(prefix, "0");
		}

		if(m_HexCaps)
			return String::Concat(prefix, x.ToString("X", m_NFI));
		else
			return String::Concat(prefix, x.ToString("x", m_NFI));
	}

	String^ NumToString::gsU64H(UInt64 x)
	{
		String^ prefix = ""; 

		if(m_HexShowLeadX)
			prefix = "0x";

		if(m_HexCaps)
			return String::Concat(prefix, x.ToString("X", m_NFI));
		else
			return String::Concat(prefix, x.ToString("x", m_NFI));
	}

	String^ NumToString::gsS08H(SByte x)
	{
		String^ prefix = ""; 

		if(m_HexShowLeadX)
			prefix = "0x";

		if(m_HexShowLeadZeros && x >= 0)
		{
			if(x < 0x10)
				prefix = String::Concat(prefix, "0");
		}

		if(m_HexCaps)
			return String::Concat(prefix, x.ToString("X", m_NFI));
		else
			return String::Concat(prefix, x.ToString("x", m_NFI));
	}

	String^ NumToString::gsS16H(Int16 x)
	{
		String^ prefix = ""; 

		if(m_HexShowLeadX)
			prefix = "0x";

		if(m_HexShowLeadZeros && x >= 0)
		{
			if(x < 0xF)
				prefix = String::Concat(prefix, "000");
			else if(x < 0xFF)
				prefix = String::Concat(prefix, "00");
			else if(x < 0xFFF)
				prefix = String::Concat(prefix, "0");
		}

		if(m_HexCaps)
			return String::Concat(prefix, x.ToString("X", m_NFI));
		else
			return String::Concat(prefix, x.ToString("x", m_NFI));
	}

	String^ NumToString::gsS32H(Int32 x)
	{
		String^ prefix; 

		if(m_HexShowLeadX)
			prefix = "0x";
		else
			prefix = "";

		if(m_HexShowLeadZeros && x >= 0)
		{
			if(x < 0xF)
				prefix = String::Concat(prefix, "0000000");
			else if(x < 0xFF)
				prefix = String::Concat(prefix, "000000");
			else if(x < 0xFFF)
				prefix = String::Concat(prefix, "00000");
			else if(x < 0xFFFF)
				prefix = String::Concat(prefix, "0000");
			else if(x < 0xFFFFF)
				prefix = String::Concat(prefix, "000");
			else if(x < 0xFFFFFF)
				prefix = String::Concat(prefix, "00");
			else if(x < 0xFFFFFFF)
				prefix = String::Concat(prefix, "0");
		}

		if(m_HexCaps)
			return String::Concat(prefix, x.ToString("X", m_NFI));
		else
			return String::Concat(prefix, x.ToString("x", m_NFI));
	}


	String^ NumToString::gsS64H(Int64 x)
	{
		String^ prefix;

		if(m_HexShowLeadX)
			prefix = "0x";
		else
			prefix = "";

		if(m_HexCaps)
			return String::Concat(prefix, x.ToString("X", m_NFI));
		else
			return String::Concat(prefix, x.ToString("x", m_NFI));
	}

	String^ NumToString::gsASCH(const array<char>^ x)
	{
		StringBuilder^ sb = gcnew StringBuilder((x->Length * 3) + 2);
		bool oldShowLeadX = m_HexShowLeadX;
		m_HexShowLeadX = false;
		for(int i = 0; i < x->Length; i++)
		{
			sb->Append(gsS08H(x[i]));
			sb->Append(" ");
		}
		m_HexShowLeadX = oldShowLeadX;
		return sb->ToString();
	}

	String^ NumToString::gsU08B(Byte x)
	{
		array<Char>^ retVal = gcnew array<Char>(8);
		for(int i = 7; i >= 0; i--)
		{
			if(GETBIT(x, i))
				retVal[7 - i] = '1';
			else
				retVal[7 - i] = '0';
		}
		return gcnew String(retVal);
	}

	String^ NumToString::gsS08B(SByte x)
	{
		array<Char>^ retVal = gcnew array<Char>(8);
		for(int i = 7; i >= 0; i--)
		{
			if(GETBITS(x, i))
				retVal[7 - i] = '1';
			else
				retVal[7 - i] = '0';
		}
		return gcnew String(retVal);
	}

	String^ NumToString::gsASCA(const array<char>^ x)
	{
		StringBuilder^ result = gcnew StringBuilder(x->Length + 2);

		result->Append(DOUBLE_QUOTE);
		for(int i = 0; i < x->Length; i++)
		{
			result->Append(Convert::ToChar(x[i]));
		}
		result->Append(DOUBLE_QUOTE);

		return result->ToString();
	}
	
	String^ NumToString::gsASCD(const array<char>^ x) 
	{
		StringBuilder^ result = gcnew StringBuilder((int)((double)x->Length * 2.570313) + x->Length);

		for(int i = 0; i < x->Length; i++)
		{
			result->Append(x[i]);
			result->Append(" ");
		}

		return result->ToString();
	}

	String^ NumToString::gsGuidH(Guid x)
	{
		if(m_HexCaps)
			return x.ToString("X");
		else
			return x.ToString("x");
	}

	String^ NumToString::gsGuidD(Guid x)
	{
		StringBuilder^ sb = gcnew StringBuilder(1 + 10 + 1 + 5 + 1 + 5 + 1 + 5 + 1 + 15 + 1);
		sb->Append("{");
		sb->Append("    ");
		sb->Append("}");
		return sb->ToString();
	}


	String^ NumToString::GetBitStr(unsigned char b, unsigned char p)
	{
		if(((b & (unsigned char)(1 << p)) != 0)) 
			return "1"; 
		else 
			return "0";
	}

	// NEED TO FINISH
	String^ NumToString::PutCommas(String^ intString)
	{
		int len = intString->Length;
		
		// No commas are added in this case
		if(len <= 3)
		{
			return intString;
		}

		
		//int numCommas = (len - 1) / 3;
		//String* newString;



		return intString;
	}

};