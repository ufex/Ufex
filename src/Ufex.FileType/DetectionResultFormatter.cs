using System.Text;

namespace Ufex.FileType;

/// <summary>
/// Formatting helpers for generating human-readable match detail text.
/// </summary>
public static class DetectionResultFormatter
{
	/// <summary>
	/// Formats match details as multi-line text suitable for display in a modal or tooltip.
	/// </summary>
	public static string FormatMatchDetails(DetectionMatch match)
	{
		var sb = new StringBuilder();

		if (match.Method.HasFlag(MatchMethod.Extension))
		{
			sb.AppendLine($"Extension matched: {match.FileType.ID} (.{match.MatchedExtension})");
		}

		if (match.Method.HasFlag(MatchMethod.Signature))
		{
			if (sb.Length > 0)
				sb.AppendLine();

			foreach (var sig in match.SignatureDetails)
			{
				sb.AppendLine($"Signature matched: {match.FileType.ID}");
				foreach (var rule in sig.MatchedRules)
				{
					sb.AppendLine($"  {rule}");
				}
				if (sig != match.SignatureDetails[^1])
					sb.AppendLine();
			}
		}

		return sb.ToString().TrimEnd();
	}

	/// <summary>
	/// Formats the match method as a short label for display in UI.
	/// </summary>
	public static string FormatMatchMethodLabel(MatchMethod method)
	{
		return method switch
		{
			MatchMethod.Extension | MatchMethod.Signature => "(extension + signature)",
			MatchMethod.Extension => "(extension)",
			MatchMethod.Signature => "(signature)",
			_ => "",
		};
	}
}
