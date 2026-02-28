using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ufex.API.Tests
{
	[TestClass()]
	public class ByteUtilTests
	{
		[TestMethod]
		public void GetBit()
		{
			Assert.AreEqual(true, ByteUtil.GetBit((Byte)2, 1));
			Assert.AreEqual(false, ByteUtil.GetBit((Byte)2, 0));
			Assert.AreEqual(false, ByteUtil.GetBit((UInt16)16384, 15));
			Assert.AreEqual(true, ByteUtil.GetBit((UInt16)16384, 14));
			Assert.AreEqual(false, ByteUtil.GetBit((UInt16)16384, 13));
		}

		[TestMethod]
		public void TestGetByte()
		{
			Assert.AreEqual(0x34, ByteUtil.GetByte((UInt16)0x1234, 0));
			Assert.AreEqual(0x12, ByteUtil.GetByte((UInt16)0x1234, 1));
			Assert.AreEqual(0x78, ByteUtil.GetByte((UInt32)0x12345678, 0));
			Assert.AreEqual(0x56, ByteUtil.GetByte((UInt32)0x12345678, 1));
			Assert.AreEqual(0x34, ByteUtil.GetByte((UInt32)0x12345678, 2));
			Assert.AreEqual(0x12, ByteUtil.GetByte((UInt32)0x12345678, 3));
		}

		[TestMethod]
		public void TestGetHighByte()
		{
			Assert.AreEqual(0x12, ByteUtil.GetHighByte((UInt16)0x1234));
		}

		[TestMethod]
		public void TestGetLowByte()
		{
			Assert.AreEqual(0x34, ByteUtil.GetLowByte((UInt16)0x1234));
		}

		[TestMethod]
		public void ToggleEndianUInt16()
		{
			Assert.AreEqual((UInt16)258, ByteUtil.SwapEndian((UInt16)513));
			Assert.AreEqual((UInt16)4386, ByteUtil.SwapEndian((UInt16)8721));
			Assert.AreEqual((UInt16)43707, ByteUtil.SwapEndian((UInt16)48042));
		}


		[TestMethod]
		public void ToggleEndianUInt32()
		{
			Assert.AreEqual(0x01020304u, ByteUtil.SwapEndian(0x04030201u));
			Assert.AreEqual(0x11223344u, ByteUtil.SwapEndian(0x44332211u));
		}

		[TestMethod]
		public void ToggleEndianUInt64()
		{
			Assert.AreEqual(0x0102030405060708u, ByteUtil.SwapEndian(0x0807060504030201u));
			Assert.AreEqual(0x1122334455667788u, ByteUtil.SwapEndian(0x8877665544332211u));
		}

		[TestMethod]
		public void ToggleEndianSInt16()
		{
			Assert.AreEqual((Int16)258, ByteUtil.SwapEndian((Int16)513));
			Assert.AreEqual((Int16)4386, ByteUtil.SwapEndian((Int16)8721));
			Assert.AreEqual((Int16)(-21829), ByteUtil.SwapEndian((Int16)(-17494)));
		}

		[TestMethod]
		public void ToggleEndianSInt32()
		{
			Assert.AreEqual(16909060, ByteUtil.SwapEndian(67305985));
			Assert.AreEqual(287454020, ByteUtil.SwapEndian(1144201745));
		}

		[TestMethod]
		public void ToggleEndianSInt64()
		{
			Assert.AreEqual((Int64)0x0102030405060708, ByteUtil.SwapEndian((Int64)0x0807060504030201));
			Assert.AreEqual((Int64)1234605616436508552, ByteUtil.SwapEndian((Int64)(-8613303245920329199)));
		}

		[TestMethod]
		public void TestBytesToUInt16()
		{
			Assert.AreEqual(0x3412u, ByteUtil.BytesToUInt16(new byte[] { 0x12, 0x34 }, Endian.Little));
			Assert.AreEqual(0x3412u, ByteUtil.BytesToUInt16(new byte[] { 0x34, 0x12 }, Endian.Big));
			Assert.AreEqual(0xFEDCu, ByteUtil.BytesToUInt16(new byte[] { 0xDC, 0xFE }, Endian.Little));
			Assert.AreEqual(0xDCFEu, ByteUtil.BytesToUInt16(new byte[] { 0xFE, 0xDC }, Endian.Little));
			Assert.AreEqual(0xFEDCu, ByteUtil.BytesToUInt16(new byte[] { 0xFE, 0xDC }, Endian.Big));
			Assert.AreEqual(0xDCFEu, ByteUtil.BytesToUInt16(new byte[] { 0xDC, 0xFE }, Endian.Big));
			Assert.AreEqual(0xFFFFu, ByteUtil.BytesToUInt16(new byte[] { 0xFF, 0xFF }, Endian.Little));
			Assert.AreEqual(0xFFFFu, ByteUtil.BytesToUInt16(new byte[] { 0xFF, 0xFF }, Endian.Big));
			Assert.AreEqual(0x0000u, ByteUtil.BytesToUInt16(new byte[] { 0x00, 0x00 }, Endian.Little));
			Assert.AreEqual(0x0000u, ByteUtil.BytesToUInt16(new byte[] { 0x00, 0x00 }, Endian.Big));
		}

		[TestMethod]
		public void TestBytesToUInt32()
		{
			Assert.AreEqual(0x34120000u, ByteUtil.BytesToUInt32(new byte[] { 0x00, 0x00, 0x12, 0x34 }, Endian.Little));
			Assert.AreEqual(0x34120000u, ByteUtil.BytesToUInt32(new byte[] { 0x34, 0x12, 0x00, 0x00 }, Endian.Big));
			Assert.AreEqual(0x3412FECDu, ByteUtil.BytesToUInt32(new byte[] { 0xCD, 0xFE, 0x12, 0x34 }, Endian.Little));
			Assert.AreEqual(0x3412FECDu, ByteUtil.BytesToUInt32(new byte[] { 0x34, 0x12, 0xFE, 0xCD }, Endian.Big));
			Assert.AreEqual(0xFFFFFFFFu, ByteUtil.BytesToUInt32(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, Endian.Little));
			Assert.AreEqual(0xFFFFFFFFu, ByteUtil.BytesToUInt32(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, Endian.Big));
		}

		[TestMethod]
		public void TestBytesToUInt64()
		{
			Assert.AreEqual(0x34125678ABCDEF23u, ByteUtil.BytesToUInt64(new byte[] { 0x23, 0xEF, 0xCD, 0xAB, 0x78, 0x56, 0x12, 0x34 }, Endian.Little));
			Assert.AreEqual(0x34125678ABCDEF23u, ByteUtil.BytesToUInt64(new byte[] { 0x34, 0x12, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x23 }, Endian.Big));
			Assert.AreEqual(0xFE125678ABCDEF23u, ByteUtil.BytesToUInt64(new byte[] { 0x23, 0xEF, 0xCD, 0xAB, 0x78, 0x56, 0x12, 0xFE }, Endian.Little));
			Assert.AreEqual(0xFE125678ABCDEF23u, ByteUtil.BytesToUInt64(new byte[] { 0xFE, 0x12, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x23 }, Endian.Big));
		}
	}
}
