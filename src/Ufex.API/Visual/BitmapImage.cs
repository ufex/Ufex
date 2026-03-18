using System;
using System.IO;

namespace Ufex.API.Visual;

public class BitmapImage
{
	public int Width { get; protected set; }
	public int Height { get; protected set; }

	/// <summary>
	/// Gets the stream containing the bitmap data.
	/// </summary>
	public MemoryStream ImageStream { get; protected set; }

	public BitmapImage(int width, int height)
	{
		Width = width;
		Height = height;
		ImageStream = new MemoryStream();
		
		// Initialize a valid Windows BMP with white canvas
		InitializeBitmap();
	}

	private void InitializeBitmap()
	{
		// Calculate row stride (each row must be padded to multiple of 4 bytes)
		int rowStride = ((Width * 3 + 3) / 4) * 4;
		int pixelDataSize = rowStride * Height;
		int fileSize = 54 + pixelDataSize; // 14 (file header) + 40 (info header) + pixel data

		using (var writer = new BinaryWriter(ImageStream, System.Text.Encoding.UTF8, leaveOpen: true))
		{
			// BITMAPFILEHEADER (14 bytes)
			writer.Write((byte)'B');
			writer.Write((byte)'M');
			writer.Write(fileSize);            // File size
			writer.Write((UInt16)0);           // Reserved1
			writer.Write((UInt16)0);           // Reserved2
			writer.Write((UInt32)54);          // Offset to pixel data

			// BITMAPINFOHEADER (40 bytes)
			writer.Write((UInt32)40);          // Header size
			writer.Write((Int32)Width);        // Width
			writer.Write((Int32)Height);       // Height (positive = bottom-up)
			writer.Write((UInt16)1);           // Planes
			writer.Write((UInt16)24);          // Bits per pixel
			writer.Write((UInt32)0);           // Compression (BI_RGB)
			writer.Write((UInt32)pixelDataSize); // Image size
			writer.Write((Int32)0);            // X pixels per meter
			writer.Write((Int32)0);            // Y pixels per meter
			writer.Write((UInt32)0);           // Colors used
			writer.Write((UInt32)0);           // Important colors

			// Write white pixels (BGR format: 255, 255, 255)
			byte[] whiteRow = new byte[rowStride];
			for (int i = 0; i < Width * 3; i++)
			{
				whiteRow[i] = 255;
			}
			// Padding bytes are already 0 from array initialization

			for (int y = 0; y < Height; y++)
			{
				writer.Write(whiteRow);
			}
		}

		ImageStream.Position = 0;
	}

	/// <summary>
	/// Sets pixel data for a rectangular region of the bitmap.
	/// </summary>
	/// <param name="offsetX">The X coordinate of the top-left corner of the region.</param>
	/// <param name="offsetY">The Y coordinate of the top-left corner of the region.</param>
	/// <param name="pixelData">The pixel data to set. Each pixel should be an unsigned 32-bit integer representing the ARGB components.</param>
	/// <exception cref="ArgumentException"></exception>
	public void SetPixels(int offsetX, int offsetY, UInt32[][] pixelData)
	{
		if (pixelData == null || pixelData.Length == 0)
			throw new ArgumentException("Pixel data cannot be null or empty.");

		int regionHeight = pixelData.Length;
		int regionWidth = pixelData[0].Length;

		// Validate bounds
		if (offsetX < 0 || offsetY < 0)
			throw new ArgumentException("Offset coordinates cannot be negative.");
		
		if (offsetX + regionWidth > Width || offsetY + regionHeight > Height)
			throw new ArgumentException("Pixel data region exceeds bitmap dimensions.");

		// Calculate row stride
		int rowStride = ((Width * 3 + 3) / 4) * 4;

		// Update bitmap data
		long originalPosition = ImageStream.Position;
		
		for (int y = 0; y < regionHeight; y++)
		{
			if (pixelData[y].Length != regionWidth)
				throw new ArgumentException("All rows in pixel data must have the same width.");

			int bmpY = offsetY + y;
			// BMP is bottom-up, so flip Y coordinate
			int fileRow = Height - 1 - bmpY;
			long rowOffset = 54 + (fileRow * rowStride) + (offsetX * 3);

			ImageStream.Position = rowOffset;

			for (int x = 0; x < regionWidth; x++)
			{
				UInt32 argb = pixelData[y][x];
				byte r = (byte)((argb >> 16) & 0xFF);
				byte g = (byte)((argb >> 8) & 0xFF);
				byte b = (byte)(argb & 0xFF);

				// BMP uses BGR format
				ImageStream.WriteByte(b);
				ImageStream.WriteByte(g);
				ImageStream.WriteByte(r);
			}
		}

		ImageStream.Position = originalPosition;
	}

	/// <summary>
	/// Sets the color of a single pixel in the bitmap.
	/// </summary>
	/// <param name="x">The X coordinate of the pixel.</param>
	/// <param name="y">The Y coordinate of the pixel.</param>
	/// <param name="r">The red component of the pixel.</param>
	/// <param name="g">The green component of the pixel.</param>
	/// <param name="b">The blue component of the pixel.</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public void SetPixel(int x, int y, byte r, byte g, byte b)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			throw new ArgumentOutOfRangeException("Pixel coordinates are out of bounds.");

		// Calculate row stride
		int rowStride = ((Width * 3 + 3) / 4) * 4;

		// BMP is bottom-up, so flip Y coordinate
		int fileRow = Height - 1 - y;
		long pixelOffset = 54 + (fileRow * rowStride) + (x * 3);

		long originalPosition = ImageStream.Position;
		ImageStream.Position = pixelOffset;

		// BMP uses BGR format
		ImageStream.WriteByte(b);
		ImageStream.WriteByte(g);
		ImageStream.WriteByte(r);

		ImageStream.Position = originalPosition;
	}

}
