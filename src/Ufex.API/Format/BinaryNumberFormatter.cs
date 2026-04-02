using System;
using Ufex.API.Types;

namespace Ufex.API.Format;

/// <summary>
/// Formats binary numbers as strings of 0s and 1s, with options for leading zeros and endianness.
/// </summary>
public class BinaryNumberFormatter
{
	private bool leadZeros = true;
	private Endian endian = Endian.Big;

	public bool LeadZeros
	{
		get { return leadZeros; }
		set { leadZeros = value; }
	}

	public Endian Endian
	{
		get { return endian; }
		set { endian = value; }
	}

	public BinaryNumberFormatter(bool leadZeros = true, Endian endian = Endian.Big)
	{
		LeadZeros = leadZeros;
		Endian = endian;
	}

	/// <summary>
	/// Pads the binary string with leading zeros if the leadZeros option is set, otherwise returns the string as is.
	/// </summary>
	/// <param name="s">The binary string to pad.</param>
	/// <param name="n">The total length of the resulting string.</param>
	/// <returns>The padded binary string.</returns>
	private string Pad(string s, int n)
	{
		return leadZeros ? s.PadLeft(n, '0') : s;
	}

	/// <summary>
	/// Formats an unsigned 8-bit integer as a binary string, with optional leading zeros.
	/// </summary>
	/// <param name="x">The unsigned 8-bit integer to format.</param>
	/// <returns>The binary string representation of the integer.</returns>
	public string UInt8(Byte x)
	{
		return Pad(Convert.ToString(x, 2), 8);
	}

	public string UInt16(UInt16 x)
	{
		if (endian == Endian.Little)
		{
			x = ByteUtil.SwapEndian(x);
		}
		return Pad(Convert.ToString(x, 2), 16);
	}

	public string UInt24(UInt24 x)
	{
		if (endian == Endian.Little)
		{
			x = ByteUtil.SwapEndian(x);
		}
		return Pad(Convert.ToString(x, 2), 24);
	}

	public string UInt32(UInt32 x)
	{
		if (endian == Endian.Little)
		{
			x = ByteUtil.SwapEndian(x);
		}
		return Pad(Convert.ToString(x, 2), 32);
	}

	public string UInt64(UInt64 x)
	{
		if (endian == Endian.Little)
		{
			x = ByteUtil.SwapEndian(x);
		}
		UInt32 low = (UInt32)(x & 0xFFFFFFFFUL);
		UInt32 high = (UInt32)(x >> 32);
		return $"{Convert.ToString(high, 2).PadLeft(32, '0')}{Convert.ToString(low, 2).PadLeft(32, '0')}";
	}

	public string SInt8(SByte x)
	{
		string bits = Convert.ToString((Byte)x, 2);
		return leadZeros ? bits.PadLeft(8, '0') : bits;
	}

	public string SInt16(Int16 x)
	{
		if (endian == Endian.Little)
		{
			x = ByteUtil.SwapEndian(x);
		}
		return Pad(Convert.ToString(x, 2), 16);
	}

	public string SInt32(Int32 x)
	{
		if (endian == Endian.Little)
		{
			x = ByteUtil.SwapEndian(x);
		}
		return Pad(Convert.ToString(x, 2), 32);
	}

	public string SInt64(Int64 x)
	{
		if (endian == Endian.Little)
		{
			x = ByteUtil.SwapEndian(x);
		}
		return Pad(Convert.ToString(x, 2), 64);
	}

}