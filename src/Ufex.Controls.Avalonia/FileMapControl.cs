using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Ufex.API.Visual;

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

		var blackPen = new Pen(Brushes.Black, 1);
		var typeface = new Typeface(FontFamily.Default);
		double fontSize = 12;

		// Draw outer rectangle
		context.DrawRectangle(null, blackPen, new Rect(0, 0, width - 1, height - 1));

		bool tooSmall = false;

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
					Brushes.Black);

				if (!tooSmall && sectHeight > formattedText.Height)
				{
					// Center the text in the section
					double textX = (width - formattedText.Width) / 2;
					if (textX < 0) textX = width / 3;
					double textY = ((start + finish) / 2) - (formattedText.Height / 2);

					context.DrawText(formattedText, new Point(textX, textY));
				}
				else
				{
					// Draw diagonal line to label outside the bar
					context.DrawLine(blackPen, new Point(width - 1, start), new Point(width + (extraWidth / 2), start - 25));

					// Draw the label text at the end of the line
					var labelText = new FormattedText(
						name,
						System.Globalization.CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						typeface,
						fontSize * 0.9,
						Brushes.Black);

					context.DrawText(labelText, new Point(width + (extraWidth / 2) + 2, start - 25 - (labelText.Height / 2)));

					// Check if next section fits
					double nextSectionHeight;
					if ((i + 1) < fileMap.Spans.Length)
						nextSectionHeight = GetSpanHeight(fileMap.Spans[i + 1], fileSize, height);
					else
						nextSectionHeight = fontSize + 4;

					if (nextSectionHeight > fontSize + 4)
					{
						tooSmall = true;
					}
					else
					{
						tooSmall = false;
						context.DrawLine(blackPen, new Point(width - 1, finish), new Point(width + (extraWidth / 2), finish + 25));
					}
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
