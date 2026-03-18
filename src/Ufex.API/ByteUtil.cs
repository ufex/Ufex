using System;
using System.Reflection;
using Ufex.API.Types;

namespace Ufex.API;

public class ByteUtil
{
	/// <summary>
	/// Determines the size in bytes of the given object.
	/// </summary>
	/// <param name="value"></param>
	/// <returns>The object size in bytes</returns>
	public static long GetObjectSize(object value)
	{
		return value switch
		{
			byte => sizeof(byte),
			sbyte => sizeof(sbyte),
			short => sizeof(short),
			ushort => sizeof(ushort),
			UInt24 => 3,
			int => sizeof(int),
			uint => sizeof(uint),
			long => sizeof(long),
			ulong => sizeof(ulong),
			float => sizeof(float),
			double => sizeof(double),
			byte[] arr => arr.Length,
			sbyte[] arr => arr.Length,
			short[] arr => arr.Length * sizeof(short),
			ushort[] arr => arr.Length * sizeof(ushort),
			int[] arr => arr.Length * sizeof(int),
			uint[] arr => arr.Length * sizeof(uint),
			long[] arr => arr.Length * sizeof(long),
			ulong[] arr => arr.Length * sizeof(ulong),
			float[] arr => arr.Length * sizeof(float),
			double[] arr => arr.Length * sizeof(double),
			Leb128UInt v => v.Size,
			_ when value.GetType().IsValueType => GetCompositeValueTypeSize(value),
			_ => 0 // TODO throw exception?
		};
	}

	private static long GetCompositeValueTypeSize(object value)
	{
		Type valueType = value.GetType();
		long size = 0;

		PropertyInfo[] properties = valueType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo property in properties)
		{
			if (!property.CanRead || property.GetIndexParameters().Length > 0)
				continue;

			object? propertyValue = property.GetValue(value);
			if (propertyValue is null)
				continue;

			size += GetObjectSize(propertyValue);
		}

