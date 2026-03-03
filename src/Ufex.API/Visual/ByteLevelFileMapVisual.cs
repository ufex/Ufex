using System.ComponentModel;
using System.IO;
using System.Text;

namespace Ufex.API.Visual;

/// <summary>
/// A visual representation of a file at the byte level, where each byte is represented by a label index that maps to a color and description. 
/// This visual can be used to create a "file map" that shows the structure of a file, with different colors representing different types of data 
/// (e.g., code, data, padding). The LabelIndexStream contains indices that correspond to the Labels array, which defines the color and description 
/// for each label index. The Size property indicates the total size of the file being visualized, and the BytesPerRow property determines how many 
/// bytes are represented in each row of the visual, affecting the layout and aspect ratio of the file map.
/// </summary>
public class ByteLevelFileMapVisual : Visual
{
	public struct Label
	{
		/// <summary>
		/// The color associated with this label, represented as a 32-bit unsigned 
		/// integer in ARGB format (e.g., 0xFFFF0000 for opaque red).
		/// Leave null to use a default color palette.
		/// </summary>
		public uint? Color { get; set; }
		public string Description { get; set; }
	}

	/// <summary>
	/// The labels used for the file map, where each entry defines a color and description for a specific label index.
	/// </summary>
	public Label[] Labels { get; set; }

	/// <summary>
	/// The stream containing the label indices for the file map.
	/// </summary>
	public Stream LabelIndexStream { get; set; }

	/// <summary>
	/// The total size of the file.
	/// </summary>
	public ulong Size { get; set; }

	/// <summary>
	/// The number of bytes represented by each row in the visual. 
	/// This determines how the file map is laid out visually, with each 
	/// row representing a contiguous block of bytes from the file. For example, 
	/// if BytesPerRow is 256, then each row in the visual will represent 256 bytes 
	/// from the file. Adjusting this value will change the aspect ratio of the 
	/// visual and how data is grouped together visually.
	/// </summary>
	public ushort BytesPerRow { get; set; } = 256;

	public ByteLevelFileMapVisual(Stream labelIndexStream, ulong size, Label[] labels) : base("File Map")
	{
		LabelIndexStream = labelIndexStream;
		Size = size;
		Labels = labels;
	}

	public ByteLevelFileMapVisual(Stream labelIndexStream, ulong size, Label[] labels, string description) : base(description)
	{
		LabelIndexStream = labelIndexStream;
		Size = size;
		Labels = labels;
	}
}
