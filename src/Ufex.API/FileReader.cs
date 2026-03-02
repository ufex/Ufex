using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ufex.API.Types;

namespace Ufex.API;

/// <summary>
/// Enahnced binary stream reader with support for endianness and various data types.
/// </summary>
public class FileReader : BinaryReader
{
	private Endian _endian;

	/// <summary>
	/// Buffer for temporary storage of bytes.
	/// The BinaryReader's internal buffer is marked private, so we maintain our own.
	/// </summary>
	private byte[] _buffer;

	public Endian Endian
	{
		get { return _endian; }
		set { _endian = value; }
	}

	public FileReader(Stream input) : this(input, Endian.Little, new UTF8Encoding()) { }

	public FileReader(Stream input, Endian endian) : this(input, endian, new UTF8Encoding()) { }
	
	public FileReader(Stream input, Endian endian, System.Text.Encoding encoding) : base(input, encoding)
	{ 
		_endian = endian;
		int minBufferSize = encoding.GetMaxByteCount(1);  // max bytes per one char
		if(minBufferSize < 16) 
			minBufferSize = 16;
		_buffer = new byte[minBufferSize];
	}

	/// <summary>
	/// Peeks at the next byte in the stream without advancing the position.
	/// </summary>
	public virtual byte? PeekByte()
	{
		if(BaseStream == null) throw new ObjectDisposedException("stream");

		if(!BaseStream.CanSeek)
			return null;
		long origPos = BaseStream.Position;
		int b = BaseStream.ReadByte();
		BaseStream.Position = origPos;
		if(b == -1)
			return null;
		return (byte)b;
	}

	public Byte[] ReadBytes(UInt16 count) 
	{ 
		return ReadBytes((int)count);
	}
	
	/// <summary>
	/// Reads bytes from the stream until the specified termination byte is encountered.
	/// </summary>
	/// <param name="terminationByte"></param>
	/// <param name="includeTerminationByte"></param>
	/// <returns></returns>
	/// <exception cref="EndOfStreamException"></exception>
	public Byte[] ReadBytesUntil(byte terminationByte, bool includeTerminationByte = false)
	{
		List<byte> bytes = new List<byte>();
		while(true)
		{
			int b = BaseStream.ReadByte();
			if(b == -1)
				throw new EndOfStreamException();
			if((byte)b == terminationByte)
			{
				if(includeTerminationByte)
					bytes.Add((byte)b);
				break;
			}
			bytes.Add((byte)b);
		}
		return bytes.ToArray();
	}

	/// <summary>
	/// Reads a signed 16-bit integer from the current stream.
	/// </summary>
	public override Int16 ReadInt16()
	{
		FillBuffer2(2);
		if (_endian == Endian.Little)
			return (Int16)((_buffer[0] & 255) | (_buffer[1] << 8));
		else
			return (Int16)((_buffer[1] & 255) | (_buffer[0] << 8));
	}
	
	/// <summary>
	/// Reads a signed 32-bit integer from the current stream.
	/// </summary>
	public override Int32 ReadInt32()
	{
		FillBuffer2(4);
		if (_endian == Endian.Little)
			return (((_buffer[0] & 255) | (_buffer[1] << 8)) | (_buffer[2] << 16)) | (_buffer[3] << 24);
		else
			return (((_buffer[3] & 255) | (_buffer[2] << 8)) | (_buffer[1] << 16)) | (_buffer[0] << 24);
	}

	/// <summary>
	/// Reads a signed 64-bit integer from the current stream.
	/// </summary>
	public override Int64 ReadInt64()
	{
		UInt32 a, b;

		// Read 8 bytes
		FillBuffer2(8);
		if (_endian == Endian.Little)
		{
			a = (UInt32)(((_buffer[0] | (_buffer[1] << 8)) | (_buffer[2] << 16)) | (_buffer[3] << 24));
			b = (UInt32)(((_buffer[4] | (_buffer[5] << 8)) | (_buffer[6] << 16)) | (_buffer[7] << 24));
		}
		else
		{
			a = (UInt32)(((_buffer[7] | (_buffer[6] << 8)) | (_buffer[5] << 16)) | (_buffer[4] << 24));
			b = (UInt32)(((_buffer[3] | (_buffer[2] << 8)) | (_buffer[1] << 16)) | (_buffer[0] << 24));
		}
		return (Int64)((((UInt64)(b)) << 32) | ((UInt64)(a)));
	}

