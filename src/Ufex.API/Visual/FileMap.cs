using System.ComponentModel;
using System.IO;
using System.Text;

namespace Ufex.API.Visual;

public class FileMap : Visual
{
	/// <summary>
	/// The spans that make up the file map.
	/// </summary>
	public Ufex.API.FileSpan[] Spans { get; set; }

	/// <summary>
	/// The total size of the file.
	/// </summary>
	public ulong Size { get; set; }

	public FileMap(Ufex.API.FileSpan[] spans, ulong size) : base("File Map")
	{
		Spans = spans;
		Size = size;
	}
}
