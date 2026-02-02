using System;
using System.Collections;
using System.Collections.Generic;

namespace Ufex.API.Tree;

public class TreeNodeCollection : IEnumerable<TreeNode>
{
	private readonly List<TreeNode> inner = new();
	private readonly TreeNode? owner;

	public TreeNodeCollection()
	{
		owner = null;
	}

	internal TreeNodeCollection(TreeNode owner)
	{
		this.owner = owner;
	}

	public int Count => inner.Count;

	public TreeNode this[int index]
	{
		get => inner[index];
		set
		{
			if (inner[index] != null)
			{
				inner[index].Parent = null;
			}
			inner[index] = value;
			value.Parent = owner;
		}
	}

	public TreeNode Add(TreeNode node)
	{
		node.Parent = owner;
		inner.Add(node);
		return node;
	}

	public TreeNode Add(string text)
	{
		var node = new TreeNode(text) { Parent = owner };
		inner.Add(node);
		return node;
	}

	public TreeNode Add(string text, TreeViewIcon imageIndex, TreeViewIcon selectedImageIndex)
	{
		var node = new TreeNode(text, imageIndex, selectedImageIndex) { Parent = owner };
		inner.Add(node);
		return node;
	}

	public void AddRange(TreeNode[] nodes)
	{
		foreach (var node in nodes)
		{
			Add(node);
		}
	}

	public int IndexOf(TreeNode node) => inner.IndexOf(node);

	public bool Contains(TreeNode node) => inner.Contains(node);

	public void Insert(int index, TreeNode node)
	{
		node.Parent = owner;
		inner.Insert(index, node);
	}

	public void Remove(TreeNode node)
	{
		if (inner.Remove(node))
		{
			node.Parent = null;
		}
	}

	public void RemoveAt(int index)
	{
		if (index >= 0 && index < inner.Count)
		{
			inner[index].Parent = null;
			inner.RemoveAt(index);
		}
	}

	public void Clear()
	{
		foreach (var node in inner)
		{
			node.Parent = null;
		}
		inner.Clear();
	}

	public IEnumerator<TreeNode> GetEnumerator() => inner.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();
}
