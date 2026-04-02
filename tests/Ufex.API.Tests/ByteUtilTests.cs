using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ufex.API;
using Ufex.API.Types;

namespace Ufex.API.Tests;

[TestClass]
public class ByteUtilExtendedTests
{
	private readonly struct CompositeStruct
	{
		public UInt16 Header { get; }
		public byte[] Payload { get; }

		public CompositeStruct(UInt16 header, byte[] payload)
		{
			Header = header;
			Payload = payload;
		}
	}

	private readonly struct StringLikeStruct
	{
		public int Length { get; }
		public byte[] Value { get; }

		public StringLikeStruct(int length, byte[] value)
		{
			Length = length;
			Value = value;
		}
	}

	// ── GetObjectSize ────────────────────────────────────────────────

	[TestMethod]
	public void GetObjectSize_Byte_Returns1()
	{
		Assert.AreEqual(1L, ByteUtil.GetObjectSize((Byte)0xFF));
	}

	[TestMethod]
	public void GetObjectSize_SByte_Returns1()
	{
		Assert.AreEqual(1L, ByteUtil.GetObjectSize((SByte)(-1)));
	}

	[TestMethod]
	public void GetObjectSize_Int16_Returns2()
	{
		Assert.AreEqual(2L, ByteUtil.GetObjectSize((Int16)1234));
	}

	[TestMethod]
	public void GetObjectSize_UInt16_Returns2()
	{
		Assert.AreEqual(2L, ByteUtil.GetObjectSize((UInt16)0xFFFF));
	}

	[TestMethod]
	public void GetObjectSize_UInt24_Returns3()
	{
		Assert.AreEqual(3L, ByteUtil.GetObjectSize((UInt24)0x123456));
	}

	[TestMethod]
	public void GetObjectSize_Int32_Returns4()
	{
		Assert.AreEqual(4L, ByteUtil.GetObjectSize((Int32)(-42)));
	}

	[TestMethod]
	public void GetObjectSize_UInt32_Returns4()
	{
		Assert.AreEqual(4L, ByteUtil.GetObjectSize((UInt32)0xDEADBEEF));
	}

	[TestMethod]
	public void GetObjectSize_Int64_Returns8()
	{
		Assert.AreEqual(8L, ByteUtil.GetObjectSize((Int64)(-1)));
	}

	[TestMethod]
	public void GetObjectSize_UInt64_Returns8()
	{
		Assert.AreEqual(8L, ByteUtil.GetObjectSize((UInt64)0xDEADBEEFCAFEBABE));
	}

	[TestMethod]
	public void GetObjectSize_Float_Returns4()
	{
		Assert.AreEqual(4L, ByteUtil.GetObjectSize(3.14f));
	}

	[TestMethod]
	public void GetObjectSize_Double_Returns8()
	{
		Assert.AreEqual(8L, ByteUtil.GetObjectSize(3.14));
	}

	[TestMethod]
	public void GetObjectSize_ByteArray_ReturnsLength()
	{
		Assert.AreEqual(5L, ByteUtil.GetObjectSize(new byte[] { 1, 2, 3, 4, 5 }));
		Assert.AreEqual(0L, ByteUtil.GetObjectSize(new byte[0]));
	}

	[TestMethod]
	public void GetObjectSize_SByteArray_ReturnsLength()
	{
		Assert.AreEqual(3L, ByteUtil.GetObjectSize(new sbyte[] { -1, 0, 1 }));
	}

	[TestMethod]
	public void GetObjectSize_Int16Array_ReturnsBytesLength()
	{
		Assert.AreEqual(6L, ByteUtil.GetObjectSize(new short[] { 1, 2, 3 }));
	}

	[TestMethod]
	public void GetObjectSize_UInt16Array_ReturnsBytesLength()
	{
		Assert.AreEqual(4L, ByteUtil.GetObjectSize(new ushort[] { 0xFFFF, 0x0000 }));
	}

	[TestMethod]
	public void GetObjectSize_Int32Array_ReturnsBytesLength()
	{
		Assert.AreEqual(8L, ByteUtil.GetObjectSize(new int[] { 1, 2 }));
	}

	[TestMethod]
	public void GetObjectSize_UInt32Array_ReturnsBytesLength()
	{
		Assert.AreEqual(12L, ByteUtil.GetObjectSize(new uint[] { 1, 2, 3 }));
	}

