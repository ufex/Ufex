using System;
using System.Collections.Generic;

namespace Ufex.Hex;

public class ColorProfile
{
	public string ID { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Description { get; set; }
	public List<string> FileTypePatterns { get; set; } = new List<string>();
	public List<ColorRule> Rules { get; set; } = new List<ColorRule>();

	/// <summary>
	/// Returns true if this profile applies to the given file type ID.
	/// A profile with no FileTypePatterns is global and applies to all file types.
	/// </summary>
	public bool AppliesToFileType(string fileTypeId)
	{
		if (FileTypePatterns.Count == 0)
			return true;

		foreach (var pattern in FileTypePatterns)
		{
			if (MatchesPattern(pattern, fileTypeId))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Returns true if this profile applies to any of the given file type IDs.
	/// Use this overload to include ancestor type IDs in the match.
	/// A profile with no FileTypePatterns is global and applies to all file types.
	/// </summary>
	public bool AppliesToFileType(IReadOnlyList<string> fileTypeIds)
	{
		if (FileTypePatterns.Count == 0)
			return true;

		foreach (var pattern in FileTypePatterns)
		{
			foreach (var id in fileTypeIds)
			{
				if (MatchesPattern(pattern, id))
					return true;
			}
		}

		return false;
	}

	private static bool MatchesPattern(string pattern, string fileTypeId)
	{
		if (pattern.EndsWith("*"))
		{
			var prefix = pattern.Substring(0, pattern.Length - 1);
			return fileTypeId.StartsWith(prefix, StringComparison.Ordinal);
		}
		return string.Equals(fileTypeId, pattern, StringComparison.Ordinal);
	}
}