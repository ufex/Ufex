using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ufex.API;

public enum NumberFormat : int
{
	Default = 0x0000,
	/// <summary>
	/// Binary Format - displays numbers in base 2.
	/// </summary>
	Binary = 0x0001,
	/// <summary>
	/// Octal Format - displays numbers in base 8.
	/// </summary>
	Octal = 0x0008,
	/// <summary>
	/// Decimal Format - displays numbers in base 10.
	/// </summary>
	Decimal = 0x000A,
	/// <summary>
	/// Hexadecimal Format - displays numbers in base 16.
	/// </summary>
	Hexadecimal = 0x0010,
	/// <summary>
	/// ASCII Format - displays numbers as their ASCII character equivalents.
	/// </summary>
	Ascii = 0x00F0,
	Unicode = 0x00F1
}

public sealed class DataFormatter
{
	const string NULL_STRING = "NULL";

	private HexNumberFormatter hexNumberFormatter;
	private BinaryNumberFormatter binaryNumberFormatter;
	private NumberFormat numFormat;
	private Endian endian;
	private CultureInfo cultureInfo;
	private NumberFormatInfo numberFormatInfo;

	// Format specific options
	private bool hexShowLeadX;
	private bool hexShowLeadZeros;
	private bool hexCaps;
	bool decimalCommas;

	public NumberFormat NumFormat
	{
		get { return numFormat; }
		set { numFormat = value; }
	}

	public Endian Endian
	{
		get { return endian; }
		set 
		{ 
			endian = value;
			hexNumberFormatter.Endian = value;
			binaryNumberFormatter.Endian = value;
		}
	}

	public bool HexShowLead0X
	{
		get
		{
			return hexShowLeadX;
		}
		set 
		{ 
			hexShowLeadX = value;
			hexNumberFormatter.LeadX = value;
		}
	}

	public bool HexCaps
	{
		get { return hexCaps; }
		set 
		{ 
			hexCaps = value;
			hexNumberFormatter.Caps = value;
		}
	}

	public bool HexLeadZeros
	{
		get { return hexShowLeadZeros;  }
		set
		{
			hexShowLeadZeros = value;
			hexNumberFormatter.LeadZeros = value;
		}
	}

	public bool DecimalCommas
	{
		get { return decimalCommas; }
		set { decimalCommas = value; }
	}

	public DataFormatter(string locale = "en-US")
	{
		cultureInfo = new CultureInfo(locale, false);
		numberFormatInfo = cultureInfo.NumberFormat;

		// Set the seperator
		numberFormatInfo.NumberGroupSeparator = ",";
		numberFormatInfo.NumberDecimalDigits = 0;

		// Default Options
		hexShowLeadX = true;
		hexShowLeadZeros = true;
		hexCaps = true;
		decimalCommas = false;
		binaryNumberFormatter = new BinaryNumberFormatter(true, this.Endian);
		hexNumberFormatter = new HexNumberFormatter(hexCaps, hexShowLeadX, hexShowLeadZeros, this.Endian);
		hexNumberFormatter.NFI = numberFormatInfo;
	}

	[Obsolete]
	public void SetNumFormat(NumberFormat format) 
	{ 
		numFormat = format; 
	}
	
	[Obsolete]
	public void SetEndian(Endian endian)
	{ 
		this.endian = endian;
	}

	[Obsolete]
	public void SetHexFormat(bool leadX, bool zeros, bool caps) 
	{ 
		hexNumberFormatter.LeadX = leadX; 
		hexNumberFormatter.LeadZeros = zeros; 
		hexNumberFormatter.Caps = caps;
	}

	[Obsolete]
	public void SetDecFormat(bool commas) 
	{ 
		decimalCommas = commas; 
	}

	private string Base10(IFormattable n)
	{
		return n.ToString(decimalCommas ? "N" : "", numberFormatInfo);
	}

	public string Object(Object x)
	{
		Type t = x.GetType();
		if (t.Equals(typeof(String)))
			return (string)x;
		else if (t.Equals(typeof(Byte)))
			return this.Byte((Byte)x);
		else if (t.Equals(typeof(UInt16)))
			return this.UInt16((UInt16)x);
		else if (t.Equals(typeof(UInt32)))
			return this.UInt32((UInt32)x);
		else if (t.Equals(typeof(UInt64)))
			return this.UInt64((UInt64)x);
		else if (t.Equals(typeof(SByte)))
			return this.SByte((SByte)x);
		else if (t.Equals(typeof(Int16)))
			return this.Int16((Int16)x);
		else if (t.Equals(typeof(Int32)))
			return Int32((Int32)x);
		else if (t.Equals(typeof(Int64)))
			return this.Int64((Int64)x);
		else if (t.Equals(typeof(Boolean)))
			return this.Bool((Boolean)x);
		else if (t.Equals(typeof(Object).MakePointerType().MakeArrayType()))
			return this.ObjectArray((Object[])x);
		else if (t.Equals(typeof(Byte).MakeArrayType()))
			return this.ByteArray((Byte[])x);
		else if (t.Equals(typeof(UInt16).MakeArrayType()))
			return this.UInt16Array((UInt16[])x);
		else if (t.Equals(typeof(UInt32).MakeArrayType()))
			return this.UInt32Array((UInt32[])x);
		else if (t.Equals(typeof(SByte).MakeArrayType()))
			return this.SByteArray((SByte[])x);
		else if (t.Equals(typeof(Guid)))
			return x.ToString();
		else
			return x.ToString();
	}

