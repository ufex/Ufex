using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Ufex.API.Visual;

public class VectorImage : Ufex.API.Visual.Image
{
	private Stream _stream;

	/// <summary>
	/// Gets the stream containing the vector image data (SVG).
	/// </summary>
	public Stream SvgStream { get { return stream; } }

	public VectorImage(Stream stream) : this(stream, "Image");

	public VectorImage(Stream stream, string description) : base(description)
	{
		_stream = stream;
	}
}
