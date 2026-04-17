using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace Ufex.Controls.Avalonia;

/// <summary>
/// Lightweight control that renders hex view content via a DrawingContext callback.
/// Has no children, no layout overhead — just a single Render pass.
/// </summary>
public class HexViewPanel : Control
{
	/// <summary>
	/// Callback invoked during Render to draw content.
	/// </summary>
	public Action<DrawingContext>? RenderCallback { get; set; }

	public override void Render(DrawingContext context)
	{
		RenderCallback?.Invoke(context);
	}

	/// <summary>
	/// Triggers a repaint of this panel.
	/// </summary>
	public void Repaint()
	{
		InvalidateVisual();
	}
}
