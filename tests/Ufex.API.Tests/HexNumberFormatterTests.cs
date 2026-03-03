using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ufex.API.Format;
using Ufex.API.Types;

namespace Ufex.API.Tests
{
	[TestClass()]
	public class HexNumberFormatterTests
	{
		[TestMethod]
		public void TestUnsigned()
		{
			HexNumberFormatter nf = new HexNumberFormatter(true, true);
			nf.LeadZeros = true;
			nf.Endian = Endian.Big;
			Assert.AreEqual(nf.UInt8(0x09), "0x09");
			Assert.AreEqual(nf.UInt8(0x17), "0x17");
			Assert.AreEqual(nf.UInt8(0xFF), "0xFF");
			Assert.AreEqual(nf.UInt16(0x0004), "0x0004");
			Assert.AreEqual(nf.UInt16(0x0074), "0x0074");
			Assert.AreEqual(nf.UInt16(0x014A), "0x014A");
			Assert.AreEqual(nf.UInt16(0x1234), "0x1234");
			Assert.AreEqual(nf.UInt24((UInt24)0x001234u), "0x001234");
			Assert.AreEqual(nf.UInt24((UInt24)0xABCDEFu), "0xABCDEF");
			Assert.AreEqual(nf.UInt32(0x0004), "0x00000004");
			Assert.AreEqual(nf.UInt32(0x0074), "0x00000074");
			Assert.AreEqual(nf.UInt32(0x014A), "0x0000014A");
			Assert.AreEqual(nf.UInt32(0x1234), "0x00001234");
			Assert.AreEqual(nf.UInt32(0x1000123C), "0x1000123C");
		}

		[TestMethod]
		public void TestSignedLittleEndian()
		{
			HexNumberFormatter nf = new HexNumberFormatter();
			nf.LeadX = true;
			nf.Caps = true;
			nf.LeadZeros = true;
			nf.Endian = Endian.Little;
			Assert.AreEqual("0xC4", nf.SInt8(-60));
			Assert.AreEqual("0x40", nf.SInt8(64));
			Assert.AreEqual("0x7E", nf.SInt8(126));
			Assert.AreEqual("0x0400", nf.SInt16(0x0004));
			Assert.AreEqual("0x3412", nf.SInt16(0x1234));
			Assert.AreEqual("0x94FC", nf.SInt16(-876));
			Assert.AreEqual("0x94FCFFFF", nf.SInt32(-876));
			Assert.AreEqual("0xA4010000", nf.SInt32(420));
		}

		[TestMethod]
		public void TestUnsignedLittleEndian()
		{
			HexNumberFormatter nf = new HexNumberFormatter();
			nf.LeadX = true;
			nf.Caps = true;
			nf.LeadZeros = true;
			nf.Endian = Endian.Little;
			Assert.AreEqual("0x0201", nf.UInt16(0x0102));
			Assert.AreEqual("0x3412", nf.UInt16(0x1234));
			Assert.AreEqual("0x030201", nf.UInt24((UInt24)0x010203u));
			Assert.AreEqual("0x44332211", nf.UInt32(0x11223344));
			Assert.AreEqual("0x78563412", nf.UInt32(0x12345678));
			Assert.AreEqual("0x78560000", nf.UInt32(0x00005678));
		}

		[TestMethod]
		public void TestCaps()
		{
			HexNumberFormatter nf = new HexNumberFormatter();
			nf.LeadX = true;
			nf.Caps = true;
			nf.LeadZeros = true;
			Assert.AreEqual("0xAE", nf.UInt8(0xAE));
			Assert.AreEqual("0xABCD", nf.UInt16(0xABCD));
			Assert.AreEqual("0x0ABCDEF0", nf.UInt32(0x0ABCDEF0));
			Assert.AreEqual("0xA2345678B2345678", nf.UInt64(0xA2345678B2345678));
			nf.Caps = false;
			Assert.AreEqual("0xae", nf.UInt8(0xAE));
			Assert.AreEqual("0xabcd", nf.UInt16(0xABCD));
			Assert.AreEqual("0x0abcdef0", nf.UInt32(0x0ABCDEF0));
			Assert.AreEqual("0xa2345678b2345678", nf.UInt64(0xA2345678B2345678));
		}

		[TestMethod]
		public void TestZeros()
		{
			HexNumberFormatter nf = new HexNumberFormatter(true, false, true, Endian.Big);
			Assert.AreEqual("09", nf.UInt8(0x09));
			Assert.AreEqual("17", nf.UInt8(0x17));
			Assert.AreEqual("FF", nf.UInt8(0xFF));
			Assert.AreEqual("0004", nf.UInt16(0x0004));
			Assert.AreEqual("0074", nf.UInt16(0x0074));
			Assert.AreEqual("014A", nf.UInt16(0x014A));
			Assert.AreEqual("1234", nf.UInt16(0x1234));
			Assert.AreEqual("001234", nf.UInt24((UInt24)0x001234u));
			Assert.AreEqual("ABCDEF", nf.UInt24((UInt24)0xABCDEFu));
			Assert.AreEqual("00000004", nf.UInt32(0x0004));
			Assert.AreEqual("00000074", nf.UInt32(0x0074));
			Assert.AreEqual("0000014A", nf.UInt32(0x014A));
			Assert.AreEqual("00001234", nf.UInt32(0x1234));
			Assert.AreEqual("00011234", nf.UInt32(0x011234));
			Assert.AreEqual("1000123C", nf.UInt32(0x1000123C));
			Assert.AreEqual("000000001000123C", nf.UInt64(0x000000001000123Cu));
			Assert.AreEqual("100000001000123C", nf.UInt64(0x100000001000123Cu));

			Assert.AreEqual("0000014A", nf.SInt32(0x014A));
		}
	}
}