using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ufex.Hex;

namespace Ufex.Hex.Tests;

[TestClass]
public class ColorProfileParserTests
{
	private static string MakeProfile(string body, string header = "ID: test\nName: Test Profile")
	{
		return $"{header}\n---\n{body}";
	}

	// ================================================================
	// Header parsing
	// ================================================================

	[TestMethod]
	public void Parse_MinimalProfile()
	{
		string input = MakeProfile("b == 0x00 => #FF0000");
		var profile = ColorProfileParser.Parse(input);

		Assert.AreEqual("test", profile.ID);
		Assert.AreEqual("Test Profile", profile.Name);
		Assert.AreEqual(1, profile.Rules.Count);
	}

	[TestMethod]
	public void Parse_WithDescription()
	{
		string input = MakeProfile(
			"_ => #000000",
			"ID: test\nName: Test\nDescription: A test profile");
		var profile = ColorProfileParser.Parse(input);

		Assert.AreEqual("A test profile", profile.Description);
	}

	[TestMethod]
	public void Parse_WithFileTypes()
	{
		string input = MakeProfile(
			"_ => #000000",
			"ID: test\nName: Test\nFileTypes: PNG, JPEG, TEXT_*");
		var profile = ColorProfileParser.Parse(input);

		Assert.AreEqual(3, profile.FileTypePatterns.Count);
		Assert.AreEqual("PNG", profile.FileTypePatterns[0]);
		Assert.AreEqual("JPEG", profile.FileTypePatterns[1]);
		Assert.AreEqual("TEXT_*", profile.FileTypePatterns[2]);
	}

	[TestMethod]
	public void Parse_MissingID_Throws()
	{
		Assert.ThrowsException<ColorProfileParseException>(
			() => ColorProfileParser.Parse("Name: Test\n---\n_ => #000000"));
	}

	[TestMethod]
	public void Parse_MissingName_Throws()
	{
		Assert.ThrowsException<ColorProfileParseException>(
			() => ColorProfileParser.Parse("ID: test\n---\n_ => #000000"));
	}

	[TestMethod]
	public void Parse_MissingSeparator_Throws()
	{
		Assert.ThrowsException<ColorProfileParseException>(
			() => ColorProfileParser.Parse("ID: test\nName: Test\n_ => #000000"));
	}

	[TestMethod]
	public void Parse_DuplicateKey_Throws()
	{
		Assert.ThrowsException<ColorProfileParseException>(
			() => ColorProfileParser.Parse("ID: test\nID: test2\nName: Test\n---\n_ => #000000"));
	}

	[TestMethod]
	public void Parse_EmptyFileTypes_Throws()
	{
		Assert.ThrowsException<ColorProfileParseException>(
			() => ColorProfileParser.Parse("ID: test\nName: Test\nFileTypes: \n---\n_ => #000000"));
	}

	[TestMethod]
	public void Parse_NoRules_Throws()
	{
		Assert.ThrowsException<ColorProfileParseException>(
			() => ColorProfileParser.Parse("ID: test\nName: Test\n---\n"));
	}

	// ================================================================
	// Rule body parsing
	// ================================================================