	/// <summary>
	/// Reads an unsigned 16-bit integer from the current stream.
	/// </summary>
	public override UInt16 ReadUInt16()
	{
		FillBuffer2(2);
		if (_endian == Endian.Little)
			return (UInt16)((_buffer[0] & 255) | (_buffer[1] << 8));
		else
			return (UInt16)((_buffer[0] << 8) | (_buffer[1] & 255));
	}

	/// <summary>
	/// Reads an unsigned 24-bit integer from the current stream.
	/// </summary>
	public UInt24 ReadUInt24()
	{
		FillBuffer2(3);
		if (_endian == Endian.Little)
			return (UInt24)(((_buffer[0] | (_buffer[1] << 8)) | (_buffer[2] << 16)));
		else
			return (UInt24)(((_buffer[2] | (_buffer[1] << 8)) | (_buffer[0] << 16)));
	}

	/// <summary>
	/// Reads an unsigned 32-bit integer from the current stream.
	/// </summary>
	public override UInt32 ReadUInt32()
	{
		FillBuffer2(4);
		if (_endian == Endian.Little)
			return (UInt32)(((_buffer[0] | (_buffer[1] << 8)) | (_buffer[2] << 16)) | (_buffer[3] << 24));
		else
			return (UInt32)(((_buffer[3] | (_buffer[2] << 8)) | (_buffer[1] << 16)) | (_buffer[0] << 24));
	}

	/// <summary>
	/// Reads an unsigned 64-bit integer from the current stream.
	/// </summary>
	public override UInt64 ReadUInt64()
	{
		UInt32 a, b;
		FillBuffer2(8);
		if (_endian == Endian.Little)
		{
			a = (UInt32)(((_buffer[0] | (_buffer[1] << 8)) | (_buffer[2] << 16)) | (_buffer[3] << 24));
			b = (UInt32)(((_buffer[4] | (_buffer[5] << 8)) | (_buffer[6] << 16)) | (_buffer[7] << 24));
		}
		else
		{
			a = (UInt32)(((_buffer[7] | (_buffer[6] << 8)) | (_buffer[5] << 16)) | (_buffer[4] << 24));
			b = (UInt32)(((_buffer[3] | (_buffer[2] << 8)) | (_buffer[1] << 16)) | (_buffer[0] << 24));
		}
		return (((UInt64)(b)) << 32) | ((UInt64)(a));
	}

	/// <summary>
	/// Reads an ASCII null-terminated string from the current stream.
	/// </summary>
	/// <returns>String read from the stream</returns>
	public string ReadNullTerminatedString()
	{
		byte lastByte;
		StringBuilder sb = new StringBuilder();

		do
		{
			int b = BaseStream.ReadByte();
			if(b == -1)
				throw new EndOfStreamException();
			lastByte = (byte)b;
			if (lastByte != 0x00)
				sb.Append((Char)lastByte);
		} while (lastByte != 0x00);

		return sb.ToString();
	}

	public Guid ReadGuid()
	{
		FillBuffer2(16);
		return new System.Guid(_buffer);
	}

	protected virtual void FillBuffer2(int numBytes) {
		if (_buffer != null && (numBytes < 0 || numBytes > _buffer.Length)) {
			throw new ArgumentOutOfRangeException("numBytes");
		}
		int bytesRead = 0;
		int n = 0;

		if (BaseStream == null) throw new ObjectDisposedException("stream");

		// Need to find a good threshold for calling ReadByte() repeatedly
		// vs. calling Read(byte[], int, int) for both buffered & unbuffered
		// streams.
		if (numBytes == 1) {
			n = BaseStream.ReadByte();
			if (n == -1) {
				throw new EndOfStreamException();
			}
			_buffer[0] = (byte)n;
			return;
		}

		do {
			n = BaseStream.Read(_buffer, bytesRead, numBytes - bytesRead);
			if (n == 0) {
				throw new EndOfStreamException();
			}
			bytesRead += n;
		} while (bytesRead < numBytes);
	}

}
