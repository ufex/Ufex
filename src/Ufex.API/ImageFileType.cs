using System;
using System.Drawing;

namespace Ufex.API
{
	public abstract class ImageFileType : FileType
	{
		Image image;
		public int Width;
		public int Height;

		public ImageFileType()
		{
			image = null;
		}

		public override Image GetImage()
		{
			image = null;
			try
			{
				m_FileStream.Position = 0;
				image = Image.FromStream(m_FileStream);
			}
			catch (System.Runtime.InteropServices.ExternalException e)
			{
				ExceptionOut(e);
				DebugOut("Error drawing Image: " + e.ToString());
			}
			catch (Exception e)
			{
				ExceptionOut(e);
				DebugOut("An error occured while drawing the image: " + e.ToString());
			}
			return image;
		}
	}
}
