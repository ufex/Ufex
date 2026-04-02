using System.IO;
using Ufex.API.Format;

namespace Ufex.API;

/// <summary>
/// Default implementation of <see cref="IFileContext"/> that provides
/// file access context to tree nodes when rendering visuals.
/// </summary>
public class FileContext : IFileContext
{
	/// <inheritdoc/>
	public FileStream FileStream { get; }

	/// <inheritdoc/>
	public NumberFormat NumberFormat { get; }

	public FileContext(FileStream fileStream, NumberFormat numberFormat)
	{
		FileStream = fileStream;
		NumberFormat = numberFormat;
	}
}
