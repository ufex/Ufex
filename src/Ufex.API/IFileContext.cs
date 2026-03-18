using System.IO;
using Ufex.API.Format;

namespace Ufex.API;

/// <summary>
/// Provides context about the current file to tree nodes when rendering visuals.
/// This allows tree nodes to read data from the file on-the-fly rather than
/// storing all data in memory upfront.
/// </summary>
public interface IFileContext
{
	/// <summary>
	/// Gets the file stream for reading data from the current file.
	/// </summary>
	FileStream FileStream { get; }

	/// <summary>
	/// Gets the current number format used for displaying numeric values.
	/// </summary>
	NumberFormat NumberFormat { get; }
}