	[TestMethod]
	public void GetObjectSize_Int64Array_ReturnsBytesLength()
	{
		Assert.AreEqual(16L, ByteUtil.GetObjectSize(new long[] { 1, 2 }));
	}

	[TestMethod]
	public void GetObjectSize_UInt64Array_ReturnsBytesLength()
	{
		Assert.AreEqual(24L, ByteUtil.GetObjectSize(new ulong[] { 1, 2, 3 }));
	}

	[TestMethod]
	public void GetObjectSize_FloatArray_ReturnsBytesLength()
	{
		Assert.AreEqual(8L, ByteUtil.GetObjectSize(new float[] { 1.0f, 2.0f }));
	}

	[TestMethod]
	public void GetObjectSize_DoubleArray_ReturnsBytesLength()
	{
		Assert.AreEqual(16L, ByteUtil.GetObjectSize(new double[] { 1.0, 2.0 }));
	}

	[TestMethod]
	public void GetObjectSize_Leb128UInt_ReturnsSizeProperty()
	{
		// Single-byte LEB128: value 127 encodes as one byte
		var leb = new Leb128UInt(new byte[] { 0x7F });
		Assert.AreEqual((long)leb.Size, ByteUtil.GetObjectSize(leb));
	}

	[TestMethod]
	public void GetObjectSize_Leb128UInt_MultiByte()
	{
		// Two-byte LEB128: value 128 encodes as 0x80 0x01
		var leb = new Leb128UInt(new byte[] { 0x80, 0x01 });
		Assert.AreEqual(2L, ByteUtil.GetObjectSize(leb));
	}

	[TestMethod]
	public void GetObjectSize_UnknownType_Returns0()
	{
		Assert.AreEqual(0L, ByteUtil.GetObjectSize("hello"));
		Assert.AreEqual(0L, ByteUtil.GetObjectSize(new object()));
	}

	[TestMethod]
	public void GetObjectSize_UnknownStruct_SumsPropertySizes()
	{
		CompositeStruct value = new CompositeStruct(0x1234, new byte[] { 1, 2, 3, 4 });
		Assert.AreEqual(6L, ByteUtil.GetObjectSize(value));
	}

	[TestMethod]
	public void GetObjectSize_StringLikeStruct_SumsLengthAndValueSizes()
	{
		int length = 3;
		StringLikeStruct value = new StringLikeStruct(length, new byte[] { 0x41, 0x42, 0x43 });
		Assert.AreEqual(4L + 3L, ByteUtil.GetObjectSize(value));
	}

	// ── GetBit UInt32 ────────────────────────────────────────────────

	[TestMethod]
	public void GetBit_UInt32_Bit0Set()
	{
		Assert.IsTrue(ByteUtil.GetBit(1u, 0));
		Assert.IsFalse(ByteUtil.GetBit(1u, 1));
	}

	[TestMethod]
	public void GetBit_UInt32_Bit31Set()
	{
		Assert.IsTrue(ByteUtil.GetBit(0x80000000u, 31));
		Assert.IsFalse(ByteUtil.GetBit(0x80000000u, 30));
	}

	[TestMethod]
	public void GetBit_UInt32_AllBitsSet()
	{
		for (int i = 0; i < 32; i++)
			Assert.IsTrue(ByteUtil.GetBit(0xFFFFFFFFu, i));
	}

	[TestMethod]
	public void GetBit_UInt32_NoBitsSet()
	{
		for (int i = 0; i < 32; i++)
			Assert.IsFalse(ByteUtil.GetBit(0u, i));
	}

	// ── GetBit UInt64 ────────────────────────────────────────────────

	[TestMethod]
	public void GetBit_UInt64_Bit0Set()
	{
		Assert.IsTrue(ByteUtil.GetBit(1UL, 0));
		Assert.IsFalse(ByteUtil.GetBit(1UL, 1));
	}

	[TestMethod]
	public void GetBit_UInt64_Bit63Set()
	{
		Assert.IsTrue(ByteUtil.GetBit(0x8000000000000000UL, 63));
		Assert.IsFalse(ByteUtil.GetBit(0x8000000000000000UL, 62));
	}

