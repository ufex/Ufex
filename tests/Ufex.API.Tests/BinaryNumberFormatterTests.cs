using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ufex.API.Format;
using Ufex.API.Types;

namespace Ufex.API.Tests
{
	[TestClass()]
	public class BinaryNumberFormatterTests
	{
		[TestMethod]
		public void Constructor_SetsDefaults()
		{
			var nf = new BinaryNumberFormatter();
			Assert.AreEqual(true, nf.LeadZeros);
			Assert.AreEqual(Endian.Big, nf.Endian);
		}

		[TestMethod]
		public void UInt8_FormatsWithAndWithoutLeadingZeros()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("00000101", nf.UInt8(0x05));

			nf.LeadZeros = false;
			Assert.AreEqual("101", nf.UInt8(0x05));
		}

		[TestMethod]
		public void UInt16_FormatsBigEndian()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("0000000100000010", nf.UInt16(0x0102));
		}

		[TestMethod]
		public void UInt16_FormatsLittleEndian()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Little);
			Assert.AreEqual("0000001000000001", nf.UInt16(0x0102));
		}

		[TestMethod]
		public void UInt24_FormatsBigAndLittleEndian()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("000000010000001000000011", nf.UInt24((UInt24)0x010203u));

			nf.Endian = Endian.Little;
			Assert.AreEqual("000000110000001000000001", nf.UInt24((UInt24)0x010203u));
		}

		[TestMethod]
		public void UInt32_FormatsBigAndLittleEndian()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("00010010001101000101011001111000", nf.UInt32(0x12345678));

			nf.Endian = Endian.Little;
			Assert.AreEqual("01111000010101100011010000010010", nf.UInt32(0x12345678));
		}

		[TestMethod]
		public void UInt64_FormatsBigAndLittleEndian()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("0000000100000010000000110000010000000101000001100000011100001000", nf.UInt64(0x0102030405060708UL));

			nf.Endian = Endian.Little;
			Assert.AreEqual("0000100000000111000001100000010100000100000000110000001000000001", nf.UInt64(0x0102030405060708UL));
		}

		[TestMethod]
		public void UInt64_IncludesNonZeroHighHalf()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("1001000010101011110011011110111100000000000000000000000000000001", nf.UInt64(0x90ABCDEF00000001UL));
		}

		[TestMethod]
		public void UInt16_NoLeadingZeros_ReturnsMinimalBits()
		{
			var nf = new BinaryNumberFormatter(false, Endian.Big);
			Assert.AreEqual("100", nf.UInt16(0x0004));

			nf.Endian = Endian.Little;
			Assert.AreEqual("10000000000", nf.UInt16(0x0004));
		}

		[TestMethod]
		public void UInt64_NoLeadingZeros_CurrentBehaviorStillReturns64Bits()
		{
			var nf = new BinaryNumberFormatter(false, Endian.Big);
			Assert.AreEqual("0000000000000000000000000000000000000000000000000000000000000101", nf.UInt64(0x05UL));
		}

		[TestMethod]
		public void SInt8_FormatsWithAndWithoutLeadingZeros()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("00000101", nf.SInt8(0x05));

			nf.LeadZeros = false;
			Assert.AreEqual("101", nf.SInt8(0x05));
		}

		[TestMethod]
		public void SInt16_FormatsBigAndLittleEndian()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("0001001000110100", nf.SInt16(0x1234));

			nf.Endian = Endian.Little;
			Assert.AreEqual("0011010000010010", nf.SInt16(0x1234));
		}

		[TestMethod]
		public void SInt32_LittleEndian_SwapsBytes()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Little);
			Assert.AreEqual("01111000010101100011010000010010", nf.SInt32(0x12345678));
		}

		[TestMethod]
		public void SignedNegative_CurrentTwoComplementWidthBehavior()
		{
			var nf = new BinaryNumberFormatter(true, Endian.Big);
			Assert.AreEqual("11111111", nf.SInt8(-1));
			Assert.AreEqual("1111111111111111", nf.SInt16(-1));
			Assert.AreEqual("11111111111111111111111111111111", nf.SInt32(-1));
			Assert.AreEqual("1111111111111111111111111111111111111111111111111111111111111111", nf.SInt64(-1));
		}
	}
}