	[TestMethod]
	public void Parse_WildcardRule()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("_ => #AABBCC"));

		Assert.AreEqual(1, profile.Rules.Count);
		Assert.IsInstanceOfType(profile.Rules[0].Predicate, typeof(WildcardExpr));
	}

	[TestMethod]
	public void Parse_ByteEqualityRule()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("b == 0xFF => #112233"));

		Assert.AreEqual(1, profile.Rules.Count);
		Assert.IsInstanceOfType(profile.Rules[0].Predicate, typeof(BinaryExpr));
		var bin = (BinaryExpr)profile.Rules[0].Predicate;
		Assert.AreEqual(BinaryOp.Equal, bin.Op);
		Assert.IsInstanceOfType(bin.Left, typeof(CurrentByteExpr));
	}

	[TestMethod]
	public void Parse_BitwiseAndMask()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("(b & 0xF0) == 0xA0 => #AABBCC"));
		Assert.AreEqual(1, profile.Rules.Count);
	}

	[TestMethod]
	public void Parse_AtWithNegativeOffset()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("at(-1) == 0x00 => #AABBCC"));

		var rule = profile.Rules[0];
		Assert.IsInstanceOfType(rule.Predicate, typeof(BinaryExpr));
		var bin = (BinaryExpr)rule.Predicate;
		Assert.IsInstanceOfType(bin.Left, typeof(AtExpr));
		var atExpr = (AtExpr)bin.Left;
		Assert.IsInstanceOfType(atExpr.Offset, typeof(UnaryExpr));
		var neg = (UnaryExpr)atExpr.Offset;
		Assert.AreEqual(UnaryOp.Negate, neg.Op);
	}

	[TestMethod]
	public void Parse_AtWithPositiveOffset()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("at(1) == 0x00 => #AABBCC"));

		var rule = profile.Rules[0];
		var bin = (BinaryExpr)rule.Predicate;
		var atExpr = (AtExpr)bin.Left;
		Assert.IsInstanceOfType(atExpr.Offset, typeof(IntegerExpr));
		Assert.AreEqual(1L, ((IntegerExpr)atExpr.Offset).Value);
	}

	[TestMethod]
	public void Parse_LogicalAnd()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("b >= 0x20 && b < 0x7F => #00FF00"));
		Assert.AreEqual(1, profile.Rules.Count);

		var bin = (BinaryExpr)profile.Rules[0].Predicate;
		Assert.AreEqual(BinaryOp.And, bin.Op);
	}

	[TestMethod]
	public void Parse_LogicalOr()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("b == 0x00 || b == 0xFF => #00FF00"));
		var bin = (BinaryExpr)profile.Rules[0].Predicate;
		Assert.AreEqual(BinaryOp.Or, bin.Op);
	}

	[TestMethod]
	public void Parse_NotExpression()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("!(b == 0x00) => #AABBCC"));
		Assert.IsInstanceOfType(profile.Rules[0].Predicate, typeof(UnaryExpr));
		var unary = (UnaryExpr)profile.Rules[0].Predicate;
		Assert.AreEqual(UnaryOp.Not, unary.Op);
	}

	[TestMethod]
	public void Parse_BitwiseNot()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("~b == 0x00 => #AABBCC"));
		// ~b creates a UnaryExpr(BitwiseNot) at the bit-level, then == 0x00
		var bin = (BinaryExpr)profile.Rules[0].Predicate;
		Assert.IsInstanceOfType(bin.Left, typeof(UnaryExpr));
	}

	[TestMethod]
	public void Parse_MultipleRules()
	{
		string body = "b == 0x00 => #111111\nb == 0xFF => #222222\n_ => #333333";
		var profile = ColorProfileParser.Parse(MakeProfile(body));
		Assert.AreEqual(3, profile.Rules.Count);
	}

	[TestMethod]
	public void Parse_CommentsAreIgnored()
	{
		string body = "# This is a comment\nb == 0x00 => #111111\n# Another comment\n_ => #222222";
		var profile = ColorProfileParser.Parse(MakeProfile(body));
		Assert.AreEqual(2, profile.Rules.Count);
	}

	// ================================================================
	// Color parsing
	// ================================================================

	[TestMethod]
	public void Parse_SingleColor_StoresAsDefaultColor()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("_ => #AABBCC"));
		var rule = profile.Rules[0];

		// AABBCCFF (fully opaque)
		Assert.AreEqual(0xAABBCCFFu, rule.DefaultColor);
		Assert.AreEqual(0, rule.ThemeColors.Count);
	}

	[TestMethod]
	public void Parse_ColorPair_StoresLightAndDark()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("_ => #112233/#445566"));
		var rule = profile.Rules[0];

		Assert.AreEqual(0x112233FFu, rule.DefaultColor);
		Assert.AreEqual(0x112233FFu, rule.ThemeColors["light"]);
		Assert.AreEqual(0x445566FFu, rule.ThemeColors["dark"]);
	}

	// ================================================================
	// Integer literal formats
	// ================================================================

	[TestMethod]
	public void Parse_HexLiteral()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("b == 0xFF => #000000"));
		var bin = (BinaryExpr)profile.Rules[0].Predicate;
		Assert.AreEqual(0xFF, ((IntegerExpr)bin.Right).Value);
	}

	[TestMethod]
	public void Parse_DecimalLiteral()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("b == 255 => #000000"));
		var bin = (BinaryExpr)profile.Rules[0].Predicate;
		Assert.AreEqual(255L, ((IntegerExpr)bin.Right).Value);
	}

	[TestMethod]
	public void Parse_BinaryLiteral()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("b == 0b11111111 => #000000"));
		var bin = (BinaryExpr)profile.Rules[0].Predicate;
		Assert.AreEqual(255L, ((IntegerExpr)bin.Right).Value);
	}

	// ================================================================
	// Shift operators
	// ================================================================

	[TestMethod]
	public void Parse_ShiftLeft()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("(b << 1) == 0x00 => #000000"));
		var cmp = (BinaryExpr)profile.Rules[0].Predicate;
		var shift = (BinaryExpr)cmp.Left;
		Assert.AreEqual(BinaryOp.ShiftLeft, shift.Op);
	}

	[TestMethod]
	public void Parse_ShiftRight()
	{
		var profile = ColorProfileParser.Parse(MakeProfile("(b >> 4) == 0x00 => #000000"));
		var cmp = (BinaryExpr)profile.Rules[0].Predicate;
		var shift = (BinaryExpr)cmp.Left;
		Assert.AreEqual(BinaryOp.ShiftRight, shift.Op);
	}

	// ================================================================
	// Full profile (utf8-like)
	// ================================================================

	[TestMethod]
	public void Parse_Utf8StyleProfile()
	{
		string input = @"ID: utf8test
Name: UTF-8 Test
Description: Test profile
FileTypes: TEXT_*
---
# Null bytes
b == 0x00 => #1a1a1a/#e5e5e5
# Printable ASCII
b >= 0x20 && b < 0x7F => #88cc88
# Lead byte + continuation
(b & 0xE0) == 0xC0 && (at(1) & 0xC0) == 0x80 => #4488cc
# Continuation preceded by lead
(at(-1) & 0xE0) == 0xC0 && (b & 0xC0) == 0x80 => #6699dd
# High bytes
b >= 0x80 => #cc8844
# Default
_ => #666666/#999999
";
		var profile = ColorProfileParser.Parse(input);

		Assert.AreEqual("utf8test", profile.ID);
		Assert.AreEqual("UTF-8 Test", profile.Name);
		Assert.AreEqual(1, profile.FileTypePatterns.Count);
		Assert.AreEqual("TEXT_*", profile.FileTypePatterns[0]);
		Assert.AreEqual(6, profile.Rules.Count);
	}
}
