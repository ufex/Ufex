using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ufex.API.Tests
{
	[TestClass()]
	public class DataManipTests
	{
		[TestMethod]
		public void GetBit()
		{
			Assert.AreEqual(true, DataManip.GetBit((Byte)2, 1));
			Assert.AreEqual(false, DataManip.GetBit((Byte)2, 0));
			Assert.AreEqual(false, DataManip.GetBit((UInt16)16384, 15));
			Assert.AreEqual(true, DataManip.GetBit((UInt16)16384, 14));
			Assert.AreEqual(false, DataManip.GetBit((UInt16)16384, 13));
		}

		[TestMethod]
		public void ToggleEndianUInt16()
		{
			Assert.AreEqual((UInt16)258, DataManip.SwapEndian((UInt16)513));
			Assert.AreEqual((UInt16)4386, DataManip.SwapEndian((UInt16)8721));
			Assert.AreEqual((UInt16)43707, DataManip.SwapEndian((UInt16)48042));
		}


		[TestMethod]
		public void ToggleEndianUInt32()
		{
			Assert.AreEqual(0x01020304u, DataManip.SwapEndian(0x04030201u));
			Assert.AreEqual(0x11223344u, DataManip.SwapEndian(0x44332211u));
		}

		[TestMethod]
		public void ToggleEndianUInt64()
		{
			Assert.AreEqual(0x0102030405060708u, DataManip.SwapEndian(0x0807060504030201u));
			Assert.AreEqual(0x1122334455667788u, DataManip.SwapEndian(0x8877665544332211u));
		}

		[TestMethod]
		public void ToggleEndianSInt16()
		{
			Assert.AreEqual((Int16)258, DataManip.SwapEndian((Int16)513));
			Assert.AreEqual((Int16)4386, DataManip.SwapEndian((Int16)8721));
			Assert.AreEqual((Int16)(-21829), DataManip.SwapEndian((Int16)(-17494)));
		}

		[TestMethod]
		public void ToggleEndianSInt32()
		{
			Assert.AreEqual(16909060, DataManip.SwapEndian(67305985));
			Assert.AreEqual(287454020, DataManip.SwapEndian(1144201745));
		}

		[TestMethod]
		public void ToggleEndianSInt64()
		{
			Assert.AreEqual((Int64)0x0102030405060708, DataManip.SwapEndian((Int64)0x0807060504030201));
			Assert.AreEqual((Int64)1234605616436508552, DataManip.SwapEndian((Int64)(-8613303245920329199)));
		}
	}
}
