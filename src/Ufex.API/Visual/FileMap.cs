using System.ComponentModel;
using System.IO;
using System.Text;

namespace Ufex.API.Visual;

public class FileMap : Visual
{
	public API.Visual.FileSpan[] Spans { get; set; }
	public ulong Size { get; set; }

	public FileMap(API.Visual.FileSpan[] spans, ulong size) : base("File Map")
	{
		Spans = spans;
		Size = size;
	}
}
