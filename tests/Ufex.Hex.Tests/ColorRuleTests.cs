using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ufex.Hex;

namespace Ufex.Hex.Tests;

[TestClass]
public class ColorRuleTests
{
	// ================================================================
	// Single-color constructor
	// ================================================================

	[TestMethod]
	public void Constructor_SingleColor_SetsDefaultColor()
	{
		var rule = new ColorRule(new WildcardExpr(), 0xAABBCCFFu);

		Assert.AreEqual(0xAABBCCFFu, rule.DefaultColor);
		Assert.AreEqual(0, rule.ThemeColors.Count);
	}

	// ================================================================
	// Light/Dark constructor
	// ================================================================

	[TestMethod]
	public void Constructor_LightDark_SetsBothColors()
	{
		var rule = new ColorRule(new WildcardExpr(), 0x111111FFu, 0x222222FFu);

		Assert.AreEqual(0x111111FFu, rule.DefaultColor);
		Assert.AreEqual(0x111111FFu, rule.ThemeColors["light"]);
		Assert.AreEqual(0x222222FFu, rule.ThemeColors["dark"]);
	}

	// ================================================================
	// GetColor
	// ================================================================

	[TestMethod]
	public void GetColor_NullTheme_ReturnsDefaultColor()
	{
		var rule = new ColorRule(new WildcardExpr(), 0x111111FFu, 0x222222FFu);

		Assert.AreEqual(0x111111FFu, rule.GetColor(null));
	}

	[TestMethod]
	public void GetColor_LightTheme_ReturnsLightColor()
	{
		var rule = new ColorRule(new WildcardExpr(), 0x111111FFu, 0x222222FFu);

		Assert.AreEqual(0x111111FFu, rule.GetColor("light"));
	}

	[TestMethod]
	public void GetColor_DarkTheme_ReturnsDarkColor()
	{
		var rule = new ColorRule(new WildcardExpr(), 0x111111FFu, 0x222222FFu);

		Assert.AreEqual(0x222222FFu, rule.GetColor("dark"));
	}

	[TestMethod]
	public void GetColor_UnknownTheme_ReturnsDefaultColor()
	{
		var rule = new ColorRule(new WildcardExpr(), 0x111111FFu, 0x222222FFu);

		Assert.AreEqual(0x111111FFu, rule.GetColor("high-contrast"));
	}

	[TestMethod]
	public void GetColor_SingleColor_AlwaysReturnsDefaultColor()
	{
		var rule = new ColorRule(new WildcardExpr(), 0xAABBCCFFu);

		Assert.AreEqual(0xAABBCCFFu, rule.GetColor(null));
		Assert.AreEqual(0xAABBCCFFu, rule.GetColor("light"));
		Assert.AreEqual(0xAABBCCFFu, rule.GetColor("dark"));
	}
}
