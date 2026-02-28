using System.IO;

namespace Ufex.API.Visual;

/// <summary>
/// A visual representation of raw bytes for display in a hex viewer.
/// </summary>
public class HexViewerVisual : Visual
{
	/// <summary>
	/// Gets the stream containing the raw bytes to display.
	/// </summary>
	public Stream Stream { get; }

	/// <summary>
	/// Gets the starting offset within the file (for display purposes).
	/// </summary>
	public long StartOffset { get; }

	public HexViewerVisual(Stream stream, string description = "Hex View") : base(description)
	{
		Stream = stream;
		StartOffset = 0;
	}

	public HexViewerVisual(Stream stream, long startOffset, string description = "Hex View") : base(description)
	{
		Stream = stream;
		StartOffset = startOffset;
	}
}