	public string Bool(bool x)
	{
		if (numFormat == NumberFormat.Default)
			return x ? "True" : "False";
		else if (numFormat == NumberFormat.Hexadecimal)
			return x ? "0x1" : "0x0";
		else if (numFormat == NumberFormat.Decimal)
			return x ? "1" : "0";
		else if (numFormat == NumberFormat.Binary)
			return x ? "1" : "0";
		else
			return x ? "True" : "False";
	}

	public string Byte(Byte x) {
		if (numFormat == NumberFormat.Default)
			return hexNumberFormatter.UInt8(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return hexNumberFormatter.UInt8(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.UInt8(x);
		else if (numFormat == NumberFormat.Ascii)
			return "\'" + Convert.ToChar(x).ToString() + "\'";
		else
			return x.ToString();
	}

	public string UInt8(Byte x)
	{
		return this.Byte(x);
	}
	
	public string UInt16(UInt16 x)
	{
		if (numFormat == NumberFormat.Default)
			return hexNumberFormatter.UInt16(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return hexNumberFormatter.UInt16(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.UInt16(x);
		else if (numFormat == NumberFormat.Ascii)
			return "\"" + Convert.ToChar(DataManip.GetHighWord(x)).ToString() + Convert.ToChar(DataManip.GetLowWord(x)).ToString() + "\"";
		else
			return x.ToString();
	}

	public string UInt32(UInt32 x)
	{
		if (numFormat == NumberFormat.Default)
			return hexNumberFormatter.UInt32(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return hexNumberFormatter.UInt32(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.UInt32(x);
		else if (numFormat == NumberFormat.Ascii)
			return "\"" + 
				Convert.ToChar(DataManip.GetHighByte(DataManip.GetHighWord(x))).ToString() +
				Convert.ToChar(DataManip.GetLowByte(DataManip.GetHighWord(x))).ToString() +
				Convert.ToChar(DataManip.GetHighByte(DataManip.GetLowWord(x))).ToString() +
				Convert.ToChar(DataManip.GetLowByte(DataManip.GetLowWord(x))).ToString() + "\"";
		else
			return x.ToString();
	}

	public string UInt64(UInt64 x)
	{
		if (numFormat == NumberFormat.Default)
			return hexNumberFormatter.UInt64(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return hexNumberFormatter.UInt64(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.UInt64(x);
		else if (numFormat == NumberFormat.Ascii)
			return "\"" +
				Convert.ToChar(DataManip.GetByte(x, 0)) +
				Convert.ToChar(DataManip.GetByte(x, 1)) +
				Convert.ToChar(DataManip.GetByte(x, 2)) +
				Convert.ToChar(DataManip.GetByte(x, 3)) +
				Convert.ToChar(DataManip.GetByte(x, 4)) +
				Convert.ToChar(DataManip.GetByte(x, 5)) +
				Convert.ToChar(DataManip.GetByte(x, 6)) +
				Convert.ToChar(DataManip.GetByte(x, 7)) + "\"";
		else
			return x.ToString();
	}

	public string SByte(SByte x)
	{
		if (numFormat == NumberFormat.Default)
			return hexNumberFormatter.SInt8(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return hexNumberFormatter.SInt8(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.SInt8(x);
		else if (numFormat == NumberFormat.Ascii)
			return "\'" + Convert.ToChar(x).ToString() + "\'";
		else
			return x.ToString();
	}

	public string Int8(SByte x)
	{
		return this.SByte(x);
	}

	public string Int16(Int16 x)
	{
		if (numFormat == NumberFormat.Default)
			return hexNumberFormatter.SInt16(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return hexNumberFormatter.SInt16(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.SInt16(x);
		else if (numFormat == NumberFormat.Ascii)
			return "-";
		else
			return x.ToString();
	}

	public string Int32(Int32 x)
	{
		if (numFormat == NumberFormat.Default)
			return hexNumberFormatter.SInt32(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return hexNumberFormatter.SInt32(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.SInt32(x);
		else if (numFormat == NumberFormat.Ascii)
			return "-";
		else
			return x.ToString();
	}

	public string Int64(Int64 x)
	{
		if (numFormat == NumberFormat.Default)
			return hexNumberFormatter.SInt64(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return hexNumberFormatter.SInt64(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.SInt64(x);
		else if (numFormat == NumberFormat.Ascii)
			return "-";
		else
			return x.ToString();
	}
	public string ObjectArray(Object[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.Object(n))) + "}";
	}

	public string ByteArray(Byte[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.Byte(n))) + "}";
	}
	public string UInt16Array(UInt16[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.UInt16(n))) + "}";
	}
	public string UInt32Array(UInt32[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.UInt32(n))) + "}";
	}
	public string UInt64Array(UInt64[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.UInt64(n))) + "}";
	}

	public string SByteArray(SByte[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.SByte(n))) + "}";
	}
	public string Int16Array(Int16[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.Int16(n))) + "}";
	}
	public string Int32Array(Int32[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.Int32(n))) + "}";
	}
	public string Int64Array(Int64[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return "{}";

		return "{" + string.Join(", ", x.Select(n => this.Int64(n))) + "}";
	}

	public string ASCIIString(byte[] x)
	{
		if (numFormat == NumberFormat.Default)
			return gsASCA(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return gsASCH(x);
		else if (numFormat == NumberFormat.Decimal)
			return gsASCD(x);
		else if (numFormat == NumberFormat.Binary)
			return gsASCA(x);
		else if (numFormat == NumberFormat.Ascii)
			return gsASCA(x);
		else
			return x.ToString();
	}

	public string ASCIIString(sbyte[] x)
	{
		Byte[] y = x.Select(n => (Byte)n).ToArray();
		if (numFormat == NumberFormat.Default)
			return gsASCA(y);
		else if (numFormat == NumberFormat.Hexadecimal)
			return gsASCH(y);
		else if (numFormat == NumberFormat.Decimal)
			return gsASCD(y);
		else if (numFormat == NumberFormat.Binary)
			return gsASCA(y);
		else if (numFormat == NumberFormat.Ascii)
			return gsASCA(y);
		else
			return x.ToString();
	}

	public string ASCIIString(string x)
	{
		if (numFormat == NumberFormat.Default)
			return '"' + x + '"';
		else if (numFormat == NumberFormat.Hexadecimal)
			return gsASCH(Encoding.ASCII.GetBytes(x));
		else if (numFormat == NumberFormat.Decimal)
			return gsASCD(Encoding.ASCII.GetBytes(x));
		else if (numFormat == NumberFormat.Binary)
			return gsASCA(Encoding.ASCII.GetBytes(x));
		else if (numFormat == NumberFormat.Ascii)
			return gsASCA(Encoding.ASCII.GetBytes(x));
		else
			return x.ToString();
	}

	private string gsASCA(byte[] x)
	{
		StringBuilder result = new StringBuilder(x.Length + 2);
		result.Append('"');
		for(int i = 0; i < x.Length; i++)
		{
			result.Append(Convert.ToChar(x[i]));
		}
		result.Append('"');
		return result.ToString();
	}

	private string gsASCH(byte[] x)
	{
		StringBuilder sb = new StringBuilder((x.Length * 3) + 2);
		HexNumberFormatter f = new HexNumberFormatter(hexCaps, false, true);
		for(int i = 0; i < x.Length; i++)
		{
			sb.Append(f.UInt8(x[i]));
			sb.Append(" ");
		}
		return sb.ToString().TrimEnd();
	}

	private string gsASCD(byte[] x) 
	{
		StringBuilder result = new StringBuilder((int)((double)x.Length * 2.570313) + x.Length);
		for (int i = 0; i < x.Length; i++)
		{
			result.Append(x[i]);
			result.Append(" ");
		}
		return result.ToString().TrimEnd();
	}

	[Obsolete]
	public string GetStrObject(Object x) { return this.Object(x); }
	[Obsolete]
	public string GetStrBool(Boolean x) { return this.Bool(x); }
	[Obsolete]
	public string GetStrU8Array(byte[] x) { return this.ByteArray(x); }
	[Obsolete]
	public string GetStrU16Array(UInt16[] x) { return this.UInt16Array(x); }
	[Obsolete]
	public string GetStrU32Array(UInt32[] x) { return this.UInt32Array(x); }

	[Obsolete]
	public string GetStrU64Array(UInt64[] x) {  return this.UInt64Array(x); }

	//[Obsolete]
	//public string GetStrGuid(Guid x) {  return this.Gu}
	[Obsolete]
	public string GetStrByte(Byte x) { return this.Byte(x); }
	[Obsolete]
	public string GetStrUInt8(Byte x) { return this.Byte(x); }
	[Obsolete]
	public string GetStrUInt16(UInt16 x) { return this.UInt16(x); }
	[Obsolete]
	public string GetStrUInt32(UInt32 x) { return this.UInt32(x); }
	[Obsolete]
	public string GetStrUInt64(UInt64 x) { return this.UInt64(x); }
	[Obsolete]
	public string GetStrInt8(SByte x) { return this.SByte(x); }
	[Obsolete]
	public string GetStrInt16(Int16 x) { return this.Int16(x); }
	[Obsolete]
	public string GetStrInt32(Int32 x) { return this.Int32(x); }
	[Obsolete]
	public string GetStrInt64(Int64 x) { return this.Int64(x); }

	/*

String^ GetStrGuid(Guid x);

String^ GetStrASCIIString(array<char>^ x);
String^ GetStrCharArray(array<char>^ x);

String^ GetStrObjectArray(array<Object^>^ x);

// Converts a null terminated string in bytes to a unicode string
static String^ AsciiBytesToString(array<Byte>^ asciiBytes);
//
// Hex Conversion Functions
//
private String^ gsU08H(Byte x);
private String^ gsU16H(UInt16 x);
private String^ gsU32H(UInt32 x);
private String^ gsU64H(UInt64 x);

private String^ gsS08H(SByte x);
String^ gsS16H(Int16 x);
String^ gsS32H(Int32 x);
String^ gsS64H(Int64 x);

String^ gsASCH(const array<char>^ x);

String^ gsGuidH(Guid x);

//
// Decimal Conversion Functions
//
inline String^ gsU64D(UInt64 x) { if (!decimalCommas) return x.ToString("", numberFormatInfo); else return x.ToString("N", numberFormatInfo); };

inline String^ gsS08D(SByte x) { return x.ToString("", numberFormatInfo); };
inline String^ gsS16D(Int16 x) { if (!decimalCommas) return x.ToString("", numberFormatInfo); else return x.ToString("N", numberFormatInfo); };
inline String^ gsS32D(Int32 x) { if (!decimalCommas) return x.ToString("", numberFormatInfo); else return x.ToString("N", numberFormatInfo); };
inline String^ gsS64D(Int64 x) { if (!decimalCommas) return x.ToString("", numberFormatInfo); else return x.ToString("N", numberFormatInfo); };
String^ gsASCD(const array<char>^ x);

//
// Binary Conversion Functions
//
String^ gsU08B(const Byte x);

inline String^ gsU16B(UInt16 x) { return String::Concat(gsU08B(GetHighByte(x)), gsU08B(GetLowByte(x))); };
inline String^ gsU32B(UInt32 x) { return String::Concat(gsU16B(GetHighWord(x)), gsU16B(GetLowWord(x))); };
inline String^ gsU64B(UInt64 x) { return String::Concat(gsU32B(GetHighDword(x)), gsU32B(GetLowDword(x))); };

String^ gsS08B(SByte x);
inline String^ gsS16B(Int16 x) { return String::Concat(gsS08B(GH_S16(x)), gsS08B(GL_S16(x))); };

//
// ASCII Conversion Functions
//
inline String^ gsU08A(Byte x) { return String::Concat("\'", Convert::ToChar(x).ToString(), "\'"); };
inline String^ gsU16A(UInt16 x) { return String::Concat(DOUBLE_QUOTE, Convert::ToChar(GetHighWord(x)).ToString(), Convert::ToChar(GetLowWord(x)).ToString(), DOUBLE_QUOTE); };


inline String^ gsS16A(Int16 x) { return String::Concat(DOUBLE_QUOTE, Convert::ToChar(GH_S16(x)).ToString(), Convert::ToChar(GL_S16(x)).ToString(), DOUBLE_QUOTE); };

String^ gsASCA(const array<char>^ x);


String^ GetBitStr(unsigned char b, unsigned char p);

// Adds commas to an integer string
String^ PutCommas(String^ intString);


inline String^ GetPre1(bool s) { if (s) return "0x"; else return ""; }
inline String^ GetPre2(Byte x, bool y) { if (x < 16 && y) return "0"; else return ""; }

static array<String^>^ decChars  = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

static String ^SINGLE_QUOTE = "\'";
static String ^DOUBLE_QUOTE = "\"";

static array<String^> ^ASCIICHARS = {
"\\0", "\\x01", "\\x02", "\\x03", "\\x04", "\\x05", "", "\\a", "", "\\t", "\\n", "\\v", "\\f", "", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
									" ", "!", "\"", "#", "$", "%", "&", "'", "(", ")", "*", "+", ",", "-", ".", "", 
									"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":", ";", "<", "=", ">", "?", 
									"@", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", 
									"P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "[", "\\", "]", "^", "_",  
									"'", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", 
									"p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "{", "|", "}", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
									"", "", "", "", "", "", "", "", "", "", "", "", "", "ý", "þ", "ÿ" };

	*/

}
