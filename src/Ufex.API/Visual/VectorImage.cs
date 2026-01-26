using System.IO;

namespace Ufex.API.Visual;

public class VectorImage : Ufex.API.Visual.Image
{
	protected Stream stream;

	/// <summary>
	/// Gets the stream containing the vector image data (SVG).
	/// </summary>
	public Stream SvgStream { get { return stream; } }

	public VectorImage(Stream stream)
	{
		this.stream = stream;
	}

}
