using System.Collections.Generic;

namespace Ufex.API.Tree;

/// <summary>
/// Describes a single context menu action available on a tree node.
/// </summary>
public class NodeAction
{
	/// <summary>
	/// Unique identifier for the action within the node (e.g. "extract", "export-csv").
	/// </summary>
	public required string Id { get; init; }

	/// <summary>
	/// Display text for the context menu item (e.g. "Extract...").
	/// </summary>
	public required string Label { get; init; }

	/// <summary>
	/// Determines menu grouping and separator placement.
	/// </summary>
	public NodeActionGroup Group { get; init; } = NodeActionGroup.Primary;

	/// <summary>
	/// Whether the action is currently available. Disabled actions appear greyed out.
	/// </summary>
	public bool IsEnabled { get; init; } = true;

	/// <summary>
	/// Tooltip text shown when hovering a disabled action.
	/// </summary>
	public string? DisabledReason { get; init; }

	/// <summary>
	/// Describes user inputs the host must collect before executing.
	/// Empty list means no input is needed.
	/// </summary>
	public IReadOnlyList<ActionInput> Inputs { get; init; } = [];
}
