using System.IO;
using System.Text;

namespace Ufex.API.Visual;

public class FileMap : VectorImage
{
	public API.Visual.FileSpan[] Spans { get; set; }

	public FileMap(API.Visual.FileSpan[] spans) : base(new MemoryStream())
	{
		Spans = spans;
	}

	private void Draw()
	{
		using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
		{
			writer.Write("Some data");
			writer.Flush();
		}
	}
}
