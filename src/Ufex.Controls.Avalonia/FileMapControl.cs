using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Ufex.API.Visual;
using Ufex.API;

namespace Ufex.Controls.Avalonia;

/// <summary>
/// A control that draws a visual representation of file segments/spans.
/// </summary>
public class FileMapControl : Control
{
	private static readonly Color[] DefaultPalette = new[]
	{
		Color.Parse("#4E79A7"), // Blue
		Color.Parse("#F28E2C"), // Orange
		Color.Parse("#E15759"), // Red
		Color.Parse("#76B7B2"), // Teal
		Color.Parse("#59A14F"), // Green
		Color.Parse("#EDC949"), // Yellow
		Color.Parse("#AF7AA1"), // Purple
		Color.Parse("#FF9DA7"), // Pink
		Color.Parse("#9C755F"), // Brown
		Color.Parse("#BAB0AB")  // Gray
	};

	public static readonly StyledProperty<FileMap?> FileMapProperty =
		AvaloniaProperty.Register<FileMapControl, FileMap?>(nameof(FileMap));

	public static readonly StyledProperty<long> FileSizeProperty =
		AvaloniaProperty.Register<FileMapControl, long>(nameof(FileSize), 0L);

	public FileMap? FileMap
	{
		get => GetValue(FileMapProperty);
		set => SetValue(FileMapProperty, value);
	}

	public long FileSize
	{
		get => GetValue(FileSizeProperty);
		set => SetValue(FileSizeProperty, value);
	}

	static FileMapControl()
	{
		AffectsRender<FileMapControl>(FileMapProperty, FileSizeProperty);
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		var fileMap = FileMap;
		var fileSize = FileSize;

		if (fileMap == null || fileMap.Spans == null || fileMap.Spans.Length == 0 || fileSize <= 0)
		{
			return;
		}

		double width = Bounds.Width / 2;
		double height = Bounds.Height;
		double extraWidth = Bounds.Width / 2;

		var lineBrush = ActualThemeVariant == ThemeVariant.Dark ? Brushes.White : Brushes.Black;
		var blackPen = new Pen(lineBrush, 1);
		var typeface = new Typeface(FontFamily.Default);
		double fontSize = 12;
		var labelForeground = lineBrush;

		// Draw outer rectangle
		context.DrawRectangle(null, blackPen, new Rect(0, 0, width - 1, height - 1));

		// Track occupied vertical ranges for external labels (top, bottom)
		var occupiedRanges = new System.Collections.Generic.List<(double Top, double Bottom)>();

		for (int i = 0; i < fileMap.Spans.Length; i++)
		{
			var span = fileMap.Spans[i];

			// Use span color if specified, otherwise rotate through default palette
			Color fillColor;
			if (span.Color.HasValue)
				fillColor = Color.FromUInt32(span.Color.Value);
			else
				fillColor = DefaultPalette[i % DefaultPalette.Length];
			var fillBrush = new SolidColorBrush(fillColor);

			double start;
			double finish;

			if (span.StartPosition > 0)
				start = ((double)(span.StartPosition - 1) / fileSize) * height;
			else
				start = 0;

			finish = ((double)span.EndPosition / fileSize) * height;

			double sectHeight = finish - start;

			// Draw section rectangle and fill
			var sectionRect = new Rect(0, start, width - 1, sectHeight);
			context.DrawRectangle(fillBrush, blackPen, sectionRect);

			// Try to draw section name if there's room
			string? name = span.Name;
			if (!string.IsNullOrEmpty(name))
			{
				var formattedText = new FormattedText(
					name,
					System.Globalization.CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					typeface,
					fontSize,
					labelForeground);

				if (sectHeight >= formattedText.Height + 4)
				{
					// Enough vertical space - draw text inside the section
					double textX = (width - formattedText.Width) / 2;
					if (textX < 2) textX = 2;
					double textY = start + (sectHeight / 2) - (formattedText.Height / 2);

					context.DrawText(formattedText, new Point(textX, textY));
				}
				else
				{
					// Not enough space - draw line from vertical middle to label outside
					double sectionMiddleY = start + (sectHeight / 2);
					
					// Create the label text to measure its height
					var labelText = new FormattedText(
						name,
						System.Globalization.CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						typeface,
						fontSize * 0.9,
						labelForeground);

					double labelHeight = labelText.Height;
					double labelY = sectionMiddleY - 15; // Initial desired position
					double labelTop = labelY - (labelHeight / 2);
					double labelBottom = labelY + (labelHeight / 2);

					// Check for overlaps with existing labels and adjust
					bool hasOverlap;
					int maxIterations = 20; // Prevent infinite loop
					int iteration = 0;
					do
					{
						hasOverlap = false;
						foreach (var (occTop, occBottom) in occupiedRanges)
						{
							// Check if ranges overlap
							if (labelTop < occBottom && labelBottom > occTop)
							{
								// Move label below the occupied range
								labelTop = occBottom + 2;
								labelBottom = labelTop + labelHeight;
								labelY = labelTop + (labelHeight / 2);
								hasOverlap = true;
								break;
							}
						}
						iteration++;
					} while (hasOverlap && iteration < maxIterations);

					// Record this label's position
					occupiedRanges.Add((labelTop, labelBottom));

					// Draw diagonal line from middle of section to label area
					context.DrawLine(blackPen, new Point(width - 1, sectionMiddleY), new Point(width + (extraWidth / 2), labelY));

					// Draw the label text at the end of the line
					context.DrawText(labelText, new Point(width + (extraWidth / 2) + 2, labelTop));
				}
			}
		}
	}

	private static double GetSpanHeight(FileSpan span, long fileSize, double controlHeight)
	{
		double start;
		double finish;

		if (span.StartPosition > 0)
			start = ((double)(span.StartPosition - 1) / fileSize) * controlHeight;
		else
			start = 0;

		finish = ((double)span.EndPosition / fileSize) * controlHeight;
		return finish - start;
	}
}
