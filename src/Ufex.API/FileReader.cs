using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ufex.API;

/// <summary>
/// Binary file reader with support for endianness and various data types.
/// </summary>
public class FileReader
{
	private const Int32 MaxCharBytesSize = 128;

	private Stream stream;
	private Byte[] buffer;
	private System.Text.Decoder decoder;
	private Byte[] charBytes;
	private Char[] singleChar;
	private Char[] charBuffer;
	private int maxCharsSize;
	private bool littleEndian;
	private bool m_2BytesPerChar;

	public Stream BaseStream 
	{
		get { return stream; }
		set { stream = value; }
	}

	public FileReader(Stream input) : this(input, true, new UTF8Encoding()) { }

	public FileReader(Stream input, bool littleEndian) : this(input, littleEndian, new UTF8Encoding()) { }
	
	public FileReader(Stream input, bool littleEndian, System.Text.Encoding encoding) 
	{ 
		if (input==null) 
		{
			throw new ArgumentNullException("input");
		}
		if (encoding==null) 
		{
			throw new ArgumentNullException("encoding");
		}
		stream = input;
		decoder = encoding.GetDecoder();
		maxCharsSize = encoding.GetMaxCharCount(MaxCharBytesSize);
		int minBufferSize = encoding.GetMaxByteCount(1);  // max bytes per one char
		if (minBufferSize < 16) 
				minBufferSize = 16;
		buffer = new Byte[minBufferSize];
		this.littleEndian = littleEndian;
	}

	~FileReader()
	{
		this.Close();
		buffer = null;
		decoder = null;
		charBytes = null;
		singleChar = null;
	}

	public void Close()
	{
		System.IO.Stream tmpStream = stream;
		stream = null;
		if (tmpStream != null)
		{
			tmpStream.Close();
		}
	}

	public Int32 Read()
	{
		if (stream == null)
			FileNotOpen();

		return InternalReadOneChar();
	}

	public bool ReadBoolean()
	{
		FillBuffer(1);
		return !(buffer[0] == 0);
	}

	public Byte[] ReadBytes(Int32 count)
	{
		Byte[] arr;
		arr = new Byte[count];
		stream.ReadExactly(arr, 0, count);
		return arr;
	}

	public Byte[] ReadBytes(UInt16 count) 
	{ 
		return ReadBytes((int)count); 
	}

	public Char ReadChar()
	{
		Int32 i;
		i = this.Read();
		if (i == -1)
		{
			EndOfFile();
		}
		return (Char)i;
	}
	public Char[] ReadChars(Int32 count)
	{
		Char[] arr;
		Int32 i;
		Char[] arr1;

		if (stream == null)
		{
			FileNotOpen();
		}

		if (count < 0)
		{
			throw new System.ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
		}

		arr = new Char[count];
		i = InternalReadChars(arr, 0, count);

		if (i != count)
		{
			arr1 = new Char[i];
			Buffer.BlockCopy(arr, 0, arr1, 0, (2 * i));
			arr = arr1;
		}
		return arr;
	}

	// Read the signed integer types
	public Int16 ReadInt16()
	{
		FillBuffer(2);
		if (littleEndian)
			return (Int16)((buffer[0] & 255) | (buffer[1] << 8));
		else
			return (Int16)((buffer[1] & 255) | (buffer[0] << 8));
	}
	
