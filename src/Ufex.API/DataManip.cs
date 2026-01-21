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

		public static UInt16 BytesToUInt16(Byte[] data, Endian endian, int offset = 0)
		{
			if (endian == Endian.Little)
				return (ushort)(data[offset] + (data[offset + 1] * 0x100u));
			else if (endian == Endian.Big)
				return (ushort)((data[offset] * 0x100u) + data[offset + 1]);
			
			return (ushort)BadEndian();
		}

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

		public static Int16 BytesToInt16(Byte[] data, Endian endian, int offset = 0)
		{
			if (endian == Endian.Little)
				return (Int16)((data[offset + 0] & 255) | (data[offset + 1] << 8));
			else if (endian == Endian.Big)
				return (Int16)((data[offset + 1] & 255) | (data[offset + 0] << 8));

			return (Int16)BadEndian();
		}

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
}
