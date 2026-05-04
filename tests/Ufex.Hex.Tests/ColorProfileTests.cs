using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ufex.Hex;

namespace Ufex.Hex.Tests;

[TestClass]
public class ColorProfileTests
{
	// ================================================================
	// AppliesToFileType (single string)
	// ================================================================

	[TestMethod]
	public void AppliesToFileType_EmptyPatterns_MatchesEverything()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string>() };

		Assert.IsTrue(profile.AppliesToFileType("PNG"));
		Assert.IsTrue(profile.AppliesToFileType("TEXT_JSON"));
		Assert.IsTrue(profile.AppliesToFileType(""));
	}

	[TestMethod]
	public void AppliesToFileType_ExactMatch()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string> { "PNG" } };

		Assert.IsTrue(profile.AppliesToFileType("PNG"));
		Assert.IsFalse(profile.AppliesToFileType("JPEG"));
		Assert.IsFalse(profile.AppliesToFileType("PNG_APNG"));
	}

	[TestMethod]
	public void AppliesToFileType_WildcardMatch()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string> { "TEXT_*" } };

		Assert.IsTrue(profile.AppliesToFileType("TEXT_JSON"));
		Assert.IsTrue(profile.AppliesToFileType("TEXT_UTF8"));
		Assert.IsTrue(profile.AppliesToFileType("TEXT_"));
		Assert.IsFalse(profile.AppliesToFileType("TEXT"));
		Assert.IsFalse(profile.AppliesToFileType("PNG"));
	}

	[TestMethod]
	public void AppliesToFileType_MultiplePatterns()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string> { "PNG", "JPEG", "GIF" } };

		Assert.IsTrue(profile.AppliesToFileType("PNG"));
		Assert.IsTrue(profile.AppliesToFileType("JPEG"));
		Assert.IsTrue(profile.AppliesToFileType("GIF"));
		Assert.IsFalse(profile.AppliesToFileType("BMP"));
	}

	[TestMethod]
	public void AppliesToFileType_MixedExactAndWildcard()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string> { "PNG", "TEXT_*" } };

		Assert.IsTrue(profile.AppliesToFileType("PNG"));
		Assert.IsTrue(profile.AppliesToFileType("TEXT_JSON"));
		Assert.IsFalse(profile.AppliesToFileType("JPEG"));
	}

	// ================================================================
	// AppliesToFileType (list of strings — ancestor chain)
	// ================================================================

	[TestMethod]
	public void AppliesToFileType_List_MatchesAnyInChain()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string> { "ARCHIVE" } };
		var chain = new List<string> { "ZIP_OOXML_DOCX", "ZIP", "ARCHIVE" };

		Assert.IsTrue(profile.AppliesToFileType((IReadOnlyList<string>)chain));
	}

	[TestMethod]
	public void AppliesToFileType_List_WildcardMatchesAncestor()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string> { "TEXT_*" } };
		var chain = new List<string> { "TEXT_JSON", "TEXT_UTF8" };

		Assert.IsTrue(profile.AppliesToFileType((IReadOnlyList<string>)chain));
	}

	[TestMethod]
	public void AppliesToFileType_List_NoMatch()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string> { "IMG_*" } };
		var chain = new List<string> { "TEXT_JSON", "TEXT_UTF8" };

		Assert.IsFalse(profile.AppliesToFileType((IReadOnlyList<string>)chain));
	}

	[TestMethod]
	public void AppliesToFileType_List_EmptyPatterns_AlwaysMatches()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string>() };
		var chain = new List<string> { "ANYTHING" };

		Assert.IsTrue(profile.AppliesToFileType((IReadOnlyList<string>)chain));
	}

	[TestMethod]
	public void AppliesToFileType_List_EmptyChain_NoMatch()
	{
		var profile = new ColorProfile { FileTypePatterns = new List<string> { "PNG" } };
		var chain = new List<string>();

		Assert.IsFalse(profile.AppliesToFileType((IReadOnlyList<string>)chain));
	}
}