	[TestMethod]
	public void GetBit_UInt64_AllBitsSet()
	{
		for (int i = 0; i < 64; i++)
			Assert.IsTrue(ByteUtil.GetBit(0xFFFFFFFFFFFFFFFFUL, i));
	}

	// ── GetByte UInt64 ───────────────────────────────────────────────

	[TestMethod]
	public void GetByte_UInt64_AllPositions()
	{
		UInt64 value = 0x0102030405060708;
		Assert.AreEqual((Byte)0x08, ByteUtil.GetByte(value, 0));
		Assert.AreEqual((Byte)0x07, ByteUtil.GetByte(value, 1));
		Assert.AreEqual((Byte)0x06, ByteUtil.GetByte(value, 2));
		Assert.AreEqual((Byte)0x05, ByteUtil.GetByte(value, 3));
		Assert.AreEqual((Byte)0x04, ByteUtil.GetByte(value, 4));
		Assert.AreEqual((Byte)0x03, ByteUtil.GetByte(value, 5));
		Assert.AreEqual((Byte)0x02, ByteUtil.GetByte(value, 6));
		Assert.AreEqual((Byte)0x01, ByteUtil.GetByte(value, 7));
	}

	// ── GetHighWord / GetLowWord ─────────────────────────────────────

	[TestMethod]
	public void GetHighWord_ReturnsUpperTwoBytes()
	{
		Assert.AreEqual((UInt16)0x1234, ByteUtil.GetHighWord(0x12345678u));
		Assert.AreEqual((UInt16)0xFFFF, ByteUtil.GetHighWord(0xFFFF0000u));
		Assert.AreEqual((UInt16)0x0000, ByteUtil.GetHighWord(0x0000FFFFu));
	}

	[TestMethod]
	public void GetLowWord_ReturnsLowerTwoBytes()
	{
		Assert.AreEqual((UInt16)0x5678, ByteUtil.GetLowWord(0x12345678u));
		Assert.AreEqual((UInt16)0x0000, ByteUtil.GetLowWord(0xFFFF0000u));
		Assert.AreEqual((UInt16)0xFFFF, ByteUtil.GetLowWord(0x0000FFFFu));
	}

	// ── GetHighDword / GetLowDword ───────────────────────────────────

	[TestMethod]
	public void GetHighDword_ReturnsUpperFourBytes()
	{
		Assert.AreEqual(0x12345678u, ByteUtil.GetHighDword(0x1234567890ABCDEFu));
		Assert.AreEqual(0xFFFFFFFFu, ByteUtil.GetHighDword(0xFFFFFFFF00000000u));
		Assert.AreEqual(0x00000000u, ByteUtil.GetHighDword(0x00000000FFFFFFFFu));
	}

	[TestMethod]
	public void GetLowDword_ReturnsLowerFourBytes()
	{
		Assert.AreEqual(0x90ABCDEFu, ByteUtil.GetLowDword(0x1234567890ABCDEFu));
		Assert.AreEqual(0x00000000u, ByteUtil.GetLowDword(0xFFFFFFFF00000000u));
		Assert.AreEqual(0xFFFFFFFFu, ByteUtil.GetLowDword(0x00000000FFFFFFFFu));
	}

	// ── GetByteFromDWORD ─────────────────────────────────────────────

	[TestMethod]
	public void GetByteFromDWORD_AllPositions()
	{
		Assert.AreEqual((Byte)0x78, ByteUtil.GetByteFromDWORD(0x12345678u, 0));
		Assert.AreEqual((Byte)0x56, ByteUtil.GetByteFromDWORD(0x12345678u, 1));
		Assert.AreEqual((Byte)0x34, ByteUtil.GetByteFromDWORD(0x12345678u, 2));
		Assert.AreEqual((Byte)0x12, ByteUtil.GetByteFromDWORD(0x12345678u, 3));
	}

	// ── SwapEndian UInt24 ────────────────────────────────────────────

	[TestMethod]
	public void SwapEndian_UInt24_ReversesBytes()
	{
		Assert.AreEqual((UInt24)0x030201u, ByteUtil.SwapEndian((UInt24)0x010203u));
		Assert.AreEqual((UInt24)0x000000u, ByteUtil.SwapEndian((UInt24)0x000000u));
		Assert.AreEqual((UInt24)0xFFFFFFu, ByteUtil.SwapEndian((UInt24)0xFFFFFFu));
		Assert.AreEqual((UInt24)0xFE0000u, ByteUtil.SwapEndian((UInt24)0x0000FEu));
	}

