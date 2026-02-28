using System;

namespace Ufex.API;

public class ArrayToNum
{
	public Endian DataEndian { get; set; }

	public ArrayToNum()
	{
		DataEndian = Endian.Little;
	}

	public ArrayToNum(Endian endian)
	{
		DataEndian = endian;
	}

	public UInt16 GetUInt16(Byte[] data)
	{
		return ByteUtil.BytesToUInt16(data, DataEndian, 0);
	}

	public UInt16 GetUInt16(Byte[] data, int offset)
	{
		return ByteUtil.BytesToUInt16(data, DataEndian, offset);
	}

	public UInt32 GetUInt32(Byte[] data)
	{
		return ByteUtil.BytesToUInt32(data, DataEndian, 0);
	}

	public UInt32 GetUInt32(Byte[] data, int offset)
	{
		return ByteUtil.BytesToUInt32(data, DataEndian, offset);
	}

	public UInt64 GetUInt64(byte[] data)
	{
		return ByteUtil.BytesToUInt64(data, DataEndian, 0);
	}

	public UInt64 GetUInt64(byte[] data, int offset)
	{
		return ByteUtil.BytesToUInt64(data, DataEndian, offset);
	}

	public Int16 GetInt16(byte[] data)
	{
		return ByteUtil.BytesToInt16(data, DataEndian);
	}

	public Int16 GetInt16(byte[] data, int offset)
	{
		return ByteUtil.BytesToInt16(data, DataEndian, offset);
	}

	public Int32 GetInt32(byte[] data) 
	{ 
		return ByteUtil.BytesToInt32(data, DataEndian, 0);
	}
	
	public Int32 GetInt32(byte[] data, int offset)
	{
		return ByteUtil.BytesToInt32(data, DataEndian, offset);
	}
}
