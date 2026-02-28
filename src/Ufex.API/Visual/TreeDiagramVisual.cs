using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Ufex.API.Visual;

public class TreeDiagramVisual : Ufex.API.Visual.VectorImage
{
	public class Node
	{
		public string Label;
		public string BorderColor = "#000000";
		public string BackgroundColor = "#FFFFFF";
		public string TextColor = "#000000";
		public List<Node> Children;

		public Node(string label)
		{
			Label = label;
			Children = new List<Node>();
		}
	}

	// Layout configuration
	private const int NodeRadius = 30;
	private const int HorizontalSpacing = 20;
	private const int VerticalSpacing = 80;
	private const int Padding = 20;

	public TreeDiagramVisual(Node rootNode, string title = "Tree Diagram") 
		: base(GenerateSvg(rootNode), title)
	{
	}

	/// <summary>
	/// Generates an SVG stream containing a top-down tree diagram.
	/// </summary>
	/// <param name="rootNode">The root node of the tree.</param>
	/// <returns>A MemoryStream containing the SVG data.</returns>
	private static MemoryStream GenerateSvg(Node rootNode)
	{
		// First pass: calculate the width of each subtree
		var subtreeWidths = new Dictionary<int, int>();
		var nodePositions = new List<(Node node, int x, int y, int parentX, int parentY)>();

		// Calculate tree dimensions
		int treeWidth = CalculateSubtreeWidth(rootNode);
		int treeHeight = CalculateTreeHeight(rootNode);

		int svgWidth = treeWidth + (Padding * 2);
		int svgHeight = (treeHeight * (NodeRadius * 2 + VerticalSpacing)) + (Padding * 2);

		// Position all nodes starting from the root
		int rootX = svgWidth / 2;
		int rootY = Padding + NodeRadius;
		PositionNodes(rootNode, rootX, rootY, treeWidth, nodePositions, -1, -1);

		// Generate SVG
		var svgStream = new MemoryStream();
		var writer = new StreamWriter(svgStream, Encoding.UTF8);

		writer.WriteLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{svgWidth}\" height=\"{svgHeight}\" viewBox=\"0 0 {svgWidth} {svgHeight}\">");
		writer.WriteLine($"  <rect width=\"{svgWidth}\" height=\"{svgHeight}\" fill=\"#FFFFFF\" />");
		writer.WriteLine("  <style>");
		writer.WriteLine("    .node-text { font-family: Arial, sans-serif; font-size: 12px; text-anchor: middle; dominant-baseline: central; }");
		writer.WriteLine("    .edge { stroke: #000000; stroke-width: 2; fill: none; }");
		writer.WriteLine("  </style>");

		// Draw edges first (so they appear behind nodes)
		foreach (var (node, x, y, parentX, parentY) in nodePositions)
		{
			if (parentX >= 0 && parentY >= 0)
			{
				DrawEdge(writer, parentX, parentY + NodeRadius, x, y - NodeRadius);
			}
		}

		// Draw nodes
		foreach (var (node, x, y, parentX, parentY) in nodePositions)
		{
			DrawNode(writer, node, x, y);
		}

		writer.WriteLine("</svg>");
		writer.Flush();

		svgStream.Position = 0;
		return svgStream;
	}

	/// <summary>
	/// Calculates the width required for a subtree.
	/// </summary>
	private static int CalculateSubtreeWidth(Node node)
	{
		if (node.Children == null || node.Children.Count == 0)
		{
			return NodeRadius * 2;
		}

		int totalWidth = 0;
		foreach (var child in node.Children)
		{
			totalWidth += CalculateSubtreeWidth(child);
		}

		// Add spacing between children
		totalWidth += (node.Children.Count - 1) * HorizontalSpacing;

		// Ensure the width is at least as wide as the node itself
		return Math.Max(totalWidth, NodeRadius * 2);
	}

	/// <summary>
	/// Calculates the height of the tree (number of levels).
	/// </summary>
	private static int CalculateTreeHeight(Node node)
	{
		if (node.Children == null || node.Children.Count == 0)
		{
			return 1;
		}

		int maxChildHeight = 0;
		foreach (var child in node.Children)
		{
			maxChildHeight = Math.Max(maxChildHeight, CalculateTreeHeight(child));
		}

		return 1 + maxChildHeight;
	}

	/// <summary>
	/// Recursively positions all nodes in the tree.
	/// </summary>
	private static void PositionNodes(
		Node node,
		int centerX,
		int y,
		int availableWidth,
		List<(Node node, int x, int y, int parentX, int parentY)> positions,
		int parentX,
		int parentY)
	{
		positions.Add((node, centerX, y, parentX, parentY));

		if (node.Children == null || node.Children.Count == 0)
		{
			return;
		}

		// Calculate total width needed for all children
		var childWidths = new List<int>();
		int totalChildWidth = 0;
		foreach (var child in node.Children)
		{
			int childWidth = CalculateSubtreeWidth(child);
			childWidths.Add(childWidth);
			totalChildWidth += childWidth;
		}
		totalChildWidth += (node.Children.Count - 1) * HorizontalSpacing;

		// Position children centered under the parent
		int childY = y + NodeRadius * 2 + VerticalSpacing;
		int startX = centerX - (totalChildWidth / 2);
		int currentX = startX;

		for (int i = 0; i < node.Children.Count; i++)
		{
			int childWidth = childWidths[i];
			int childCenterX = currentX + (childWidth / 2);

			PositionNodes(node.Children[i], childCenterX, childY, childWidth, positions, centerX, y);

			currentX += childWidth + HorizontalSpacing;
		}
	}

	/// <summary>
	/// Draws an edge (line) between a parent and child node.
	/// </summary>
	private static void DrawEdge(StreamWriter writer, int x1, int y1, int x2, int y2)
	{
		writer.WriteLine($"  <line class=\"edge\" x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" />");
	}

	/// <summary>
	/// Draws a node as a circle with a text label.
	/// </summary>
	private static void DrawNode(StreamWriter writer, Node node, int x, int y)
	{
		// Draw circle
		writer.WriteLine($"  <circle cx=\"{x}\" cy=\"{y}\" r=\"{NodeRadius}\" fill=\"{EscapeXml(node.BackgroundColor)}\" stroke=\"{EscapeXml(node.BorderColor)}\" stroke-width=\"2\" />");

		// Draw label text
		string escapedLabel = EscapeXml(node.Label);
		writer.WriteLine($"  <text class=\"node-text\" x=\"{x}\" y=\"{y}\" fill=\"{EscapeXml(node.TextColor)}\">{escapedLabel}</text>");
	}

	/// <summary>
	/// Escapes special XML characters in a string.
	/// </summary>
	private static string EscapeXml(string text)
	{
		if (string.IsNullOrEmpty(text))
			return text;

		return text
			.Replace("&", "&amp;")
			.Replace("<", "&lt;")
			.Replace(">", "&gt;")
			.Replace("\"", "&quot;")
			.Replace("'", "&apos;");
	}
}