	// ── BytesToInt16 with offset ─────────────────────────────────────

	[TestMethod]
	public void BytesToInt16_LittleEndian()
	{
		// 0x0100 LE = 1 * 256 = 256
		Assert.AreEqual((Int16)256, ByteUtil.BytesToInt16(new byte[] { 0x00, 0x01 }, Endian.Little));
		// Negative value
		Assert.AreEqual((Int16)(-1), ByteUtil.BytesToInt16(new byte[] { 0xFF, 0xFF }, Endian.Little));
		Assert.AreEqual((Int16)0, ByteUtil.BytesToInt16(new byte[] { 0x00, 0x00 }, Endian.Little));
	}

	[TestMethod]
	public void BytesToInt16_BigEndian()
	{
		Assert.AreEqual((Int16)256, ByteUtil.BytesToInt16(new byte[] { 0x01, 0x00 }, Endian.Big));
		Assert.AreEqual((Int16)(-1), ByteUtil.BytesToInt16(new byte[] { 0xFF, 0xFF }, Endian.Big));
		Assert.AreEqual((Int16)0, ByteUtil.BytesToInt16(new byte[] { 0x00, 0x00 }, Endian.Big));
	}

	[TestMethod]
	public void BytesToInt16_WithOffset()
	{
		byte[] data = { 0xAA, 0xBB, 0x01, 0x00 };
		Assert.AreEqual((Int16)1, ByteUtil.BytesToInt16(data, Endian.Little, 2));
		Assert.AreEqual((Int16)256, ByteUtil.BytesToInt16(data, Endian.Big, 2));
	}

	// ── BytesToInt32 ─────────────────────────────────────────────────

	[TestMethod]
	public void BytesToInt32_LittleEndian()
	{
		Assert.AreEqual(0x78563412, ByteUtil.BytesToInt32(new byte[] { 0x12, 0x34, 0x56, 0x78 }, Endian.Little));
		Assert.AreEqual(-1, ByteUtil.BytesToInt32(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, Endian.Little));
		Assert.AreEqual(0, ByteUtil.BytesToInt32(new byte[] { 0x00, 0x00, 0x00, 0x00 }, Endian.Little));
		Assert.AreEqual(1, ByteUtil.BytesToInt32(new byte[] { 0x01, 0x00, 0x00, 0x00 }, Endian.Little));
	}

	[TestMethod]
	public void BytesToInt32_BigEndian()
	{
		Assert.AreEqual(0x12345678, ByteUtil.BytesToInt32(new byte[] { 0x12, 0x34, 0x56, 0x78 }, Endian.Big));
		Assert.AreEqual(-1, ByteUtil.BytesToInt32(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, Endian.Big));
		Assert.AreEqual(0, ByteUtil.BytesToInt32(new byte[] { 0x00, 0x00, 0x00, 0x00 }, Endian.Big));
		Assert.AreEqual(1, ByteUtil.BytesToInt32(new byte[] { 0x00, 0x00, 0x00, 0x01 }, Endian.Big));
	}

	[TestMethod]
	public void BytesToInt32_WithOffset()
	{
		byte[] data = { 0xAA, 0xBB, 0x01, 0x00, 0x00, 0x00 };
		Assert.AreEqual(1, ByteUtil.BytesToInt32(data, Endian.Little, 2));
	}

	[TestMethod]
	public void BytesToInt32_NegativeValues()
	{
		// Int32.MinValue = -2,147,483,648 = 0x80000000
		Assert.AreEqual(Int32.MinValue, ByteUtil.BytesToInt32(new byte[] { 0x00, 0x00, 0x00, 0x80 }, Endian.Little));
		Assert.AreEqual(Int32.MinValue, ByteUtil.BytesToInt32(new byte[] { 0x80, 0x00, 0x00, 0x00 }, Endian.Big));
	}

	// ── BytesToUInt16 with offset ────────────────────────────────────

	[TestMethod]
	public void BytesToUInt16_WithOffset()
	{
		byte[] data = { 0xAA, 0xBB, 0xCC, 0x34, 0x12 };
		Assert.AreEqual((UInt16)0x1234, ByteUtil.BytesToUInt16(data, Endian.Little, 3));
		Assert.AreEqual((UInt16)0x3412, ByteUtil.BytesToUInt16(data, Endian.Big, 3));
	}

