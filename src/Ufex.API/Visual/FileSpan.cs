using System;

namespace Ufex.API.Visual;

/// <summary>
/// Represents a span or region within a file, defined by start and end positions.
/// </summary>
public struct FileSpan
{
	public Int64 StartPosition;
	public Int64 EndPosition;
	
	public FileSpan(Int64 startPosition, Int64 endPosition) 
	{ 
		StartPosition = startPosition;
		EndPosition = endPosition; 
	}
}
