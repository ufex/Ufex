using System;
using System.Collections.Generic;

namespace Ufex.FileType;

/// <summary>
/// How a file type was matched.
/// </summary>
[Flags]
public enum MatchMethod
{
	None      = 0,
	Extension = 1,
	Signature = 2,
}

/// <summary>
/// Describes a single signature rule/node that contributed to a match.
/// </summary>
public class SignatureMatchDetail
{
	/// <summary>
	/// Index of the Signature element within the FileTypeRecord.Signatures list (0-based).
	/// </summary>
	public int SignatureIndex { get; set; }

	/// <summary>
	/// Human-readable descriptions of the rules that matched within the signature.
	/// Each entry describes one Rule/SearchRule/RuleRef at the top level of the signature.
	/// Example: "Rule(type=bytes, offset=0) 0x89504E470D0A1A0A"
	/// Example: "SearchRule(type=regex, offset=0, maxLength=51200) \\x50\\x4B..."
	/// </summary>
	public List<string> MatchedRules { get; set; } = new();
}

/// <summary>
/// Captures a detected file type together with metadata about how it was detected.
/// </summary>
public class DetectionMatch
{
	/// <summary>The matched file type record.</summary>
	public FileTypeRecord FileType { get; set; }

	/// <summary>Flags indicating which methods contributed to the match.</summary>
	public MatchMethod Method { get; set; }

	/// <summary>
	/// The file extension that matched (e.g. "docx"), or null if extension didn't match.
	/// </summary>
	public string? MatchedExtension { get; set; }

	/// <summary>
	/// Details about which signatures matched. Empty if signature didn't match.
	/// </summary>
	public List<SignatureMatchDetail> SignatureDetails { get; set; } = new();
}

/// <summary>
/// The complete result from file type detection, containing all matches with details.
/// </summary>
public class DetectionResult
{
	/// <summary>All matches, sorted by specificity (most specific first).</summary>
	public List<DetectionMatch> Matches { get; set; } = new();

	/// <summary>Convenience: the number of matches.</summary>
	public int Count => Matches.Count;

	/// <summary>Convenience: the first (most specific) match, or null.</summary>
	public DetectionMatch? Best => Matches.Count > 0 ? Matches[0] : null;
}