	// ── BytesToUInt32 with offset ────────────────────────────────────

	[TestMethod]
	public void BytesToUInt32_WithOffset()
	{
		byte[] data = { 0xAA, 0x78, 0x56, 0x34, 0x12 };
		Assert.AreEqual(0x12345678u, ByteUtil.BytesToUInt32(data, Endian.Little, 1));
		Assert.AreEqual(0x78563412u, ByteUtil.BytesToUInt32(data, Endian.Big, 1));
	}

	// ── BytesToUInt64 with offset ────────────────────────────────────

	[TestMethod]
	public void BytesToUInt64_WithOffset()
	{
		byte[] data = { 0xFF, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
		Assert.AreEqual(0x0102030405060708u, ByteUtil.BytesToUInt64(data, Endian.Little, 1));
		Assert.AreEqual(0x0807060504030201u, ByteUtil.BytesToUInt64(data, Endian.Big, 1));
	}

	[TestMethod]
	public void BytesToUInt64_ZeroAndMax()
	{
		Assert.AreEqual(0UL, ByteUtil.BytesToUInt64(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, Endian.Little));
		Assert.AreEqual(0UL, ByteUtil.BytesToUInt64(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, Endian.Big));
		Assert.AreEqual(UInt64.MaxValue, ByteUtil.BytesToUInt64(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, Endian.Little));
		Assert.AreEqual(UInt64.MaxValue, ByteUtil.BytesToUInt64(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, Endian.Big));
	}

	// ── SwapEndian roundtrip tests ───────────────────────────────────

	[TestMethod]
	public void SwapEndian_UInt16_DoubleSwapReturnsOriginal()
	{
		UInt16 value = 0xABCD;
		Assert.AreEqual(value, ByteUtil.SwapEndian(ByteUtil.SwapEndian(value)));
	}

	[TestMethod]
	public void SwapEndian_UInt32_DoubleSwapReturnsOriginal()
	{
		UInt32 value = 0xDEADBEEF;
		Assert.AreEqual(value, ByteUtil.SwapEndian(ByteUtil.SwapEndian(value)));
	}

	[TestMethod]
	public void SwapEndian_UInt64_DoubleSwapReturnsOriginal()
	{
		UInt64 value = 0xDEADBEEFCAFEBABE;
		Assert.AreEqual(value, ByteUtil.SwapEndian(ByteUtil.SwapEndian(value)));
	}

	[TestMethod]
	public void SwapEndian_Int16_DoubleSwapReturnsOriginal()
	{
		Int16 value = -12345;
		Assert.AreEqual(value, ByteUtil.SwapEndian(ByteUtil.SwapEndian(value)));
	}

	[TestMethod]
	public void SwapEndian_Int32_DoubleSwapReturnsOriginal()
	{
		Int32 value = -123456789;
		Assert.AreEqual(value, ByteUtil.SwapEndian(ByteUtil.SwapEndian(value)));
	}

	[TestMethod]
	public void SwapEndian_Int64_DoubleSwapReturnsOriginal()
	{
		Int64 value = -1234567890123456789;
		Assert.AreEqual(value, ByteUtil.SwapEndian(ByteUtil.SwapEndian(value)));
	}

	[TestMethod]
	public void SwapEndian_UInt24_DoubleSwapReturnsOriginal()
	{
		UInt24 value = (UInt24)0xABCDEFu;
		Assert.AreEqual(value, ByteUtil.SwapEndian(ByteUtil.SwapEndian(value)));
	}

	// ── Boundary / edge case tests ───────────────────────────────────

	[TestMethod]
	public void SwapEndian_UInt16_Zero()
	{
		Assert.AreEqual((UInt16)0, ByteUtil.SwapEndian((UInt16)0));
	}

	[TestMethod]
	public void SwapEndian_UInt32_Zero()
	{
		Assert.AreEqual(0u, ByteUtil.SwapEndian(0u));
	}

	[TestMethod]
	public void SwapEndian_UInt64_Zero()
	{
		Assert.AreEqual(0UL, ByteUtil.SwapEndian(0UL));
	}

	[TestMethod]
	public void SwapEndian_UInt16_MaxValue()
	{
		Assert.AreEqual(UInt16.MaxValue, ByteUtil.SwapEndian(UInt16.MaxValue));
	}

	[TestMethod]
	public void SwapEndian_UInt32_MaxValue()
	{
		Assert.AreEqual(UInt32.MaxValue, ByteUtil.SwapEndian(UInt32.MaxValue));
	}

	[TestMethod]
	public void SwapEndian_UInt64_MaxValue()
	{
		Assert.AreEqual(UInt64.MaxValue, ByteUtil.SwapEndian(UInt64.MaxValue));
	}

	[TestMethod]
	public void GetHighByte_Zero()
	{
		Assert.AreEqual((Byte)0, ByteUtil.GetHighByte(0));
	}

	[TestMethod]
	public void GetLowByte_Zero()
	{
		Assert.AreEqual((Byte)0, ByteUtil.GetLowByte(0));
	}

	[TestMethod]
	public void GetHighWord_Zero()
	{
		Assert.AreEqual((UInt16)0, ByteUtil.GetHighWord(0));
	}

	[TestMethod]
	public void GetLowWord_Zero()
	{
		Assert.AreEqual((UInt16)0, ByteUtil.GetLowWord(0));
	}

	[TestMethod]
	public void GetHighDword_Zero()
	{
		Assert.AreEqual(0u, ByteUtil.GetHighDword(0));
	}

	[TestMethod]
	public void GetLowDword_Zero()
	{
		Assert.AreEqual(0u, ByteUtil.GetLowDword(0));
	}

	// ── Consistency: GetByte matches GetHighByte/GetLowByte ──────────

	[TestMethod]
	public void GetByte_UInt16_ConsistentWithHighLow()
	{
		UInt16 value = 0xABCD;
		Assert.AreEqual(ByteUtil.GetLowByte(value), ByteUtil.GetByte(value, 0));
		Assert.AreEqual(ByteUtil.GetHighByte(value), ByteUtil.GetByte(value, 1));
	}

	// ── Consistency: GetByte matches GetHighWord/GetLowWord ──────────

	[TestMethod]
	public void GetByte_UInt32_ConsistentWithWords()
	{
		UInt32 value = 0x12345678;
		UInt16 low = ByteUtil.GetLowWord(value);
		UInt16 high = ByteUtil.GetHighWord(value);
		Assert.AreEqual(ByteUtil.GetLowByte(low), ByteUtil.GetByte(value, 0));
		Assert.AreEqual(ByteUtil.GetHighByte(low), ByteUtil.GetByte(value, 1));
		Assert.AreEqual(ByteUtil.GetLowByte(high), ByteUtil.GetByte(value, 2));
		Assert.AreEqual(ByteUtil.GetHighByte(high), ByteUtil.GetByte(value, 3));
	}

	// ── Consistency: GetByteFromDWORD matches GetByte(UInt32) ────────

	[TestMethod]
	public void GetByteFromDWORD_MatchesGetByte()
	{
		UInt32 value = 0xDEADBEEF;
		for (int i = 0; i < 4; i++)
			Assert.AreEqual(ByteUtil.GetByte(value, i), ByteUtil.GetByteFromDWORD(value, i));
	}

	// ── BytesToUInt16/BytesToInt16 consistency ────────────────────────

	[TestMethod]
	public void BytesToUInt16_BytesToInt16_Consistent()
	{
		// For values < 0x8000, unsigned and signed should match
		byte[] data = { 0x34, 0x12 };
		UInt16 unsigned = ByteUtil.BytesToUInt16(data, Endian.Little);
		Int16 signed = ByteUtil.BytesToInt16(data, Endian.Little);
		Assert.AreEqual((Int16)unsigned, signed);
	}

	// ── BytesToUInt32/BytesToInt32 consistency ────────────────────────

	[TestMethod]
	public void BytesToUInt32_BytesToInt32_Consistent()
	{
		// For values < 0x80000000, unsigned and signed should match
		byte[] data = { 0x12, 0x34, 0x56, 0x00 };
		UInt32 unsigned = ByteUtil.BytesToUInt32(data, Endian.Little);
		Int32 signed = ByteUtil.BytesToInt32(data, Endian.Little);
		Assert.AreEqual((Int32)unsigned, signed);
	}
}
