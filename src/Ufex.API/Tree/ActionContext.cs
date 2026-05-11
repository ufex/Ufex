using System;
using System.Collections.Generic;
using System.Threading;

namespace Ufex.API.Tree;

/// <summary>
/// Provides context for executing a node action, including file access,
/// collected user inputs, progress reporting, and cancellation.
/// </summary>
public class ActionContext
{
	/// <summary>
	/// File context providing access to the file stream and number format.
	/// </summary>
	public required IFileContext FileContext { get; init; }

	/// <summary>
	/// Collected user inputs keyed by <see cref="ActionInput.Key"/>.
	/// Value types: <see cref="string"/> for SaveFileInput and FolderInput;
	/// <see cref="bool"/> for ConfirmInput.
	/// </summary>
	public IReadOnlyDictionary<string, object> Inputs { get; init; } = new Dictionary<string, object>();

	/// <summary>
	/// Optional progress reporter for long-running actions.
	/// </summary>
	public IProgress<ActionProgress>? Progress { get; init; }

	/// <summary>
	/// Cancellation token allowing the user to cancel the action.
	/// </summary>
	public CancellationToken CancellationToken { get; init; }

	/// <summary>
	/// Retrieves a collected input value by key.
	/// </summary>
	public T GetInput<T>(string key) => (T)Inputs[key];
}
