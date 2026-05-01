using System;
using System.IO;
using System.Threading.Tasks;

namespace Ufex.Hex;

/// <summary>
/// Manages a sliding window buffer over a read-only Stream.
/// Handles aligned reads and position tracking for efficient scrolling.
/// </summary>
public class HexBuffer
{
	private readonly Stream _stream;
	private readonly long _streamLength;
	private byte[] _buffer;
	private long _bufferStart;
	private int _validBytes;

	/// <summary>
	/// Gets the size of the buffer in bytes.
	/// </summary>
	public int BufferSize => _buffer.Length;

	/// <summary>
	/// Gets the file offset corresponding to index 0 in the buffer.
	/// </summary>
	public long BufferStart => _bufferStart;

	/// <summary>
	/// Gets the number of valid bytes currently in the buffer.
	/// </summary>
	public int ValidBytes => _validBytes;

	/// <summary>
	/// Gets the total length of the underlying stream.
	/// </summary>
	public long StreamLength => _streamLength;

	/// <summary>
	/// Gets the underlying buffer array (read-only access intended).
	/// </summary>
	public byte[] Data => _buffer;

	/// <summary>
	/// Creates a new HexBuffer over the given stream.
	/// </summary>
	/// <param name="stream">A seekable, readable stream.</param>
	/// <param name="bufferSize">Buffer size in bytes (default 64KB).</param>
	public HexBuffer(Stream stream, int bufferSize = 65536)
	{
		_stream = stream;
		_streamLength = stream.Length;
		_buffer = new byte[bufferSize];
		_bufferStart = 0;
		_validBytes = 0;
	}

	/// <summary>
	/// Returns true if the range [position, position + count) is entirely within the current buffer.
	/// </summary>
	public bool Contains(long position, int count)
	{
		return position >= _bufferStart &&
		       position + count <= _bufferStart + _validBytes;
	}

	/// <summary>
	/// Loads data centered around the given display position.
	/// Positions 1/4 of the buffer before the display position to allow backward scrolling.
	/// </summary>
	/// <param name="displayPosition">The file offset of the first displayed byte.</param>
	/// <param name="rowAlignment">Align the buffer start to a multiple of this value (typically columns per row).</param>
	public void Load(long displayPosition, int rowAlignment = 1)
	{
		long bufferStart = Math.Max(0, displayPosition - _buffer.Length / 4);

		if (rowAlignment > 1)
			bufferStart = (bufferStart / rowAlignment) * rowAlignment;

		_bufferStart = bufferStart;

		_stream.Seek(_bufferStart, SeekOrigin.Begin);
		int bytesToRead = (int)Math.Min(_buffer.Length, _streamLength - _bufferStart);
		_validBytes = ReadFully(_stream, _buffer, bytesToRead);

		if (_validBytes < _buffer.Length)
			Array.Clear(_buffer, _validBytes, _buffer.Length - _validBytes);
	}

	/// <summary>
	/// Loads data into a temporary buffer on a background thread, then swaps it in.
	/// Returns a Task that completes when the buffer is ready.
	/// </summary>
	/// <param name="displayPosition">The file offset of the first displayed byte.</param>
	/// <param name="rowAlignment">Align the buffer start to a multiple of this value.</param>
	public async Task LoadAsync(long displayPosition, int rowAlignment = 1)
	{
		long bufferStart = Math.Max(0, displayPosition - _buffer.Length / 4);

		if (rowAlignment > 1)
			bufferStart = (bufferStart / rowAlignment) * rowAlignment;

		long bufStart = bufferStart;
		int bytesToRead = (int)Math.Min(_buffer.Length, _streamLength - bufStart);
		byte[] tempBuffer = new byte[_buffer.Length];

		int bytesRead = await Task.Run(() =>
		{
			lock (_stream)
			{
				_stream.Seek(bufStart, SeekOrigin.Begin);
				return ReadFully(_stream, tempBuffer, bytesToRead);
			}
		});

		_bufferStart = bufStart;
		_validBytes = bytesRead;
		Array.Copy(tempBuffer, _buffer, _buffer.Length);
	}

	/// <summary>
	/// Reads a byte at the given file offset, or returns -1 if out of buffer.
	/// </summary>
	public int ReadByte(long fileOffset)
	{
		long idx = fileOffset - _bufferStart;
		if (idx < 0 || idx >= _validBytes)
			return -1;
		return _buffer[idx];
	}

	/// <summary>
	/// Gets the buffer offset for a given file position, or -1 if not in buffer.
	/// </summary>
	public long GetBufferOffset(long filePosition)
	{
		long offset = filePosition - _bufferStart;
		if (offset < 0 || offset >= _validBytes)
			return -1;
		return offset;
	}

	/// <summary>
	/// Reads bytes from the stream into the buffer, handling partial reads.
	/// </summary>
	private static int ReadFully(Stream stream, byte[] buffer, int count)
	{
		int totalRead = 0;
		while (totalRead < count)
		{
			int read = stream.Read(buffer, totalRead, count - totalRead);
			if (read == 0) break;
			totalRead += read;
		}
		return totalRead;
	}

	/// <summary>
	/// Resizes the buffer. Invalidates all currently buffered data.
	/// </summary>
	public void Resize(int newSize)
	{
		_buffer = new byte[newSize];
		_bufferStart = 0;
		_validBytes = 0;
	}
}
