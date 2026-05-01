using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ufex.Hex;

namespace Ufex.Hex.Tests;

[TestClass]
public class HexBufferTests
{
	private static MemoryStream MakeStream(byte[] data)
	{
		return new MemoryStream(data, writable: false);
	}

	private static byte[] SequentialBytes(int count)
	{
		var data = new byte[count];
		for (int i = 0; i < count; i++)
			data[i] = (byte)(i & 0xFF);
		return data;
	}

	// ================================================================
	// Constructor / initial state
	// ================================================================

	[TestMethod]
	public void Constructor_SetsStreamLength()
	{
		var data = new byte[100];
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 64);

		Assert.AreEqual(100, buffer.StreamLength);
		Assert.AreEqual(64, buffer.BufferSize);
		Assert.AreEqual(0, buffer.ValidBytes);
	}

	// ================================================================
	// Load
	// ================================================================

	[TestMethod]
	public void Load_FromStart_FillsBuffer()
	{
		var data = SequentialBytes(256);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 64);

		buffer.Load(0, 1);

		Assert.AreEqual(0L, buffer.BufferStart);
		Assert.AreEqual(64, buffer.ValidBytes);
		Assert.AreEqual(0x00, buffer.Data[0]);
		Assert.AreEqual(0x3F, buffer.Data[63]);
	}

	[TestMethod]
	public void Load_CentersBufferAroundDisplayPosition()
	{
		var data = SequentialBytes(1024);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 256);

		// Load at position 512 — buffer should start ~1/4 buffer before
		buffer.Load(512, 1);

		// Buffer starts at position - bufferSize/4 = 512 - 64 = 448
		Assert.IsTrue(buffer.BufferStart <= 512);
		Assert.IsTrue(buffer.Contains(512, 16));
	}

	[TestMethod]
	public void Load_AlignsToRowBoundary()
	{
		var data = SequentialBytes(1024);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 256);

		buffer.Load(100, 16);

		// Buffer start should be aligned to multiple of 16
		Assert.AreEqual(0, buffer.BufferStart % 16);
	}

	[TestMethod]
	public void Load_SmallFile_ReadsEntireFile()
	{
		var data = SequentialBytes(32);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 256);

		buffer.Load(0, 1);

		Assert.AreEqual(32, buffer.ValidBytes);
	}

	// ================================================================
	// Contains
	// ================================================================

	[TestMethod]
	public void Contains_WithinBuffer_ReturnsTrue()
	{
		var data = SequentialBytes(256);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 128);

		buffer.Load(0, 1);

		Assert.IsTrue(buffer.Contains(0, 64));
		Assert.IsTrue(buffer.Contains(0, 128));
	}

	[TestMethod]
	public void Contains_OutsideBuffer_ReturnsFalse()
	{
		var data = SequentialBytes(256);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 64);

		buffer.Load(0, 1);

		Assert.IsFalse(buffer.Contains(64, 1));
		Assert.IsFalse(buffer.Contains(0, 128));
	}

	// ================================================================
	// ReadByte
	// ================================================================

	[TestMethod]
	public void ReadByte_ValidOffset_ReturnsCorrectByte()
	{
		var data = SequentialBytes(256);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 256);

		buffer.Load(0, 1);

		Assert.AreEqual(0x00, buffer.ReadByte(0));
		Assert.AreEqual(0x42, buffer.ReadByte(0x42));
		Assert.AreEqual(0xFF, buffer.ReadByte(0xFF));
	}

	[TestMethod]
	public void ReadByte_OutOfBuffer_ReturnsMinusOne()
	{
		var data = SequentialBytes(256);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 64);

		buffer.Load(0, 1);

		Assert.AreEqual(-1, buffer.ReadByte(64));
		Assert.AreEqual(-1, buffer.ReadByte(-1));
	}

	// ================================================================
	// GetBufferOffset
	// ================================================================

	[TestMethod]
	public void GetBufferOffset_ValidPosition_ReturnsOffset()
	{
		var data = SequentialBytes(512);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 256);

		buffer.Load(128, 1);

		long offset = buffer.GetBufferOffset(128);
		Assert.IsTrue(offset >= 0);
		Assert.AreEqual(data[128], buffer.Data[offset]);
	}

	[TestMethod]
	public void GetBufferOffset_OutsideBuffer_ReturnsMinusOne()
	{
		var data = SequentialBytes(512);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 64);

		buffer.Load(0, 1);

		Assert.AreEqual(-1, buffer.GetBufferOffset(500));
	}

	// ================================================================
	// Resize
	// ================================================================

	[TestMethod]
	public void Resize_ChangesBufferSize_InvalidatesData()
	{
		var data = SequentialBytes(256);
		using var stream = MakeStream(data);
		var buffer = new HexBuffer(stream, 64);

		buffer.Load(0, 1);
		Assert.AreEqual(64, buffer.ValidBytes);

		buffer.Resize(128);
		Assert.AreEqual(128, buffer.BufferSize);
		Assert.AreEqual(0, buffer.ValidBytes);
	}
}
