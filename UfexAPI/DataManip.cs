using System;

namespace Ufex.API
{
	public class DataManip
	{
		public static bool GetBit(Byte v, int p) 
		{ 
			return ((v & (Byte)(1 << p)) != 0); 
		}
		public static bool GetBit(UInt16 v, int p) 
		{ 
			return ((v & (UInt16)(1 << p)) != 0); 
		}
		public static bool GetBit(UInt32 v, int p) 
		{ 
			return ((v & (UInt32)(1 << p)) != 0); 
		}
		public static bool GetBit(UInt64 v, int p) 
		{ 
			return ((v & (UInt64)(1 << p)) != 0); 
		}

		public static Byte GetByte(UInt16 value, int position) 
		{ 
			return (Byte)((value & (UInt16)(0xFF << (position * 8))) >> (position * 8)); 
		}
		public static Byte GetByte(UInt32 value, int position) 
		{ 
			return (Byte)((value & (UInt32)(0xFF << (position * 8))) >> (position * 8)); 
		}
		public static Byte GetByte(UInt64 value, int position) 
		{ 
			return (Byte)((value & (UInt64)(0xFF << (position * 8))) >> (position * 8)); 
		}

		// Get a BYTE from a WORD
		public static Byte GetHighByte(UInt16 x) 
		{ 
			return (Byte)(x >> 8); 
		}

		public static Byte GetLowByte(UInt16 x) 
		{ 
			return (Byte)(x & 0x00FF); 
		}

		// Get a WORD from a DWORD
		public static UInt16 GetHighWord(UInt32 x) { return (UInt16)(x >> 16); }
		public static UInt16 GetLowWord(UInt32 x) { return (UInt16)(x & 0x0000FFFF); }

		// Get a DWORD from a QWORD
		public static UInt32 GetHighDword(UInt64 x) { return (UInt32)(x >> 32); }
		public static UInt32 GetLowDword(UInt64 x) { return (UInt32)(x & 0x00000000FFFFFFFF); }

		// Get a specific byte from a DWORD
		public static Byte GetByteFromDWORD(UInt32 v, int p) { return (Byte)((v & (UInt32)(0xFF << (p * 8))) >> (p * 8)); }

		public static UInt16 SwapEndian(UInt16 x)
		{
			return (UInt16)((x >> 8) + (0xFF00 & (x << 8)));
		}

		public static UInt32 SwapEndian(UInt32 x)
		{
			return (0x000000FF) & (x >> 24) |
				(0x0000FF00) & (x >> 8) |
				(0x00FF0000) & (x << 8) |
				(0xFF000000) & (x << 24);
		}
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

		public static Int16 SwapEndian(Int16 x)
		{
			return (Int16)SwapEndian((UInt16)x);
		}

		public static Int32 SwapEndian(Int32 x)
		{
			return (Int32)SwapEndian((UInt32)x);
		}

		public static Int64 SwapEndian(Int64 x)
		{
			return (Int64)SwapEndian((UInt64)x);
		}

		public static UInt16 BytesToUInt16(Byte[] data, int offset, Endian endian)
		{
			if (endian == Endian.Little)
				return (ushort)(data[offset] + (data[offset + 1] * 0x100u));
			else if (endian == Endian.Big)
				return (ushort)((data[offset] * 0x100u) + data[offset + 1]);
			else
				return (ushort)BadEndian();
		}
		public static UInt32 BytesToUInt32(Byte[] data, int offset, Endian endian)
		{
			if (endian == Endian.Little)
			{
				return (uint)((data[offset])
					+ (data[offset + 1] * 0x100)
					+ (data[offset + 2] * 0x10000)
					+ (data[offset + 3] * 0x1000000));
			}
			else if (endian == Endian.Big)
			{
				return (uint)((data[offset] * 0x1000000)
					+ (data[offset + 1] * 0x10000)
					+ (data[offset + 2] * 0x100)
					+ (data[offset + 3]));
			}
			return 0;
		}

		private static int BadEndian()
		{
			throw new Exception("Invalid Endian");
			return 0;
		}
	}
}
