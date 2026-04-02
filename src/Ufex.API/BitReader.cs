using System;
using System.IO;

namespace Ufex.API;

/// <summary>
/// Reads bit-aligned values from a stream.
/// The bit order determines both how bits are consumed within each byte
/// and how the returned value is packed.
/// </summary>
public sealed class BitReader
{
	private readonly Stream _stream;
	private readonly BitOrder _bitOrder;
	private Int32 _bitBuffer;
	private Int32 _bitCount;

	public BitReader(Stream stream, BitOrder bitOrder = BitOrder.LeastSignificantFirst)
	{
		ArgumentNullException.ThrowIfNull(stream);

		_stream = stream;
		_bitOrder = bitOrder;
	}

	/// <summary>
	/// Gets the stream that this reader consumes bits from.
	/// </summary>
	public Stream BaseStream => _stream;

	/// <summary>
	/// Gets the bit ordering used by this reader.
	/// </summary>
	public BitOrder BitOrder => _bitOrder;

	/// <summary>
	/// Gets whether the reader is currently aligned to the next byte boundary.
	/// </summary>
	public Boolean IsByteAligned => _bitCount == 0;

	/// <summary>
	/// Reads a single bit from the stream.
	/// </summary>
	/// <exception cref="EndOfStreamException">Thrown when the end of the stream is reached.</exception>
	public Boolean ReadBit()
	{
		return ReadNextBit() != 0;
	}

	/// <summary>
	/// Reads up to 32 bits from the stream and packs them into an unsigned integer.
	/// For <see cref="Ufex.API.BitOrder.LeastSignificantFirst"/>, the first bit read
	/// becomes bit 0 of the return value. For
	/// <see cref="Ufex.API.BitOrder.MostSignificantFirst"/>, the first bit read
	/// becomes the highest requested bit.
	/// </summary>
	/// <param name="count">The number of bits to read. Must be between 0 and 32.</param>
	/// <returns>The packed value that was read.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is outside the valid range.</exception>
	/// <exception cref="EndOfStreamException">Thrown when the end of the stream is reached.</exception>
	public UInt32 ReadBits(Int32 count)
	{
		ValidateBitCount(count, 32);
		return (UInt32)ReadBitsInternal(count);
	}

	/// <summary>
	/// Reads up to 64 bits from the stream and packs them into an unsigned integer.
	/// </summary>
	/// <param name="count">The number of bits to read. Must be between 0 and 64.</param>
	/// <returns>The packed value that was read.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is outside the valid range.</exception>
	/// <exception cref="EndOfStreamException">Thrown when the end of the stream is reached.</exception>
	public UInt64 ReadBits64(Int32 count)
	{
		ValidateBitCount(count, 64);
		return ReadBitsInternal(count);
	}

	/// <summary>
	/// Attempts to read a single bit without consuming input when the stream ends.
	/// </summary>
	public Boolean TryReadBit(out Boolean value)
	{
		if (!TryReadBitsInternal(1, out UInt64 result))
		{
			value = false;
			return false;
		}

		value = result != 0;
		return true;
	}

	/// <summary>
	/// Attempts to read up to 32 bits without consuming input when the stream ends.
	/// This requires a seekable stream so the reader can restore its previous state.
	/// </summary>
	public Boolean TryReadBits(Int32 count, out UInt32 value)
	{
		ValidateBitCount(count, 32);

		if (!TryReadBitsInternal(count, out UInt64 result))
		{
			value = 0;
			return false;
		}

		value = (UInt32)result;
		return true;
	}

	/// <summary>
	/// Attempts to read up to 64 bits without consuming input when the stream ends.
	/// This requires a seekable stream so the reader can restore its previous state.
	/// </summary>
	public Boolean TryReadBits64(Int32 count, out UInt64 value)
	{
		ValidateBitCount(count, 64);
		return TryReadBitsInternal(count, out value);
	}

	/// <summary>
	/// Skips the specified number of bits.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
	/// <exception cref="EndOfStreamException">Thrown when the end of the stream is reached.</exception>
	public void SkipBits(Int32 count)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(count);

		for (Int32 i = 0; i < count; i++)
			ReadNextBit();
	}

	/// <summary>
	/// Aligns the reader to the next byte boundary.
	/// </summary>
	public void AlignToByteBoundary()
	{
		_bitCount = 0;
		_bitBuffer = 0;
	}

	/// <summary>
	/// Reads the next 8 bits from the bit stream.
	/// When the reader is byte-aligned, this reads the next byte from the underlying stream.
	/// </summary>
	public Byte ReadByte()
	{
		return (Byte)ReadBits(8);
	}

	/// <summary>
	/// Reads the specified number of bytes from the bit stream.
	/// </summary>
	public Byte[] ReadBytes(Int32 count)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(count);

		Byte[] bytes = new Byte[count];
		for (Int32 i = 0; i < count; i++)
			bytes[i] = ReadByte();
		return bytes;
	}

	private UInt64 ReadBitsInternal(Int32 count)
	{
		UInt64 result = 0;

		if (_bitOrder == BitOrder.LeastSignificantFirst)
		{
			for (Int32 i = 0; i < count; i++)
				result |= ((UInt64)ReadNextBit()) << i;
		}
		else
		{
			for (Int32 i = 0; i < count; i++)
				result = (result << 1) | (UInt64)ReadNextBit();
		}

		return result;
	}

	private Boolean TryReadBitsInternal(Int32 count, out UInt64 value)
	{
		if (!BaseStream.CanSeek)
			throw new NotSupportedException("TryReadBits requires a seekable stream.");

		Int64 originalPosition = BaseStream.Position;
		Int32 originalBitBuffer = _bitBuffer;
		Int32 originalBitCount = _bitCount;

		try
		{
			value = ReadBitsInternal(count);
			return true;
		}
		catch (EndOfStreamException)
		{
			BaseStream.Position = originalPosition;
			_bitBuffer = originalBitBuffer;
			_bitCount = originalBitCount;
			value = 0;
			return false;
		}
	}

	private Byte ReadNextBit()
	{
		EnsureBitAvailable();

		if (_bitOrder == BitOrder.LeastSignificantFirst)
		{
			Byte bit = (Byte)(_bitBuffer & 1);
			_bitBuffer >>= 1;
			_bitCount--;
			return bit;
		}

		Int32 msbIndex = _bitCount - 1;
		Byte result = (Byte)((_bitBuffer >> msbIndex) & 1);
		_bitCount--;
		if (_bitCount == 0)
			_bitBuffer = 0;
		return result;
	}

	private void EnsureBitAvailable()
	{
		if (_bitCount != 0)
			return;

		Int32 b = BaseStream.ReadByte();
		if (b == -1)
			throw new EndOfStreamException();

		_bitBuffer = b;
		_bitCount = 8;
	}

	private static void ValidateBitCount(Int32 count, Int32 maxCount)
	{
		if (count < 0 || count > maxCount)
			throw new ArgumentOutOfRangeException(nameof(count), $"count must be between 0 and {maxCount}.");
	}
}
