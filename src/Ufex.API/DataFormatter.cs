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

	private HexNumberFormatter _hexNumberFormatter;
	private BinaryNumberFormatter binaryNumberFormatter;
	private NumberFormat numFormat;
	private Endian endian;
	private CultureInfo cultureInfo;
	private NumberFormatInfo numberFormatInfo;
	private string _arrayPrefix = "[";
	private string _arraySuffix = "]";
	private string _emptyArray = "[]";

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
			_hexNumberFormatter.Endian = value;
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
			_hexNumberFormatter.LeadX = value;
		}
	}

	public bool HexCaps
	{
		get { return hexCaps; }
		set 
		{ 
			hexCaps = value;
			_hexNumberFormatter.Caps = value;
		}
	}

	public bool HexLeadZeros
	{
		get { return hexShowLeadZeros;  }
		set
		{
			hexShowLeadZeros = value;
			_hexNumberFormatter.LeadZeros = value;
		}
	}

	public bool DecimalCommas
	{
		get { return decimalCommas; }
		set { decimalCommas = value; }
	}

	public string ArrayPrefix
	{
		get { return _arrayPrefix; }
		set 
		{ 
			_arrayPrefix = value;
			_emptyArray = _arrayPrefix + _arraySuffix;
		}
	}

	public string ArraySuffix
	{
		get { return _arraySuffix; }
		set 
		{ 
			_arraySuffix = value; 
			_emptyArray = _arrayPrefix + _arraySuffix; 
		}
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
		_hexNumberFormatter = new HexNumberFormatter(hexCaps, hexShowLeadX, hexShowLeadZeros, this.Endian);
		_hexNumberFormatter.NFI = numberFormatInfo;
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
		_hexNumberFormatter.LeadX = leadX; 
		_hexNumberFormatter.LeadZeros = zeros; 
		_hexNumberFormatter.Caps = caps;
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
			return _hexNumberFormatter.UInt8(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.UInt8(x);
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
			return _hexNumberFormatter.UInt16(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.UInt16(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.UInt16(x);
		else if (numFormat == NumberFormat.Ascii)
			return "\"" + Convert.ToChar(ByteUtil.GetHighWord(x)).ToString() + Convert.ToChar(ByteUtil.GetLowWord(x)).ToString() + "\"";
		else
			return x.ToString();
	}

	public string UInt32(UInt32 x)
	{
		if (numFormat == NumberFormat.Default)
			return _hexNumberFormatter.UInt32(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.UInt32(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.UInt32(x);
		else if (numFormat == NumberFormat.Ascii)
			return "\"" + 
				Convert.ToChar(ByteUtil.GetHighByte(ByteUtil.GetHighWord(x))).ToString() +
				Convert.ToChar(ByteUtil.GetLowByte(ByteUtil.GetHighWord(x))).ToString() +
				Convert.ToChar(ByteUtil.GetHighByte(ByteUtil.GetLowWord(x))).ToString() +
				Convert.ToChar(ByteUtil.GetLowByte(ByteUtil.GetLowWord(x))).ToString() + "\"";
		else
			return x.ToString();
	}

	public string UInt64(UInt64 x)
	{
		if (numFormat == NumberFormat.Default)
			return _hexNumberFormatter.UInt64(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.UInt64(x);
		else if (numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (numFormat == NumberFormat.Binary)
			return binaryNumberFormatter.UInt64(x);
		else if (numFormat == NumberFormat.Ascii)
			return "\"" +
				Convert.ToChar(ByteUtil.GetByte(x, 0)) +
				Convert.ToChar(ByteUtil.GetByte(x, 1)) +
				Convert.ToChar(ByteUtil.GetByte(x, 2)) +
				Convert.ToChar(ByteUtil.GetByte(x, 3)) +
				Convert.ToChar(ByteUtil.GetByte(x, 4)) +
				Convert.ToChar(ByteUtil.GetByte(x, 5)) +
				Convert.ToChar(ByteUtil.GetByte(x, 6)) +
				Convert.ToChar(ByteUtil.GetByte(x, 7)) + "\"";
		else
			return x.ToString();
	}

	public string SByte(SByte x)
	{
		if (numFormat == NumberFormat.Default)
			return _hexNumberFormatter.SInt8(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.SInt8(x);
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
			return _hexNumberFormatter.SInt16(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.SInt16(x);
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
			return _hexNumberFormatter.SInt32(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.SInt32(x);
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
			return _hexNumberFormatter.SInt64(x);
		else if (numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.SInt64(x);
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
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.Object(n))) + _arraySuffix;
	}

	public string ByteArray(Byte[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.Byte(n))) + _arraySuffix;
	}
	public string UInt16Array(UInt16[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.UInt16(n))) + _arraySuffix;
	}
	public string UInt32Array(UInt32[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.UInt32(n))) + _arraySuffix;
	}
	public string UInt64Array(UInt64[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.UInt64(n))) + _arraySuffix;
	}

	public string SByteArray(SByte[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.SByte(n))) + _arraySuffix;
	}
	public string Int16Array(Int16[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.Int16(n))) + _arraySuffix;
	}
	public string Int32Array(Int32[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.Int32(n))) + _arraySuffix;
	}
	public string Int64Array(Int64[] x)
	{
		if (x == null)
			return NULL_STRING;

		if (x.Length == 0)
			return _emptyArray;

		return _arrayPrefix + string.Join(", ", x.Select(n => this.Int64(n))) + _arraySuffix;
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
/*
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
