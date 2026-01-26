using System.IO;

namespace Ufex.API.Visual;

public class RasterImage : Ufex.API.Visual.Image
{
	protected Stream stream;

	/// <summary>
	/// Gets the stream containing the raster image data (BMP).
	/// </summary>
	public Stream ImageStream { get { return stream; } }

	public RasterImage(Stream stream)
	{
		this.stream = stream;
	}

}