	public Int32 ReadInt32()
	{
		FillBuffer(4);
		if (littleEndian)
			return (((buffer[0] & 255) | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24);
		else
			return (((buffer[3] & 255) | (buffer[2] << 8)) | (buffer[1] << 16)) | (buffer[0] << 24);
	}

	public Int64 ReadInt64()
	{
		UInt32 a, b;

		// Read 8 bytes
		FillBuffer(8);
		if (littleEndian)
		{
			a = (UInt32)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
			b = (UInt32)(((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 16)) | (buffer[7] << 24));
		}
		else
		{
			a = (UInt32)(((buffer[7] | (buffer[6] << 8)) | (buffer[5] << 16)) | (buffer[4] << 24));
			b = (UInt32)(((buffer[3] | (buffer[2] << 8)) | (buffer[1] << 16)) | (buffer[0] << 24));
		}
		return (Int64)((((UInt64)(b)) << 32) | ((UInt64)(a)));
	}

	public SByte ReadSByte()
	{
		FillBuffer(1);
		return (SByte)buffer[0];
	}

	// Read the unsinged integer types
	public Byte ReadByte()
	{
		FillBuffer(1);
		return buffer[0];
	}

	public UInt16 ReadUInt16()
	{
		FillBuffer(2);
		if (littleEndian)
			return (UInt16)((buffer[0] & 255) | (buffer[1] << 8));
		else
			return (UInt16)((buffer[0] << 8) | (buffer[1] & 255));
	}

	public UInt32 ReadUInt32()
	{
		FillBuffer(4);
		if (littleEndian)
			return (UInt32)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
		else
			return (UInt32)(((buffer[3] | (buffer[2] << 8)) | (buffer[1] << 16)) | (buffer[0] << 24));
	}

	public UInt64 ReadUInt64()
	{
		UInt32 a, b;
		FillBuffer(8);
		if (littleEndian)
		{
			a = (UInt32)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
			b = (UInt32)(((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 16)) | (buffer[7] << 24));
		}
		else
		{
			a = (UInt32)(((buffer[7] | (buffer[6] << 8)) | (buffer[5] << 16)) | (buffer[4] << 24));
			b = (UInt32)(((buffer[3] | (buffer[2] << 8)) | (buffer[1] << 16)) | (buffer[0] << 24));
		}
		return (((UInt64)(b)) << 32) | ((UInt64)(a));
	}

	// Reads a null terminated string
	/// <summary>
	/// Reads a null-terminated string from the current stream.
	/// </summary>
	/// <returns>String read from the stream</returns>
	public string ReadNullTermString()
	{
		byte lastByte;
		StringBuilder sb = new StringBuilder();

		do
		{
			lastByte = (byte)stream.ReadByte();
			if (lastByte != 0x00)
				sb.Append((Char)lastByte);

		} while (lastByte != 0x00);

		return sb.ToString();
	}

	public Guid ReadGuid()
	{
		FillBuffer(16);
		return new System.Guid(buffer);
	}

	protected void Init(System.IO.Stream input, bool littleEndian, System.Text.Encoding encoding)
	{

	}

	private void FillBuffer(Int32 numBytes)
	{
		Int32 i1 = 0;
		Int32 i = 0;

		if (this.stream == null)
		{
			// Error File Not Open
			FileNotOpen();
		}

		if (numBytes == 1)
		{
			i1 = this.stream.ReadByte();
			if (i1 == -1)
			{
				EndOfFile();
			}
			this.buffer[0] = (Byte)i1;
			return;
		}

		do
		{
			i1 = this.stream.Read(this.buffer, i, (numBytes - i));
			if (i1 == 0)
			{
				EndOfFile();
			}
			i += i1;
		} while (i < numBytes);
	}

	private Int32 InternalReadOneChar()
	{
		Int32 i = 0;
		Int32 i1 = 0;
		Int32 i2;

		if (charBytes == null)
		{
			charBytes = new Byte[MaxCharBytesSize];
		}

		if (singleChar == null)
		{
			singleChar = new Char[1];
		}

		while (i == 0)
		{
			i1 = m_2BytesPerChar ? 2 : 1;
			i2 = stream.ReadByte();
			charBytes[0] = (Byte)i2;
			if (i2 == -1)
			{
				i1 = 0;
			}

			if (i1 == 2)
			{
				i2 = stream.ReadByte();
				charBytes[1] = (Byte)i2;
				if (i2 == -1)
				{
					i1 = 1;
				}
			}

			if (i1 != 0)
			{
				i = decoder.GetChars(charBytes, 0, i1, singleChar, 0);
			}
			else
			{
				return -1;
			}
		}
		if (i != 0)
		{
			return singleChar[0];
		}

		return -1;
	}
	private Int32 InternalReadChars(Char[] buffer, Int32 index, Int32 count)
	{
		int i = 0;
		int i1 = 0;
		int i2 = count;
		if (charBytes == null)
		{
			charBytes = new Byte[i2];
		}
		do
		{
			i1 = i2;
			if (m_2BytesPerChar)
			{
				i1 = i1 << 1;
			}
			if (i1 > 128)
			{
				i1 = 128;
			}

			i1 = stream.Read(charBytes, 0, i1);
			if (i1 == 0)
			{
				return count - i2;
			}

			i = decoder.GetChars(charBytes, 0, i1, buffer, index);
			i2 -= i;
			index += i;
		} while (i2 > 0);

		return count;
	}

	private void EndOfFile() 
	{ 
		throw new Exception("EndOfFile"); 
	}
	private void FileNotOpen() 
	{ 
		throw new Exception("FileNotOpen"); 
	}

}
