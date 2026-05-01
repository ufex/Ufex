using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ufex.Hex;

namespace Ufex.Hex.Tests;

[TestClass]
public class HexSearchTests
{
	private static MemoryStream MakeStream(byte[] data)
	{
		return new MemoryStream(data, writable: false);
	}

	// ================================================================
	// Find (exact match)
	// ================================================================

	[TestMethod]
	public void Find_PatternAtStart()
	{
		byte[] data = [ 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00 ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0x50, 0x4B ]);

		Assert.AreEqual(1, state.TotalResults);
		Assert.AreEqual(0L, state.Matches[0]);
	}

	[TestMethod]
	public void Find_PatternAtEnd()
	{
		byte[] data = [ 0x00, 0x00, 0x50, 0x4B ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0x50, 0x4B ]);

		Assert.AreEqual(1, state.TotalResults);
		Assert.AreEqual(2L, state.Matches[0]);
	}

	[TestMethod]
	public void Find_MultipleMatches()
	{
		byte[] data = [ 0xAA, 0xBB, 0x00, 0xAA, 0xBB, 0x00 ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0xAA, 0xBB ]);

		Assert.AreEqual(2, state.TotalResults);
		Assert.AreEqual(0L, state.Matches[0]);
		Assert.AreEqual(3L, state.Matches[1]);
	}

	[TestMethod]
	public void Find_NoMatch()
	{
		byte[] data = [ 0x00, 0x01, 0x02, 0x03 ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0xFF, 0xFE ]);

		Assert.AreEqual(0, state.TotalResults);
		Assert.AreEqual(-1, state.CurrentIndex);
	}

	[TestMethod]
	public void Find_EmptyPattern_ReturnsNoResults()
	{
		byte[] data = [ 0x00 ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, []);

		Assert.AreEqual(0, state.TotalResults);
	}

	[TestMethod]
	public void Find_PatternLargerThanData_ReturnsNoResults()
	{
		byte[] data = [ 0x00 ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0x00, 0x01 ]);

		Assert.AreEqual(0, state.TotalResults);
	}

	[TestMethod]
	public void Find_SingleBytePattern()
	{
		byte[] data = [ 0x01, 0x02, 0x01, 0x03, 0x01 ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0x01 ]);

		Assert.AreEqual(3, state.TotalResults);
		Assert.AreEqual(0L, state.Matches[0]);
		Assert.AreEqual(2L, state.Matches[1]);
		Assert.AreEqual(4L, state.Matches[2]);
	}

	[TestMethod]
	public void Find_CrossesBufferBoundary()
	{
		// Use a small search buffer to force the pattern to span a chunk boundary
		byte[] data = new byte[20];
		data[8] = 0xAA;
		data[9] = 0xBB;
		using var stream = MakeStream(data);

		// Buffer of 10 means pattern at offset 8-9 is near boundary
		var state = HexSearch.Find(stream, [ 0xAA, 0xBB ], searchBufferSize: 10);

		Assert.AreEqual(1, state.TotalResults);
		Assert.AreEqual(8L, state.Matches[0]);
	}

	[TestMethod]
	public void Find_PreservesStreamPosition()
	{
		byte[] data = [ 0x00, 0x01, 0x02, 0x03 ];
		using var stream = MakeStream(data);
		stream.Position = 2;

		HexSearch.Find(stream, [ 0x01 ]);

		Assert.AreEqual(2, stream.Position);
	}

	// ================================================================
	// FindCaseInsensitive
	// ================================================================

	[TestMethod]
	public void FindCaseInsensitive_MatchesDifferentCase()
	{
		// "Hello" in bytes
		byte[] data = [ 0x48, 0x65, 0x6C, 0x6C, 0x6F ];
		using var stream = MakeStream(data);

		// Search for "hello" (lowercase)
		var state = HexSearch.FindCaseInsensitive(stream, [ 0x68, 0x65, 0x6C, 0x6C, 0x6F ]);

		Assert.AreEqual(1, state.TotalResults);
		Assert.AreEqual(0L, state.Matches[0]);
	}

	[TestMethod]
	public void FindCaseInsensitive_NonAscii_ExactMatchOnly()
	{
		byte[] data = [ 0xC0, 0xC1, 0xC2 ];
		using var stream = MakeStream(data);

		// Non-ASCII bytes should match exactly (no case folding)
		var state = HexSearch.FindCaseInsensitive(stream, [ 0xC0, 0xC1 ]);
		Assert.AreEqual(1, state.TotalResults);

		var noMatch = HexSearch.FindCaseInsensitive(stream, [ 0xE0, 0xE1 ]);
		Assert.AreEqual(0, noMatch.TotalResults);
	}

	// ================================================================
	// MoveNext / MovePrevious
	// ================================================================

	[TestMethod]
	public void MoveNext_WrapsAround()
	{
		byte[] data = [ 0xAA, 0x00, 0xAA, 0x00, 0xAA ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0xAA ]);

		Assert.AreEqual(3, state.TotalResults);
		Assert.AreEqual(0, state.CurrentIndex);

		HexSearch.MoveNext(state);
		Assert.AreEqual(1, state.CurrentIndex);

		HexSearch.MoveNext(state);
		Assert.AreEqual(2, state.CurrentIndex);

		HexSearch.MoveNext(state);
		Assert.AreEqual(0, state.CurrentIndex); // wrapped
	}

	[TestMethod]
	public void MovePrevious_WrapsAround()
	{
		byte[] data = [ 0xAA, 0x00, 0xAA ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0xAA ]);

		Assert.AreEqual(0, state.CurrentIndex);

		HexSearch.MovePrevious(state);
		Assert.AreEqual(1, state.CurrentIndex); // wrapped to last

		HexSearch.MovePrevious(state);
		Assert.AreEqual(0, state.CurrentIndex);
	}

	[TestMethod]
	public void MoveNext_NoResults_DoesNothing()
	{
		var state = new HexSearchState([0xFF]);
		Assert.AreEqual(-1, state.CurrentIndex);

		HexSearch.MoveNext(state);
		Assert.AreEqual(-1, state.CurrentIndex);
	}

	// ================================================================
	// CurrentMatchPosition
	// ================================================================

	[TestMethod]
	public void CurrentMatchPosition_ReturnsCorrectPosition()
	{
		byte[] data = [ 0x00, 0xAA, 0x00, 0xAA ];
		using var stream = MakeStream(data);
		var state = HexSearch.Find(stream, [ 0xAA ]);

		Assert.AreEqual(1L, HexSearch.CurrentMatchPosition(state));

		HexSearch.MoveNext(state);
		Assert.AreEqual(3L, HexSearch.CurrentMatchPosition(state));
	}

	[TestMethod]
	public void CurrentMatchPosition_NoResults_ReturnsMinusOne()
	{
		var state = new HexSearchState([0xFF]);
		Assert.AreEqual(-1L, HexSearch.CurrentMatchPosition(state));
	}
}
