using System;

namespace Ufex.API;
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
		this.LeadZeros = leadZeros;
		this.Endian = endian;
	}

	private string Pad(string s, int n)
	{
		return leadZeros ? s.PadLeft(n, '0') : s;
	}

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
		UInt32 low = (UInt32)(x & 0xFFFFFFFF);
		UInt32 high = (UInt32)(x & 0xFFFFFFFF00000000) >> 32;
		return $"{Convert.ToString(high, 2).PadLeft(32, '0')}{Convert.ToString(low, 2).PadLeft(32, '0')}";
	}

	public string SInt8(SByte x)
	{
		return Pad(Convert.ToString(x, 2), 8);
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