		FieldInfo[] fields = valueType.GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo field in fields)
		{
			object? fieldValue = field.GetValue(value);
			if (fieldValue is null)
				continue;

			size += GetObjectSize(fieldValue);
		}

		return size;
	}

	/// <summary>
	/// Get a bit from a byte
	/// </summary>
	/// <param name="v">The byte to get the bit from.</param>
	/// <param name="p">The position of the bit to get.</param>
	/// <returns>The bit at the specified position.</returns>
	public static bool GetBit(Byte v, int p) 
	{ 
		return ((v & (Byte)(1 << p)) != 0); 
	}

	/// <summary>
	/// Get a bit from a UInt16
	/// </summary>
	/// <param name="v">The UInt16 to get the bit from.</param>
	/// <param name="p">The position of the bit to get.</param>
	/// <returns>The bit at the specified position.</returns>
	public static bool GetBit(UInt16 v, int p) 
	{ 
		return ((v & (UInt16)(1 << p)) != 0); 
	}

	/// <summary>
	/// Get a bit from a UInt32
	/// </summary>
	/// <param name="v">The UInt32 to get the bit from.</param>
	/// <param name="p">The position of the bit to get.</param>
	/// <returns>The bit at the specified position.</returns>
	public static bool GetBit(UInt32 v, int p) 
	{ 
		return ((v & (UInt32)(1 << p)) != 0); 
	}

	/// <summary>
	/// Get a bit from a UInt64
	/// </summary>
	/// <param name="v">The UInt64 to get the bit from.</param>
	/// <param name="p">The position of the bit to get.</param>
	/// <returns>The bit at the specified position.</returns>
	public static bool GetBit(UInt64 v, int p) 
	{ 
		return ((v & (UInt64)(1 << p)) != 0); 
	}

	/// <summary>
	/// Get a byte from a UInt16
	/// </summary>
	/// <param name="value">The UInt16 to get the byte from.</param>
	/// <param name="position">The position of the byte to get.</param>
	/// <returns>The byte at the specified position.</returns>
	public static Byte GetByte(UInt16 value, int position) 
	{ 
		return (Byte)((value & (UInt16)(0xFF << (position * 8))) >> (position * 8)); 
	}

	/// <summary>
	/// Get a byte from a UInt32
	/// </summary>
	/// <param name="value">The UInt32 to get the byte from.</param>
	/// <param name="position">The position of the byte to get.</param>
	/// <returns>The byte at the specified position.</returns>
	public static Byte GetByte(UInt32 value, int position) 
	{ 
		return (Byte)((value & (UInt32)(0xFF << (position * 8))) >> (position * 8)); 
	}

	/// <summary>
	/// Get a byte from a UInt64
	/// </summary>
	/// <param name="value">The UInt64 to get the byte from.</param>
	/// <param name="position">The position of the byte to get.</param>
	/// <returns>The byte at the specified position.</returns>
	public static Byte GetByte(UInt64 value, int position) 
	{ 
		return (Byte)((value & (0xFFUL << (position * 8))) >> (position * 8)); 
	}

	/// <summary>
	/// Get the high byte from a UInt16
	/// </summary>
	/// <param name="x">The UInt16 to get the high byte from.</param>
	/// <returns>The high byte of the UInt16 value.</returns>
	public static Byte GetHighByte(UInt16 x) 
	{ 
		return (Byte)(x >> 8); 
	}

	/// <summary>
	/// Get the low byte from a UInt16
	/// </summary>
	/// <param name="x">The UInt16 to get the low byte from.</param>
	/// <returns>The low byte of the UInt16 value.</returns>
	public static Byte GetLowByte(UInt16 x) 
	{ 
		return (Byte)(x & 0x00FF); 
	}

	/// <summary>
	/// Get the high word from a UInt32
	/// </summary>
	/// <param name="x">The UInt32 to get the high word from.</param>
	/// <returns>The high word of the UInt32 value.</returns>
	public static UInt16 GetHighWord(UInt32 x) { return (UInt16)(x >> 16); }
	
	/// <summary>
	/// Get the low word from a UInt32
	/// </summary>
	/// <param name="x">The UInt32 to get the low word from.</param>
	/// <returns>The low word of the UInt32 value.</returns>
	public static UInt16 GetLowWord(UInt32 x) { return (UInt16)(x & 0x0000FFFF); }

	/// <summary>
	/// Get the high dword from a UInt64
	/// </summary>
	/// <param name="x">The UInt64 to get the high dword from.</param>
	/// <returns>The high dword of the UInt64 value.</returns>
	public static UInt32 GetHighDword(UInt64 x) { return (UInt32)(x >> 32); }
	public static UInt32 GetLowDword(UInt64 x) { return (UInt32)(x & 0x00000000FFFFFFFF); }

	/// <summary>
	/// Get a specific byte from a UInt32
	/// </summary>
	/// <param name="v">The UInt32 to get the byte from.</param>
	/// <param name="p">The position of the byte to get.</param>
	/// <returns>The byte at the specified position.</returns>
	public static Byte GetByteFromDWORD(UInt32 v, int p) { return (Byte)((v & (UInt32)(0xFF << (p * 8))) >> (p * 8)); }

	/// <summary>
	/// Swap the endianness of a UInt16
	/// </summary>
	/// <param name="x">The UInt16 to swap the endianness of.</param>
	/// <returns>The UInt16 with the endianness swapped.</returns>
	public static UInt16 SwapEndian(UInt16 x)
	{
		return (UInt16)((x >> 8) + (0xFF00 & (x << 8)));
	}

	/// <summary>
	/// Swap the endianness of a UInt24
	/// </summary>
	/// <param name="x">The UInt24 to swap the endianness of.</param>
	/// <returns>The UInt24 with the endianness swapped.</returns>
	public static UInt24 SwapEndian(UInt24 x)
	{
		uint v = x;
		return (UInt24)((0x0000FFu & (v >> 16)) |
			(0x00FF00u & v) |
			(0xFF0000u & (v << 16)));
	}

	/// <summary>
	/// Swap the endianness of a UInt32
	/// </summary>
	/// <param name="x">The UInt32 to swap the endianness of.</param>
	/// <returns>The UInt32 with the endianness swapped.</returns>
	public static UInt32 SwapEndian(UInt32 x)
	{
		return (0x000000FF) & (x >> 24) |
			(0x0000FF00) & (x >> 8) |
			(0x00FF0000) & (x << 8) |
			(0xFF000000) & (x << 24);
	}

	/// <summary>
	/// Swap the endianness of a UInt64
	/// </summary>
	/// <param name="x">The UInt64 to swap the endianness of.</param>
	/// <returns>The UInt64 with the endianness swapped.</returns>
	public static UInt64 SwapEndian(UInt64 x)
	{
		return (0x00000000000000FF) & (x >> 56) | 
			(0x000000000000FF00) & (x >> 40) | 
			(0x0000000000FF0000) & (x >> 24) | 
			(0x00000000FF000000) & (x >>  8) | 
			(0x000000FF00000000) & (x <<  8) | 
			(0x0000FF0000000000) & (x << 24) | 
			(0x00FF000000000000) & (x << 40) | 
			(0xFF00000000000000) & (x << 56);
	}

	/// <summary>
	/// Swap the endianness of an Int16
	/// </summary>
	/// <param name="x">The Int16 to swap the endianness of.</param>
	/// <returns>The Int16 with the endianness swapped.</returns>
	public static Int16 SwapEndian(Int16 x)
	{
		return (Int16)SwapEndian((UInt16)x);
	}

	/// <summary>
	/// Swap the endianness of an Int32
	/// </summary>
	/// <param name="x">The Int32 to swap the endianness of.</param>
	/// <returns>The Int32 with the endianness swapped.</returns>
	public static Int32 SwapEndian(Int32 x)
	{
		return (Int32)SwapEndian((UInt32)x);
	}

	/// <summary>
	/// Swap the endianness of an Int64
	/// </summary>
	/// <param name="x">The Int64 to swap the endianness of.</param>
	/// <returns>The Int64 with the endianness swapped.</returns>
	public static Int64 SwapEndian(Int64 x)
	{
		return (Int64)SwapEndian((UInt64)x);
	}

	/// <summary>
	/// Convert a byte array to a UInt16
	/// </summary>
	/// <param name="data">The byte array to convert.</param>
	/// <param name="endian">The endianness of the byte array.</param>
	/// <param name="offset">The offset to start from.</param>
	/// <returns>The UInt16 value.</returns>
	public static UInt16 BytesToUInt16(Byte[] data, Endian endian, int offset = 0)
	{
		if (endian == Endian.Little)
			return (ushort)(data[offset] + (data[offset + 1] * 0x100u));
		else if (endian == Endian.Big)
			return (ushort)((data[offset] * 0x100u) + data[offset + 1]);
		
		return (ushort)BadEndian();
	}

	/// <summary>
	/// Convert a byte array to a UInt32
	/// </summary>
	/// <param name="data">The byte array to convert.</param>
	/// <param name="endian">The endianness of the byte array.</param>
	/// <param name="offset">The offset to start from.</param>
	/// <returns>The UInt32 value</returns>
	public static UInt32 BytesToUInt32(Byte[] data, Endian endian, int offset = 0)
	{
		if (endian == Endian.Little)
		{
			return (uint)((data[offset])
				+ (data[offset + 1] * 0x100u)
				+ (data[offset + 2] * 0x10000u)
				+ (data[offset + 3] * 0x1000000u));
		}
		else if (endian == Endian.Big)
		{
			return (uint)((data[offset] * 0x1000000u)
				+ (data[offset + 1] * 0x10000u)
				+ (data[offset + 2] * 0x100u)
				+ (data[offset + 3]));
		}
		return (uint)BadEndian();
	}

	/// <summary>
	/// Convert a byte array to a UInt64
	/// </summary>
	/// <param name="data">The byte array to convert.</param>
	/// <param name="endian">The endianness of the byte array.</param>
	/// <param name="off">The offset to start from.</param>
	/// <returns>The UInt64 value.</returns>
	public static UInt64 BytesToUInt64(Byte[] data, Endian endian, int off = 0)
	{
		UInt32 a = 0, b = 0;
		if (endian == Endian.Little)
		{
			a = (uint)(((data[off + 0] | (data[off + 1] << 8)) | (data[off + 2] << 16)) | (data[off + 3] << 24));
			b = (uint)(((data[off + 4] | (data[off + 5] << 8)) | (data[off + 6] << 16)) | (data[off + 7] << 24));
		}
		else if (endian == Endian.Big)
		{
			a = (uint)(((data[off + 7] | (data[off + 6] << 8)) | (data[off + 5] << 16)) | (data[off + 4] << 24));
			b = (uint)(((data[off + 3] | (data[off + 2] << 8)) | (data[off + 1] << 16)) | (data[off + 0] << 24));
		}
		else
		{
			return (UInt64)BadEndian();
		}
		return (((UInt64)(b)) << 32) | ((UInt64)(a));
	}

	/// <summary>
	/// Converts a byte array to a signed 16-bit integer considering the specified endianness.
	/// </summary>
	/// <param name="data">The byte array to convert.</param>
	/// <param name="endian">The endianness of the byte array.</param>
	/// <param name="offset">The offset to start from.</param>
	/// <returns>The Int16 value.</returns>
	public static Int16 BytesToInt16(Byte[] data, Endian endian, int offset = 0)
	{
		if (endian == Endian.Little)
			return (Int16)((data[offset + 0] & 255) | (data[offset + 1] << 8));
		else if (endian == Endian.Big)
			return (Int16)((data[offset + 1] & 255) | (data[offset + 0] << 8));

		return (Int16)BadEndian();
	}

	/// <summary>
	/// Converts a byte array to a signed 32-bit integer considering the specified endianness.
	/// </summary>
	/// <param name="data">The byte array containing the data.</param>
	/// <param name="endian">The endianness of the data.</param>
	/// <param name="offset">The starting index in the byte array.</param>
	/// <returns>The converted 32-bit signed integer.</returns>
	public static Int32 BytesToInt32(Byte[] data, Endian endian, int offset = 0)
	{
		if (endian == Endian.Little)
		{
			return (Int32)(((data[offset + 0] & 255) | 
				(data[offset + 1] << 8)) | 
				(data[offset + 2] << 16)) | 
				(data[offset + 3] << 24);
		}
		else if (endian == Endian.Big)
		{
			return (Int32)(((data[offset + 3] & 255) | 
				(data[offset + 2] << 8)) | 
				(data[offset + 1] << 16)) | 
				(data[offset] << 24);
		}
		return (Int32)BadEndian();
	}

	private static int BadEndian()
	{
		throw new Exception("Invalid Endian");
	}
}
