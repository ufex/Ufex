namespace Ufex.API.Tree;

/// <summary>
/// Describes the outcome of a node action execution.
/// </summary>
public class ActionResult
{
	/// <summary>
	/// Whether the action succeeded, failed, or was cancelled.
	/// </summary>
	public ActionResultStatus Status { get; init; }

	/// <summary>
	/// Optional status message shown to the user.
	/// </summary>
	public string? Message { get; init; }

	/// <summary>
	/// When true, the host resets <see cref="TreeNode.ChildrenLoaded"/> and
	/// re-invokes <see cref="TreeNode.LoadChildren"/> to reflect changes.
	/// </summary>
	public bool RefreshChildren { get; init; }

	public static ActionResult Success(string? message = null) =>
		new() { Status = ActionResultStatus.Success, Message = message };

	public static ActionResult Failure(string message) =>
		new() { Status = ActionResultStatus.Failure, Message = message };
}

/// <summary>
/// Indicates the outcome of a node action execution.
/// </summary>
public enum ActionResultStatus
{
	Success,
	Failure,
	Cancelled
}
