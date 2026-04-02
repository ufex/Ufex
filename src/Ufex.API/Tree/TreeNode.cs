using System;
using System.Collections;
using System.Collections.Generic;
using Ufex.API.Visual;

namespace Ufex.API.Tree;

/// <summary>
/// Represents a node in a tree structure.
/// Designed to be structurally similar to System.Windows.Forms.TreeNode
/// to serve as an abstraction layer for different UI frameworks.
/// </summary>
public class TreeNode
{
	private readonly TreeNodeCollection nodes;

	/// <summary>
	/// Gets or sets the path separator string used in the FullPath property.
	/// </summary>
	public static string PathSeparator { get; set; } = "\\";

	/// <summary>
	/// Gets the array of visuals to display when the tree node is selected.
	/// </summary>
	public virtual Ufex.API.Visual.Visual[] Visuals => [];

	/// <summary>
	/// Gets the array of visuals to display when the tree node is selected,
	/// with access to the file context for on-the-fly data reading.
	/// The default implementation returns the value of the <see cref="Visuals"/> property
	/// for backwards compatibility with existing tree node subclasses.
	/// </summary>
	/// <param name="context">The file context providing access to the file stream and reader.</param>
	/// <returns>An array of visuals to display for this node.</returns>
	public virtual Ufex.API.Visual.Visual[] GetVisuals(IFileContext context) => Visuals;

	/// <summary>
	/// Indicates this node has children that will be generated on demand
	/// via <see cref="LoadChildren"/>. When true and <see cref="Nodes"/> is empty,
	/// the UI will show an expand arrow and call <see cref="LoadChildren"/> when expanded.
	/// </summary>
	public virtual bool HasDeferredChildren => false;

	/// <summary>
	/// Gets whether <see cref="LoadChildren"/> has already been called.
	/// </summary>
	public bool ChildrenLoaded { get; private set; }

	/// <summary>
	/// Called by the UI when the user expands this node for the first time.
	/// Override to populate <see cref="Nodes"/> with child TreeNodes.
	/// The default implementation is a no-op. Always call <c>base.LoadChildren(context)</c>
	/// at the end of your override to mark children as loaded.
	/// </summary>
	/// <param name="context">The file context providing access to the file stream and reader.</param>
	public virtual void LoadChildren(IFileContext context)
	{
		ChildrenLoaded = true;
	}

	public TreeNode()
	{
		Text = string.Empty;
		nodes = new TreeNodeCollection(this);
	}

	public TreeNode(string text)
	{
		Text = text;
		nodes = new TreeNodeCollection(this);
	}

	public TreeNode(string text, TreeViewIcon icon, TreeViewIcon selectedIcon)
	{
		Text = text;
		Icon = icon;
		SelectedIcon = selectedIcon;
		nodes = new TreeNodeCollection(this);
	}

	public TreeNode(string text, TreeNode[] children)
	{
		Text = text;
		nodes = new TreeNodeCollection(this);
		foreach(var child in children)
		{
			nodes.Add(child);
		}
	}

	/// <summary>
	/// Gets or sets the text displayed in the tree node.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// Gets or sets the object that contains data about the tree node.
	/// </summary>
	public object? Tag { get; set; }

	/// <summary>
	/// Gets or sets the image displayed when the tree node is in the unselected state.
	/// </summary>
	public TreeViewIcon Icon { get; set; } = TreeViewIcon.NullIcon;

	/// <summary>
	/// Gets or sets the image displayed when the tree node is in the selected state.
	/// </summary>
	public TreeViewIcon SelectedIcon { get; set; } = TreeViewIcon.NullIcon;

	/// <summary>
	/// Gets the parent tree node of the current tree node.
	/// </summary>
	public TreeNode? Parent { get; internal set; }

	/// <summary>
	/// Gets the collection of TreeNode objects assigned to the current tree node.
	/// </summary>
	public TreeNodeCollection Nodes => nodes;

	/// <summary>
	/// Gets the zero-based index of the tree node within the tree node collection.
	/// </summary>
	public int Index
	{
		get
		{
			if (Parent == null)
			{
				return -1;
			}
			return Parent.Nodes.IndexOf(this);
		}
	}

	/// <summary>
	/// Gets the path from the root tree node to the current tree node.
	/// </summary>
	public string FullPath
	{
		get
		{
			if (Parent == null)
			{
				return Text;
			}
			return Parent.FullPath + PathSeparator + Text;
		}
	}

	/// <summary>
	/// Gets the first child tree node in the tree node collection.
	/// </summary>
	public TreeNode? FirstNode => nodes.Count > 0 ? nodes[0] : null;

	/// <summary>
	/// Gets the last child tree node in the tree node collection.
	/// </summary>
	public TreeNode? LastNode => nodes.Count > 0 ? nodes[nodes.Count - 1] : null;

	/// <summary>
	/// Gets the next sibling tree node.
	/// </summary>
	public TreeNode? NextNode
	{
		get
		{
			if (Parent == null) return null;
			int index = Index;
			if (index < 0 || index >= Parent.Nodes.Count - 1) return null;
			return Parent.Nodes[index + 1];
		}
	}

	/// <summary>
	/// Gets the previous sibling tree node.
	/// </summary>
	public TreeNode? PrevNode
	{
		get
		{
			if (Parent == null) return null;
			int index = Index;
			if (index <= 0) return null;
			return Parent.Nodes[index - 1];
		}
	}

	/// <summary>
	/// Gets the number of child tree nodes.
	/// </summary>
	public int GetNodeCount(bool includeSubTrees)
	{
		int count = nodes.Count;
		if (includeSubTrees)
		{
			foreach (var node in nodes)
			{
				count += node.GetNodeCount(true);
			}
		}
		return count;
	}

	/// <summary>
	/// Removes the current tree node from the tree view.
	/// </summary>
	public void Remove()
	{
		Parent?.Nodes.Remove(this);
	}

	public override string ToString() => Text;
}

public delegate void TreeNodeMouseClickEventHandler(object? sender, TreeNodeMouseClickEventArgs e);

public sealed class TreeNodeMouseClickEventArgs : EventArgs
{
	public TreeNodeMouseClickEventArgs(TreeNode node)
	{
		Node = node;
	}

	public TreeNode Node { get; }
}
