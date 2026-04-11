using System;
using System.IO;

namespace Ufex.API.Visual;

[Obsolete("RasterImage is deprecated. Use RasterImageVisual instead.", false)]
public class RasterImage : Ufex.API.Visual.ImageVisual
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

	public RasterImage(Stream stream) : this(stream, "Image")
	{
	}

	public RasterImage(Stream stream, string description) : base(description)
	{
		ImageStream = stream;
	}

}
