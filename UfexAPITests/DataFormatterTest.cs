using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ufex.API.Tests
{
	[TestClass()]
	public class DataFormatterTests
	{
		[TestMethod]
		public void TestByteArray()
		{
			var f = new DataFormatter();
			f.NumFormat = NumberFormat.Decimal;
			f.SetDecFormat(false);
			Assert.AreEqual("{7, 34, 100}", f.ByteArray(new byte[] { 7, 34, 100 }));
			Assert.AreEqual("{88}", f.ByteArray(new byte[] { 88 }));

			f.NumFormat = NumberFormat.Hexadecimal;
			f.HexCaps = true;
			f.HexLeadZeros = true;
			f.HexShowLead0X = true;
			Assert.AreEqual("{0x58}", f.ByteArray(new byte[] { 88 }));
			Assert.AreEqual("{0x07, 0x22, 0x64}", f.ByteArray(new byte[] { 7, 34, 100 }));

			f.NumFormat = NumberFormat.Binary;
			Assert.AreEqual("{01011000}", f.ByteArray(new byte[] { 88 }));

		}


		[TestMethod]
		public void ToggleEndianUInt32()
		{
			var f = new DataFormatter();
			f.NumFormat = NumberFormat.Decimal;
			f.SetDecFormat(false);
			Assert.AreEqual("20", f.UInt8(20));
			Assert.AreEqual("2500", f.UInt16(2500));
			Assert.AreEqual("123456789", f.UInt32(123456789));
		}

		[TestMethod]
		public void TestASCIIString()
		{
			byte[] fooBar = { 0x46, 0x6f, 0x6f, 0x20, 0x42, 0x61, 0x72 };
			var f = new DataFormatter();
			f.NumFormat = NumberFormat.Ascii;
			Assert.AreEqual("\"Foo Bar\"", f.ASCIIString(fooBar));
			f.NumFormat = NumberFormat.Hexadecimal;
			Assert.AreEqual("46 6F 6F 20 42 61 72", f.ASCIIString(fooBar));
		}

		[TestMethod]
		public void ToggleEndianSInt16()
		{
		}

		[TestMethod]
		public void ToggleEndianSInt32()
		{
		}

		[TestMethod]
		public void ToggleEndianSInt64()
		{
		}
	}
}
