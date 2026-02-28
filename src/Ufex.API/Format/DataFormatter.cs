using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ufex.API.Format;

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
	private BinaryNumberFormatter _binaryNumberFormatter;
	private NumberFormat _numFormat;
	private Endian _endian;
	private CultureInfo _cultureInfo;
	private NumberFormatInfo _numberFormatInfo;
	private string _arrayPrefix = "[";
	private string _arraySuffix = "]";
	private string _emptyArray = "[]";

	// Format specific options
	private bool _hexShowLeadX;
	private bool _hexShowLeadZeros;
	private bool _hexCaps;
	private bool _decimalCommas;

	/// <summary>
	/// NumberFormat to use for formatting numbers.
	/// </summary>
	public NumberFormat NumFormat
	{
		get { return _numFormat; }
		set { _numFormat = value; }
	}

	public Endian Endian
	{
		get { return _endian; }
		set 
		{ 
			_endian = value;
			_hexNumberFormatter.Endian = value;
			_binaryNumberFormatter.Endian = value;
		}
	}

	/// <summary>
	/// Whether to show the "0x" prefix for hexadecimal numbers.
	/// </summary>
	public bool HexShowLead0X
	{
		get
		{
			return _hexShowLeadX;
		}
		set 
		{ 
			_hexShowLeadX = value;
			_hexNumberFormatter.LeadX = value;
		}
	}

	/// <summary>
	/// Whether to use uppercase letters for hexadecimal letters.
	/// </summary>
	public bool HexCaps
	{
		get { return _hexCaps; }
		set 
		{ 
			_hexCaps = value;
			_hexNumberFormatter.Caps = value;
		}
	}

	public bool HexLeadZeros
	{
		get { return _hexShowLeadZeros;  }
		set
		{
			_hexShowLeadZeros = value;
			_hexNumberFormatter.LeadZeros = value;
		}
	}

	public bool DecimalCommas
	{
		get { return _decimalCommas; }
		set { _decimalCommas = value; }
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
		_cultureInfo = new CultureInfo(locale, false);
		_numberFormatInfo = _cultureInfo.NumberFormat;

		// Set the seperator
		_numberFormatInfo.NumberGroupSeparator = ",";
		_numberFormatInfo.NumberDecimalDigits = 0;

		// Default Options
		_hexShowLeadX = true;
		_hexShowLeadZeros = true;
		_hexCaps = true;
		_decimalCommas = false;
		_binaryNumberFormatter = new BinaryNumberFormatter(true, this.Endian);
		_hexNumberFormatter = new HexNumberFormatter(_hexCaps, _hexShowLeadX, _hexShowLeadZeros, this.Endian);
		_hexNumberFormatter.NFI = _numberFormatInfo;
	}

	[Obsolete]
	public void SetHexFormat(bool leadX, bool zeros, bool caps) 
	{ 
		_hexNumberFormatter.LeadX = leadX; 
		_hexNumberFormatter.LeadZeros = zeros; 
		_hexNumberFormatter.Caps = caps;
	}

	private string Base10(IFormattable n)
	{
		return n.ToString(_decimalCommas ? "N" : "", _numberFormatInfo);
	}

	public string Object(Object x) => x switch
	{
		string s => s,
		// Integral types
		byte b => Byte(b),
		ushort us => UInt16(us),
		uint ui => UInt32(ui),
		ulong ul => UInt64(ul),
		sbyte sb => SByte(sb),
		short s => Int16(s),
		int i => Int32(i),
		long l => Int64(l),
		bool bo => Bool(bo),
		// Arrays
		object[] oa => ObjectArray(oa),
		byte[] ba => ByteArray(ba),
		ushort[] usa => UInt16Array(usa),
		uint[] uia => UInt32Array(uia),
		sbyte[] sba => SByteArray(sba),
		// Display file offsets as 32-bit numbers unless the offset is large
		FileOffset fo => fo.Value > 0x99999999 ? Int64(fo.Value) : UInt32((uint)fo.Value),
		_ => x.ToString()
	};

	public string Bool(bool x)
	{
		if (_numFormat == NumberFormat.Default)
			return x ? "True" : "False";
		else if (_numFormat == NumberFormat.Hexadecimal)
			return x ? "0x1" : "0x0";
		else if (_numFormat == NumberFormat.Decimal)
			return x ? "1" : "0";
		else if (_numFormat == NumberFormat.Binary)
			return x ? "1" : "0";
		else
			return x ? "True" : "False";
	}

	public string Byte(Byte x) {
		if (_numFormat == NumberFormat.Default)
			return _hexNumberFormatter.UInt8(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.UInt8(x);
		else if (_numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (_numFormat == NumberFormat.Binary)
			return _binaryNumberFormatter.UInt8(x);
		else if (_numFormat == NumberFormat.Ascii)
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
		if (_numFormat == NumberFormat.Default)
			return _hexNumberFormatter.UInt16(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.UInt16(x);
		else if (_numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (_numFormat == NumberFormat.Binary)
			return _binaryNumberFormatter.UInt16(x);
		else if (_numFormat == NumberFormat.Ascii)
			return "\"" + Convert.ToChar(ByteUtil.GetHighWord(x)).ToString() + Convert.ToChar(ByteUtil.GetLowWord(x)).ToString() + "\"";
		else
			return x.ToString();
	}

	public string UInt32(UInt32 x)
	{
		if (_numFormat == NumberFormat.Default)
			return _hexNumberFormatter.UInt32(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.UInt32(x);
		else if (_numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (_numFormat == NumberFormat.Binary)
			return _binaryNumberFormatter.UInt32(x);
		else if (_numFormat == NumberFormat.Ascii)
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
		if (_numFormat == NumberFormat.Default)
			return _hexNumberFormatter.UInt64(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.UInt64(x);
		else if (_numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (_numFormat == NumberFormat.Binary)
			return _binaryNumberFormatter.UInt64(x);
		else if (_numFormat == NumberFormat.Ascii)
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
		if (_numFormat == NumberFormat.Default)
			return _hexNumberFormatter.SInt8(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.SInt8(x);
		else if (_numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (_numFormat == NumberFormat.Binary)
			return _binaryNumberFormatter.SInt8(x);
		else if (_numFormat == NumberFormat.Ascii)
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
		if (_numFormat == NumberFormat.Default)
			return _hexNumberFormatter.SInt16(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.SInt16(x);
		else if (_numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (_numFormat == NumberFormat.Binary)
			return _binaryNumberFormatter.SInt16(x);
		else if (_numFormat == NumberFormat.Ascii)
			return "-";
		else
			return x.ToString();
	}

	public string Int32(Int32 x)
	{
		if (_numFormat == NumberFormat.Default)
			return _hexNumberFormatter.SInt32(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.SInt32(x);
		else if (_numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (_numFormat == NumberFormat.Binary)
			return _binaryNumberFormatter.SInt32(x);
		else if (_numFormat == NumberFormat.Ascii)
			return "-";
		else
			return x.ToString();
	}

	public string Int64(Int64 x)
	{
		if (_numFormat == NumberFormat.Default)
			return _hexNumberFormatter.SInt64(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return _hexNumberFormatter.SInt64(x);
		else if (_numFormat == NumberFormat.Decimal)
			return Base10(x);
		else if (_numFormat == NumberFormat.Binary)
			return _binaryNumberFormatter.SInt64(x);
		else if (_numFormat == NumberFormat.Ascii)
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
		if (_numFormat == NumberFormat.Default)
			return gsASCA(x);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return gsASCH(x);
		else if (_numFormat == NumberFormat.Decimal)
			return gsASCD(x);
		else if (_numFormat == NumberFormat.Binary)
			return gsASCA(x);
		else if (_numFormat == NumberFormat.Ascii)
			return gsASCA(x);
		else
			return x.ToString();
	}

	public string ASCIIString(sbyte[] x)
	{
		Byte[] y = x.Select(n => (Byte)n).ToArray();
		if (_numFormat == NumberFormat.Default)
			return gsASCA(y);
		else if (_numFormat == NumberFormat.Hexadecimal)
			return gsASCH(y);
		else if (_numFormat == NumberFormat.Decimal)
			return gsASCD(y);
		else if (_numFormat == NumberFormat.Binary)
			return gsASCA(y);
		else if (_numFormat == NumberFormat.Ascii)
			return gsASCA(y);
		else
			return x.ToString();
	}

	public string ASCIIString(string x)
	{
		if (_numFormat == NumberFormat.Default)
			return '"' + x + '"';
		else if (_numFormat == NumberFormat.Hexadecimal)
			return gsASCH(Encoding.ASCII.GetBytes(x));
		else if (_numFormat == NumberFormat.Decimal)
			return gsASCD(Encoding.ASCII.GetBytes(x));
		else if (_numFormat == NumberFormat.Binary)
			return gsASCA(Encoding.ASCII.GetBytes(x));
		else if (_numFormat == NumberFormat.Ascii)
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
		HexNumberFormatter f = new HexNumberFormatter(_hexCaps, false, true);
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
