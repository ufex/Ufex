using System;
using System.Collections.Generic;

namespace Ufex.Hex;

public class ColorRule
{
	public Expr Predicate { get; set; }

	/// <summary>
	/// The default color used when no theme-specific color is available,
	/// or when only a single color was specified in the profile.
	/// </summary>
	public UInt32 DefaultColor { get; set; }

	/// <summary>
	/// Theme-specific colors. Keys are theme names (e.g. "light", "dark").
	/// When a pair is specified in the DSL (e.g. #aaa/#bbb), the first is stored
	/// under "light" and the second under "dark".
	/// </summary>
	public Dictionary<string, UInt32> ThemeColors { get; set; } = new Dictionary<string, UInt32>();

	public ColorRule(Expr predicate, UInt32 defaultColor)
	{
		Predicate = predicate;
		DefaultColor = defaultColor;
	}

	public ColorRule(Expr predicate, UInt32 lightColor, UInt32 darkColor)
	{
		Predicate = predicate;
		DefaultColor = lightColor;
		ThemeColors["light"] = lightColor;
		ThemeColors["dark"] = darkColor;
	}

	/// <summary>
	/// Resolves the color for the given theme. Falls back to DefaultColor
	/// if the theme is not found in ThemeColors.
	/// </summary>
	public UInt32 GetColor(string? theme)
	{
		if (theme != null && ThemeColors.TryGetValue(theme, out var color))
			return color;
		return DefaultColor;
	}
}