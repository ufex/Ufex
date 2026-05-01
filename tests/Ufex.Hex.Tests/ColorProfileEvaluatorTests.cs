using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ufex.Hex;

namespace Ufex.Hex.Tests;

[TestClass]
public class ColorProfileEvaluatorTests
{
	private static ColorProfile ParseProfile(string body)
	{
		return ColorProfileParser.Parse($"ID: test\nName: Test\n---\n{body}");
	}

	// ================================================================
	// Basic rule matching
	// ================================================================

	[TestMethod]
	public void Evaluate_WildcardMatchesAll()
	{
		var profile = ParseProfile("_ => #FF0000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x00, 0x42, 0xFF ];

		var colors = evaluator.Evaluate(buffer, 0, 3);

		Assert.AreEqual(3, colors.Length);
		Assert.AreEqual(0xFF0000FFu, colors[0]);
		Assert.AreEqual(0xFF0000FFu, colors[1]);
		Assert.AreEqual(0xFF0000FFu, colors[2]);
	}

	[TestMethod]
	public void Evaluate_ExactByteMatch()
	{
		var profile = ParseProfile("b == 0x42 => #00FF00\n_ => #FF0000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x00, 0x42, 0xFF ];

		var colors = evaluator.Evaluate(buffer, 0, 3);

		Assert.AreEqual(0xFF0000FFu, colors[0]); // 0x00 hits wildcard
		Assert.AreEqual(0x00FF00FFu, colors[1]); // 0x42 hits exact
		Assert.AreEqual(0xFF0000FFu, colors[2]); // 0xFF hits wildcard
	}

	[TestMethod]
	public void Evaluate_FirstMatchWins()
	{
		var profile = ParseProfile("b == 0x42 => #111111\nb >= 0x00 => #222222\n_ => #333333");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x42 ];

		var colors = evaluator.Evaluate(buffer, 0, 1);

		Assert.AreEqual(0x111111FFu, colors[0]); // first rule wins
	}

	[TestMethod]
	public void Evaluate_NoMatch_ReturnsZero()
	{
		var profile = ParseProfile("b == 0xFF => #AABBCC");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x00 ];

		var colors = evaluator.Evaluate(buffer, 0, 1);

		Assert.AreEqual(0u, colors[0]);
	}

	// ================================================================
	// Comparison operators
	// ================================================================

	[TestMethod]
	public void Evaluate_RangeComparison()
	{
		var profile = ParseProfile("b >= 0x20 && b < 0x7F => #00FF00\n_ => #FF0000");
		var evaluator = new ColorProfileEvaluator(profile);

		byte[] buffer = [ 0x1F, 0x20, 0x41, 0x7E, 0x7F ];
		var colors = evaluator.Evaluate(buffer, 0, 5);

		Assert.AreEqual(0xFF0000FFu, colors[0]); // 0x1F < 0x20
		Assert.AreEqual(0x00FF00FFu, colors[1]); // 0x20 in range
		Assert.AreEqual(0x00FF00FFu, colors[2]); // 0x41 in range
		Assert.AreEqual(0x00FF00FFu, colors[3]); // 0x7E in range
		Assert.AreEqual(0xFF0000FFu, colors[4]); // 0x7F not < 0x7F
	}

