using System;
using System.Collections;
using System.Collections.Generic;

namespace Ufex.API
{
    public class TreeNode
    {
        private readonly TreeNodeCollection nodes;

        public TreeNode(string text)
        {
            Text = text;
            nodes = new TreeNodeCollection();
        }

        public string Text { get; set; }

        public object? Tag { get; set; }

        public TreeNodeCollection Nodes => nodes;
    }

    public class TreeNodeCollection : IEnumerable<TreeNode>
    {
        private readonly List<TreeNode> inner = new();

        public int Count => inner.Count;

        public TreeNode this[int index]
        {
            get => inner[index];
            set => inner[index] = value;
        }

        public TreeNode Add(TreeNode node)
        {
            inner.Add(node);
            return node;
        }

        public TreeNode Add(string text)
        {
            var node = new TreeNode(text);
            inner.Add(node);
            return node;
        }

        public IEnumerator<TreeNode> GetEnumerator() => inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();
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
}
