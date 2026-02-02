using System;
using System.Dynamic;

namespace Ufex.API;

/// <summary>
/// Represents a span or region within a file, defined by start and end positions.
/// </summary>
public struct FileSpan
{
	/// <summary>
	/// The starting position of the span in the file.
	/// </summary>
	public long StartPosition { get; set; }
	
	/// <summary>
	/// The ending position of the span in the file.
	/// </summary>
	public long EndPosition { get; set; }

	/// <summary>
	/// An optional name or label for the span.
	/// </summary>
	public string? Name { get; set; }
	
	/// <summary>
	/// The color associated with the span.
	/// </summary>
	public uint? Color { get; set; } // ARGB color value (0xAARRGGBB format)
	
	public FileSpan(long startPosition, long endPosition) 
	{ 
		StartPosition = startPosition;
		EndPosition = endPosition;
		Name = null;
		Color = null;
	}

	public FileSpan(long startPosition, long endPosition, string? name, uint? color = null)
	{
		StartPosition = startPosition;
		EndPosition = endPosition;
		Name = name;
		Color = color;
	}
}