	[TestMethod]
	public void Evaluate_NotEqual()
	{
		var profile = ParseProfile("b != 0x00 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x00, 0x01 ];

		var colors = evaluator.Evaluate(buffer, 0, 2);

		Assert.AreEqual(0x000000FFu, colors[0]);
		Assert.AreEqual(0xAABBCCFFu, colors[1]);
	}

	// ================================================================
	// Bitwise operators
	// ================================================================

	[TestMethod]
	public void Evaluate_BitwiseAnd()
	{
		var profile = ParseProfile("(b & 0xF0) == 0xA0 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0xA0, 0xAF, 0xBF ];

		var colors = evaluator.Evaluate(buffer, 0, 3);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // 0xA0 & 0xF0 = 0xA0
		Assert.AreEqual(0xAABBCCFFu, colors[1]); // 0xAF & 0xF0 = 0xA0
		Assert.AreEqual(0x000000FFu, colors[2]); // 0xBF & 0xF0 = 0xB0
	}

	[TestMethod]
	public void Evaluate_BitwiseOr()
	{
		var profile = ParseProfile("(b | 0x0F) == 0xFF => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0xF0, 0x00 ];

		var colors = evaluator.Evaluate(buffer, 0, 2);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // 0xF0 | 0x0F = 0xFF
		Assert.AreEqual(0x000000FFu, colors[1]); // 0x00 | 0x0F = 0x0F
	}

	[TestMethod]
	public void Evaluate_BitwiseXor()
	{
		var profile = ParseProfile("(b ^ 0xFF) == 0x00 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0xFF, 0x00 ];

		var colors = evaluator.Evaluate(buffer, 0, 2);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // 0xFF ^ 0xFF = 0x00
		Assert.AreEqual(0x000000FFu, colors[1]); // 0x00 ^ 0xFF = 0xFF
	}

	[TestMethod]
	public void Evaluate_ShiftLeft()
	{
		var profile = ParseProfile("(b << 4) == 0x10 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x01, 0x02 ];

		var colors = evaluator.Evaluate(buffer, 0, 2);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // 1 << 4 = 16 = 0x10
		Assert.AreEqual(0x000000FFu, colors[1]); // 2 << 4 = 32
	}

	[TestMethod]
	public void Evaluate_ShiftRight()
	{
		var profile = ParseProfile("(b >> 4) == 0x0A => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0xA0, 0xB0 ];

		var colors = evaluator.Evaluate(buffer, 0, 2);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // 0xA0 >> 4 = 0x0A
		Assert.AreEqual(0x000000FFu, colors[1]); // 0xB0 >> 4 = 0x0B
	}

	// ================================================================
	// Unary operators
	// ================================================================

	[TestMethod]
	public void Evaluate_LogicalNot()
	{
		var profile = ParseProfile("!(b == 0x00) => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x00, 0x01 ];

		var colors = evaluator.Evaluate(buffer, 0, 2);

		Assert.AreEqual(0x000000FFu, colors[0]); // !(true) = false
		Assert.AreEqual(0xAABBCCFFu, colors[1]); // !(false) = true
	}

	[TestMethod]
	public void Evaluate_BitwiseNot()
	{
		var profile = ParseProfile("(~b & 0xFF) == 0xFE => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x01, 0x02 ];

		var colors = evaluator.Evaluate(buffer, 0, 2);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // ~0x01 & 0xFF = 0xFE
		Assert.AreEqual(0x000000FFu, colors[1]); // ~0x02 & 0xFF = 0xFD
	}

	[TestMethod]
	public void Evaluate_Negate()
	{
		var profile = ParseProfile("at(-1) == 0x42 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x42, 0x00 ];

		var colors = evaluator.Evaluate(buffer, 1, 1);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // at(-1) looks at buffer[0] = 0x42
	}

	// ================================================================
	// at() neighbor access
	// ================================================================

	[TestMethod]
	public void Evaluate_AtPositiveOffset()
	{
		var profile = ParseProfile("at(1) == 0xBB => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0xAA, 0xBB, 0xCC ];

		var colors = evaluator.Evaluate(buffer, 0, 2);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // at(1) from pos 0 = buffer[1] = 0xBB
		Assert.AreEqual(0x000000FFu, colors[1]); // at(1) from pos 1 = buffer[2] = 0xCC
	}

	[TestMethod]
	public void Evaluate_AtNegativeOffset()
	{
		var profile = ParseProfile("at(-1) == 0xAA => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0xAA, 0xBB, 0xCC ];

		var colors = evaluator.Evaluate(buffer, 1, 2);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // at(-1) from pos 1 = buffer[0] = 0xAA
		Assert.AreEqual(0x000000FFu, colors[1]); // at(-1) from pos 2 = buffer[1] = 0xBB
	}

	[TestMethod]
	public void Evaluate_AtOutOfBounds_ReturnsFalse()
	{
		var profile = ParseProfile("at(-1) == 0x00 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x42 ];

		var colors = evaluator.Evaluate(buffer, 0, 1);

		// at(-1) from pos 0 is OOB; comparison returns false
		Assert.AreEqual(0x000000FFu, colors[0]);
	}

	// ================================================================
	// Logical short-circuit
	// ================================================================

	[TestMethod]
	public void Evaluate_AndShortCircuit_FalseSkipsRight()
	{
		// If left is false, right is not evaluated (even if it would error)
		var profile = ParseProfile("b == 0xFF && at(-100) == 0x00 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x00 ];

		var colors = evaluator.Evaluate(buffer, 0, 1);

		Assert.AreEqual(0x000000FFu, colors[0]); // b != 0xFF, short-circuits
	}

	[TestMethod]
	public void Evaluate_OrShortCircuit_TrueSkipsRight()
	{
		var profile = ParseProfile("b == 0x42 || at(-100) == 0x00 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x42 ];

		var colors = evaluator.Evaluate(buffer, 0, 1);

		Assert.AreEqual(0xAABBCCFFu, colors[0]); // b == 0x42, short-circuits
	}

	// ================================================================
	// Theme support
	// ================================================================

	[TestMethod]
	public void Evaluate_LightTheme_UsesLightColor()
	{
		var profile = ParseProfile("_ => #111111/#222222");
		var evaluator = new ColorProfileEvaluator(profile, "light");
		byte[] buffer = [ 0x00 ];

		var colors = evaluator.Evaluate(buffer, 0, 1);

		Assert.AreEqual(0x111111FFu, colors[0]);
	}

	[TestMethod]
	public void Evaluate_DarkTheme_UsesDarkColor()
	{
		var profile = ParseProfile("_ => #111111/#222222");
		var evaluator = new ColorProfileEvaluator(profile, "dark");
		byte[] buffer = [ 0x00 ];

		var colors = evaluator.Evaluate(buffer, 0, 1);

		Assert.AreEqual(0x222222FFu, colors[0]);
	}

	[TestMethod]
	public void Evaluate_NoTheme_UsesDefaultColor()
	{
		var profile = ParseProfile("_ => #111111/#222222");
		var evaluator = new ColorProfileEvaluator(profile, null);
		byte[] buffer = [ 0x00 ];

		var colors = evaluator.Evaluate(buffer, 0, 1);

		// Default is the light color (first in pair)
		Assert.AreEqual(0x111111FFu, colors[0]);
	}

	// ================================================================
	// EvaluateSingle
	// ================================================================

	[TestMethod]
	public void EvaluateSingle_ReturnsCorrectColor()
	{
		var profile = ParseProfile("b == 0x42 => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);
		byte[] buffer = [ 0x00, 0x42, 0xFF ];

		Assert.AreEqual(0x000000FFu, evaluator.EvaluateSingle(buffer, 0));
		Assert.AreEqual(0xAABBCCFFu, evaluator.EvaluateSingle(buffer, 1));
		Assert.AreEqual(0x000000FFu, evaluator.EvaluateSingle(buffer, 2));
	}

	// ================================================================
	// Display offset within buffer
	// ================================================================

	[TestMethod]
	public void Evaluate_WithDisplayOffset()
	{
		var profile = ParseProfile("b == 0xBB => #AABBCC\n_ => #000000");
		var evaluator = new ColorProfileEvaluator(profile);

		// Context bytes before, then display bytes
		byte[] buffer = [ 0x00, 0x00, 0xBB, 0xCC, 0x00 ];

		// Display starts at offset 2, count 2
		var colors = evaluator.Evaluate(buffer, 2, 2);

		Assert.AreEqual(2, colors.Length);
		Assert.AreEqual(0xAABBCCFFu, colors[0]); // buffer[2] = 0xBB
		Assert.AreEqual(0x000000FFu, colors[1]); // buffer[3] = 0xCC
	}
}
