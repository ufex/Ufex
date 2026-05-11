namespace Ufex.API.Tree;

/// <summary>
/// Reports progress for a long-running node action.
/// </summary>
public class ActionProgress
{
	/// <summary>
	/// Optional status message describing current progress.
	/// </summary>
	public string? Message { get; init; }

	/// <summary>
	/// Progress percentage from 0.0 to 1.0, or null for indeterminate.
	/// </summary>
	public double? PercentComplete { get; init; }
}
