using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using System.IO;
using Ufex.API.Types;

namespace Ufex.API.Tests
{
	[TestClass()]
	public class Leb128UIntTests
	{
		// --- Constructor from byte[] ---

		[TestMethod]
		public void ByteConstructor_SingleByte_Zero()
		{
			var leb = new Leb128UInt([ 0x00 ]);
			Assert.AreEqual(new BigInteger(0), (BigInteger)(uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_SingleByte_SmallValue()
		{
			// 0x05 = 5 (no continuation bit)
			var leb = new Leb128UInt([ 0x05 ]);
			Assert.AreEqual(5u, (uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_SingleByte_MaxSingleByte()
		{
			// 0x7F = 127 (largest single-byte LEB128 value)
			var leb = new Leb128UInt([ 0x7F ]);
			Assert.AreEqual(127u, (uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_TwoByte_128()
		{
			// 128 = 0x80 0x01 in LEB128
			var leb = new Leb128UInt([ 0x80, 0x01 ]);
			Assert.AreEqual(128u, (uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_TwoByte_300()
		{
			// 300 = 0xAC 0x02 in LEB128
			// 300 = 0b100101100
			// chunk1: 0101100 = 0x2C | 0x80 = 0xAC
			// chunk2: 0000010 = 0x02
			var leb = new Leb128UInt([ 0xAC, 0x02 ]);
			Assert.AreEqual(300u, (uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_ThreeByte_16384()
		{
			// 16384 = 0x80 0x80 0x01 in LEB128
			var leb = new Leb128UInt([ 0x80, 0x80, 0x01 ]);
			Assert.AreEqual(16384u, (uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_FourByte_LargeValue()
		{
			// 624485 = 0xE5 0x8E 0x26 in LEB128 (classic Wikipedia example)
			// 624485 = 0b10011000_10001110_00101
			// chunk1: 1100101 | 0x80 = 0xE5
			// chunk2: 0001110 | 0x80 = 0x8E
			// chunk3: 0100110 = 0x26
			var leb = new Leb128UInt([ 0xE5, 0x8E, 0x26 ]);
			Assert.AreEqual(624485u, (uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_FiveByte_UInt32Max()
		{
			// UInt32.MaxValue = 4294967295
			// LEB128: 0xFF 0xFF 0xFF 0xFF 0x0F
			var leb = new Leb128UInt([ 0xFF, 0xFF, 0xFF, 0xFF, 0x0F ]);
			Assert.AreEqual(uint.MaxValue, (uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_IgnoresTrailingBytes()
		{
			// After the terminating byte (no continuation bit), remaining bytes should be ignored
			var leb = new Leb128UInt([ 0x05, 0xFF, 0xFF ]);
			Assert.AreEqual(5u, (uint)leb);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidDataException))]
		public void ByteConstructor_TooManyBytes_Throws()
		{
			// 17 continuation bytes → shift reaches 112 (MaxBytes * 7), should throw
			var leb = new Leb128UInt([
				0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
				0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80
			]);
		}

		// --- Constructor from BigInteger ---

		[TestMethod]
		public void BigIntegerConstructor_Zero()
		{
			var leb = new Leb128UInt(BigInteger.Zero);
			Assert.AreEqual(0u, (uint)leb);
		}

		[TestMethod]
		public void BigIntegerConstructor_LargeValue()
		{
			var leb = new Leb128UInt(new BigInteger(1000000));
			Assert.AreEqual(1000000u, (uint)leb);
		}

		// --- Bytes property (encoding) ---

		[TestMethod]
		public void Bytes_Zero()
		{
			var leb = new Leb128UInt(BigInteger.Zero);
			CollectionAssert.AreEqual(new byte[] { 0x00 }, leb.Bytes);
		}

		[TestMethod]
		public void Bytes_SmallValue()
		{
			var leb = new Leb128UInt(new BigInteger(5));
			CollectionAssert.AreEqual(new byte[] { 0x05 }, leb.Bytes);
		}

		[TestMethod]
		public void Bytes_127()
		{
			var leb = new Leb128UInt(new BigInteger(127));
			CollectionAssert.AreEqual(new byte[] { 0x7F }, leb.Bytes);
		}

		[TestMethod]
		public void Bytes_128()
		{
			var leb = new Leb128UInt(new BigInteger(128));
			CollectionAssert.AreEqual(new byte[] { 0x80, 0x01 }, leb.Bytes);
		}

		[TestMethod]
		public void Bytes_300()
		{
			var leb = new Leb128UInt(new BigInteger(300));
			CollectionAssert.AreEqual(new byte[] { 0xAC, 0x02 }, leb.Bytes);
		}

		[TestMethod]
		public void Bytes_624485()
		{
			var leb = new Leb128UInt(new BigInteger(624485));
			CollectionAssert.AreEqual(new byte[] { 0xE5, 0x8E, 0x26 }, leb.Bytes);
		}

		[TestMethod]
		public void Bytes_UInt32Max()
		{
			var leb = new Leb128UInt(new BigInteger(uint.MaxValue));
			CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F }, leb.Bytes);
		}

		// --- Roundtrip: encode then decode ---

		[TestMethod]
		public void Roundtrip_Zero()
		{
			var original = new Leb128UInt(BigInteger.Zero);
			var roundtripped = new Leb128UInt(original.Bytes);
			Assert.AreEqual((uint)original, (uint)roundtripped);
		}

		[TestMethod]
		public void Roundtrip_128()
		{
			var original = new Leb128UInt(new BigInteger(128));
			var roundtripped = new Leb128UInt(original.Bytes);
			Assert.AreEqual((uint)original, (uint)roundtripped);
		}

		[TestMethod]
		public void Roundtrip_624485()
		{
			var original = new Leb128UInt(new BigInteger(624485));
			var roundtripped = new Leb128UInt(original.Bytes);
			Assert.AreEqual((uint)original, (uint)roundtripped);
		}

		[TestMethod]
		public void Roundtrip_UInt32Max()
		{
			var original = new Leb128UInt(new BigInteger(uint.MaxValue));
			var roundtripped = new Leb128UInt(original.Bytes);
			Assert.AreEqual((uint)original, (uint)roundtripped);
		}

		// --- Conversion operators ---

		[TestMethod]
		public void ImplicitConversion_ToUInt()
		{
			var leb = new Leb128UInt(new BigInteger(42));
			uint result = (uint)leb;
			Assert.AreEqual(42u, result);
		}

		[TestMethod]
		public void ImplicitConversion_ToInt()
		{
			var leb = new Leb128UInt(new BigInteger(42));
			int result = (int)leb;
			Assert.AreEqual(42, result);
		}

		[TestMethod]
		public void ImplicitConversion_ToLong()
		{
			var leb = new Leb128UInt(new BigInteger(42));
			long result = (long)leb;
			Assert.AreEqual(42L, result);
		}

		[TestMethod]
		public void ImplicitConversion_ToBigInteger()
		{
			var leb = new Leb128UInt(new BigInteger(42));
			BigInteger result = leb;
			Assert.AreEqual(new BigInteger(42), result);
		}

		[TestMethod]
		public void ExplicitConversion_FromUInt()
		{
			Leb128UInt leb = (Leb128UInt)42u;
			Assert.AreEqual(42u, (uint)leb);
		}

		[TestMethod]
		public void ExplicitConversion_FromInt()
		{
			Leb128UInt leb = (Leb128UInt)42;
			Assert.AreEqual(42u, (uint)leb);
		}

		[TestMethod]
		[ExpectedException(typeof(OverflowException))]
		public void ExplicitConversion_FromNegativeInt_Throws()
		{
			Leb128UInt leb = (Leb128UInt)(-1);
		}

		[TestMethod]
		[ExpectedException(typeof(OverflowException))]
		public void BigIntegerConstructor_Negative_Throws()
		{
			var leb = new Leb128UInt(new BigInteger(-1));
		}

		[TestMethod]
		[ExpectedException(typeof(OverflowException))]
		public void Subtraction_Underflow_Throws()
		{
			var a = new Leb128UInt(new BigInteger(10));
			var b = new Leb128UInt(new BigInteger(20));
			var result = a - b;
		}

		// --- Equality operators ---

		[TestMethod]
		public void Equality_SameValue_ReturnsTrue()
		{
			var a = new Leb128UInt(new BigInteger(100));
			var b = new Leb128UInt(new BigInteger(100));
			Assert.IsTrue(a == b);
			Assert.IsFalse(a != b);
		}

		[TestMethod]
		public void Equality_DifferentValue_ReturnsFalse()
		{
			var a = new Leb128UInt(new BigInteger(100));
			var b = new Leb128UInt(new BigInteger(200));
			Assert.IsFalse(a == b);
			Assert.IsTrue(a != b);
		}

		// --- Arithmetic operators ---

		[TestMethod]
		public void Addition()
		{
			var a = new Leb128UInt(new BigInteger(100));
			var b = new Leb128UInt(new BigInteger(50));
			var result = a + b;
			Assert.AreEqual(150u, (uint)result);
		}

		[TestMethod]
		public void Subtraction()
		{
			var a = new Leb128UInt(new BigInteger(100));
			var b = new Leb128UInt(new BigInteger(30));
			var result = a - b;
			Assert.AreEqual(70u, (uint)result);
		}

		// --- Equals / GetHashCode ---

		[TestMethod]
		public void Equals_Object_SameValue()
		{
			var a = new Leb128UInt(new BigInteger(42));
			var b = new Leb128UInt(new BigInteger(42));
			Assert.IsTrue(a.Equals((object)b));
		}

		[TestMethod]
		public void Equals_Object_DifferentType_ReturnsFalse()
		{
			var a = new Leb128UInt(new BigInteger(42));
			Assert.IsFalse(a.Equals("not a leb128"));
		}

		[TestMethod]
		public void GetHashCode_SameForEqualValues()
		{
			var a = new Leb128UInt(new BigInteger(999));
			var b = new Leb128UInt(new BigInteger(999));
			Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
		}

		// --- ToString ---

		[TestMethod]
		public void ToString_ReturnsDecimalString()
		{
			var leb = new Leb128UInt(new BigInteger(624485));
			Assert.AreEqual("624485", leb.ToString());
		}

		[TestMethod]
		public void ToString_Zero()
		{
			var leb = new Leb128UInt(BigInteger.Zero);
			Assert.AreEqual("0", leb.ToString());
		}

		// --- CompareTo ---

		[TestMethod]
		public void CompareTo_LessThan()
		{
			var a = new Leb128UInt(new BigInteger(10));
			var b = new Leb128UInt(new BigInteger(20));
			Assert.IsTrue(a.CompareTo(b) < 0);
		}

		[TestMethod]
		public void CompareTo_GreaterThan()
		{
			var a = new Leb128UInt(new BigInteger(20));
			var b = new Leb128UInt(new BigInteger(10));
			Assert.IsTrue(a.CompareTo(b) > 0);
		}

		[TestMethod]
		public void CompareTo_Equal()
		{
			var a = new Leb128UInt(new BigInteger(10));
			var b = new Leb128UInt(new BigInteger(10));
			Assert.AreEqual(0, a.CompareTo(b));
		}

		[TestMethod]
		public void CompareTo_Null_Returns1()
		{
			var a = new Leb128UInt(new BigInteger(10));
			Assert.AreEqual(1, a.CompareTo(null));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void CompareTo_WrongType_Throws()
		{
			var a = new Leb128UInt(new BigInteger(10));
			a.CompareTo("not a leb128");
		}

		// --- Shift overflow bug test ---
		// These tests verify the fix for int overflow at shift >= 25
		// in the byte[] constructor.

		[TestMethod]
		public void ByteConstructor_FiveByte_HighBitsCorrect()
		{
			// Value: 0x0FFFFFFFFF (max 5-byte LEB128 unsigned value with all payload bits set)
			// = 2^35 - 1 = 34359738367
			// LEB128: 0xFF 0xFF 0xFF 0xFF 0x7F
			// This exercises the shift=28 path where int overflow occurs
			var leb = new Leb128UInt([ 0xFF, 0xFF, 0xFF, 0xFF, 0x7F ]);
			BigInteger expected = (BigInteger.One << 35) - 1; // 34359738367
			Assert.AreEqual(expected.ToString(), leb.ToString());
		}

		[TestMethod]
		public void ByteConstructor_Shift28_ValueCorrect()
		{
			// Specifically test a value that sets bits at position 28+
			// Value with only the 5th byte set to 0x01 → value = 1 << 28 = 268435456
			// LEB128: 0x80 0x80 0x80 0x80 0x01
			var leb = new Leb128UInt([ 0x80, 0x80, 0x80, 0x80, 0x01 ]);
			Assert.AreEqual((uint)268435456, (uint)leb);
		}

		[TestMethod]
		public void ByteConstructor_Shift28_AllBitsSet()
		{
			// 0x0F at shift 28 → 0x0F << 28 = 0xF0000000 = 4026531840
			// This is where int overflow happens: (int)(0x0F << 28) = -268435456
			// LEB128: 0x80 0x80 0x80 0x80 0x0F → value = 0xF0000000
			var leb = new Leb128UInt([ 0x80, 0x80, 0x80, 0x80, 0x0F ]);
			uint expected = 0xF0000000u; // 4026531840
			Assert.AreEqual(expected, (uint)leb);
		}
	}
}
