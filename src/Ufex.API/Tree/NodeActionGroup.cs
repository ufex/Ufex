namespace Ufex.API.Tree;

/// <summary>
/// Determines how actions are grouped and separated in the context menu.
/// Actions are displayed in enum order with separators between groups.
/// </summary>
public enum NodeActionGroup
{
	Primary,
	Export,
	Edit,
	Destructive
}
