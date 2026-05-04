using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ufex.Hex;

namespace Ufex.Hex.Tests;

[TestClass]
public class HexFormatTests
{
	// ================================================================
	// Lookup table tests
	// ================================================================

	[TestMethod]
	public void HexUpper_AllValues_AreUppercaseTwoDigit()
	{
		Assert.AreEqual("00", HexFormat.HexUpper[0x00]);
		Assert.AreEqual("0A", HexFormat.HexUpper[0x0A]);
		Assert.AreEqual("4B", HexFormat.HexUpper[0x4B]);
		Assert.AreEqual("FF", HexFormat.HexUpper[0xFF]);
	}

	[TestMethod]
	public void HexLower_AllValues_AreLowercaseTwoDigit()
	{
		Assert.AreEqual("00", HexFormat.HexLower[0x00]);
		Assert.AreEqual("0a", HexFormat.HexLower[0x0A]);
		Assert.AreEqual("4b", HexFormat.HexLower[0x4B]);
		Assert.AreEqual("ff", HexFormat.HexLower[0xFF]);
	}

	[TestMethod]
	public void AsciiChars_PrintableChars_MapToThemselves()
	{
		Assert.AreEqual("A", HexFormat.AsciiChars[0x41]);
		Assert.AreEqual("z", HexFormat.AsciiChars[0x7A]);
		Assert.AreEqual(" ", HexFormat.AsciiChars[0x20]);
		Assert.AreEqual("~", HexFormat.AsciiChars[0x7E]);
	}

	[TestMethod]
	public void AsciiChars_NonPrintableChars_MapToDot()
	{
		Assert.AreEqual(".", HexFormat.AsciiChars[0x00]);
		Assert.AreEqual(".", HexFormat.AsciiChars[0x1F]);
		Assert.AreEqual(".", HexFormat.AsciiChars[0x7F]);
		Assert.AreEqual(".", HexFormat.AsciiChars[0xFF]);
	}

	[TestMethod]
	public void LookupTables_Have256Entries()
	{
		Assert.AreEqual(256, HexFormat.HexUpper.Length);
		Assert.AreEqual(256, HexFormat.HexLower.Length);
		Assert.AreEqual(256, HexFormat.AsciiChars.Length);
	}

	// ================================================================
	// FormatBytes tests
	// ================================================================

	[TestMethod]
	public void FormatBytes_BasicSequence()
	{
		byte[] data = [ 0x50, 0x4B, 0x03, 0x04 ];
		Assert.AreEqual("50 4B 03 04", HexFormat.FormatBytes(data, 0, 4));
	}

	[TestMethod]
	public void FormatBytes_WithOffset()
	{
		byte[] data = [ 0xAA, 0xBB, 0xCC, 0xDD ];
		Assert.AreEqual("CC DD", HexFormat.FormatBytes(data, 2, 2));
	}

	[TestMethod]
	public void FormatBytes_SingleByte()
	{
		byte[] data = [ 0xFF ];
		Assert.AreEqual("FF", HexFormat.FormatBytes(data, 0, 1));
	}

	[TestMethod]
	public void FormatBytes_ZeroCount_ReturnsEmpty()
	{
		byte[] data = [ 0x00 ];
		Assert.AreEqual(string.Empty, HexFormat.FormatBytes(data, 0, 0));
	}

	[TestMethod]
	public void FormatBytes_CountExceedsLength_ClampedToArrayEnd()
	{
		byte[] data = [ 0x01, 0x02 ];
		Assert.AreEqual("01 02", HexFormat.FormatBytes(data, 0, 10));
	}

	// ================================================================
	// ParseHexString tests
	// ================================================================

	[TestMethod]
	public void ParseHexString_CompactHex()
	{
		var result = HexFormat.ParseHexString("504B0304");
		CollectionAssert.AreEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, result);
	}

	[TestMethod]
	public void ParseHexString_SpaceSeparated()
	{
		var result = HexFormat.ParseHexString("50 4B 03 04");
		CollectionAssert.AreEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, result);
	}

	[TestMethod]
	public void ParseHexString_DashSeparated()
	{
		var result = HexFormat.ParseHexString("50-4B-03-04");
		CollectionAssert.AreEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, result);
	}

	[TestMethod]
	public void ParseHexString_ColonSeparated()
	{
		var result = HexFormat.ParseHexString("50:4B:03:04");
		CollectionAssert.AreEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, result);
	}

	[TestMethod]
	public void ParseHexString_OddLength_ReturnsNull()
	{
		Assert.IsNull(HexFormat.ParseHexString("ABC"));
	}

	[TestMethod]
	public void ParseHexString_EmptyString_ReturnsNull()
	{
		Assert.IsNull(HexFormat.ParseHexString(""));
	}

	[TestMethod]
	public void ParseHexString_InvalidChars_ReturnsNull()
	{
		Assert.IsNull(HexFormat.ParseHexString("ZZZZ"));
	}

	[TestMethod]
	public void ParseHexString_MixedCase()
	{
		var result = HexFormat.ParseHexString("aAbBcCdD");
		CollectionAssert.AreEqual(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, result);
	}

	// ================================================================
	// IsHexChar tests
	// ================================================================

	[TestMethod]
	public void IsHexChar_ValidDigits_ReturnTrue()
	{
		Assert.IsTrue(HexFormat.IsHexChar('0'));
		Assert.IsTrue(HexFormat.IsHexChar('9'));
		Assert.IsTrue(HexFormat.IsHexChar('a'));
		Assert.IsTrue(HexFormat.IsHexChar('f'));
		Assert.IsTrue(HexFormat.IsHexChar('A'));
		Assert.IsTrue(HexFormat.IsHexChar('F'));
	}

	[TestMethod]
	public void IsHexChar_InvalidChars_ReturnFalse()
	{
		Assert.IsFalse(HexFormat.IsHexChar('g'));
		Assert.IsFalse(HexFormat.IsHexChar('G'));
		Assert.IsFalse(HexFormat.IsHexChar(' '));
		Assert.IsFalse(HexFormat.IsHexChar('-'));
	}
}
