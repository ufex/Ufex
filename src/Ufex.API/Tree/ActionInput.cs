namespace Ufex.API.Tree;

/// <summary>
/// Describes a single piece of user input the host must gather before executing an action.
/// The plugin declares what it needs; the host decides how to present the UI.
/// </summary>
public abstract class ActionInput
{
	/// <summary>
	/// Key used to retrieve the collected value from <see cref="ActionContext.Inputs"/>.
	/// </summary>
	public required string Key { get; init; }

	/// <summary>
	/// Prompt text displayed to the user when gathering this input.
	/// </summary>
	public required string Prompt { get; init; }
}

/// <summary>
/// Requests a file path via a save-file dialog.
/// The collected value is a <see cref="string"/> file path.
/// </summary>
public class SaveFileInput : ActionInput
{
	/// <summary>
	/// Suggested default file name for the save dialog.
	/// </summary>
	public string? SuggestedFileName { get; init; }

	/// <summary>
	/// Optional file type filters for the save dialog.
	/// </summary>
	public FileFilter[]? Filters { get; init; }
}

/// <summary>
/// Requests a folder path via a folder-picker dialog.
/// The collected value is a <see cref="string"/> folder path.
/// </summary>
public class FolderInput : ActionInput
{
}

/// <summary>
/// Requests user confirmation via an OK/Cancel dialog.
/// The collected value is a <see cref="bool"/> (true if confirmed).
/// </summary>
public class ConfirmInput : ActionInput
{
}

/// <summary>
/// Describes a file type filter for save/open file dialogs.
/// </summary>
public class FileFilter
{
	/// <summary>
	/// Display name for the filter (e.g. "CSV Files").
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// File extensions without dots (e.g. ["csv", "txt"]).
	/// </summary>
	public required string[] Extensions { get; init; }
}
