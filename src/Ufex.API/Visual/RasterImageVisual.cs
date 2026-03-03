using System.IO;

namespace Ufex.API.Visual;

public class RasterImageVisual : Ufex.API.Visual.ImageVisual
{
	private Stream _stream;

	/// <summary>
	/// Gets the stream containing the raster image data (BMP or PNG).
	/// </summary>
	public Stream ImageStream 
	{ 
		get { return _stream; }
		protected set { _stream = value; }
	}

	public RasterImageVisual(Stream stream) : this(stream, "Image")
	{
	}

	public RasterImageVisual(Stream stream, string description) : base(description)
	{
		ImageStream = stream;
	}

}
