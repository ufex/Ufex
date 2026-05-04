using System;
using System.Collections.Generic;
using System.IO;

namespace Ufex.Hex;

/// <summary>
/// Represents the state of a search operation.
/// Holds the search pattern, cached match positions, and current index.
/// </summary>
public class HexSearchState
{
	/// <summary>
	/// The byte pattern that was searched for.
	/// </summary>
	public byte[] Pattern { get; }

	/// <summary>
	/// All match positions found in the stream.
	/// </summary>
	public List<long> Matches { get; }

	/// <summary>
	/// The index of the currently selected match, or -1 if none.
	/// </summary>
	public int CurrentIndex { get; set; }

	/// <summary>
	/// The total number of results found.
	/// </summary>
	public int TotalResults => Matches.Count;

	public HexSearchState(byte[] pattern)
	{
		Pattern = pattern;
		Matches = new List<long>();
		CurrentIndex = -1;
	}
}

/// <summary>
/// Byte pattern search over a Stream. Stateless: all state lives in HexSearchState.
/// </summary>
public static class HexSearch
{
	private const int DefaultSearchBufferSize = 65536;

	/// <summary>
	/// Searches the entire stream for the given byte pattern (exact match).
	/// The stream position is saved and restored.
	/// </summary>
	/// <param name="stream">The stream to search.</param>
	/// <param name="pattern">The byte pattern to find.</param>
	/// <param name="searchBufferSize">Size of the internal read buffer.</param>
	/// <returns>A HexSearchState containing all match positions.</returns>
	public static HexSearchState Find(Stream stream, byte[] pattern, int searchBufferSize = DefaultSearchBufferSize)
	{
		var state = new HexSearchState(pattern);
		if (pattern.Length == 0) return state;

		long fileSize = stream.Length;
		long savedPosition = stream.Position;

		try
		{
			stream.Seek(0, SeekOrigin.Begin);

			int bufSize = Math.Max(searchBufferSize, pattern.Length * 4);
			byte[] buffer = new byte[bufSize];
			long fileOffset = 0;
			int overlap = pattern.Length - 1;
			int carryOver = 0;

			while (fileOffset < fileSize)
			{
				int bytesToRead = bufSize - carryOver;
				int bytesRead = ReadFully(stream, buffer, carryOver, bytesToRead);

				int totalBytes = carryOver + bytesRead;
				if (totalBytes < pattern.Length) break;

				long searchStartOffset = fileOffset - carryOver;
				int searchLimit = totalBytes - pattern.Length + 1;

				for (int i = 0; i < searchLimit; i++)
				{
					bool match = true;
					for (int j = 0; j < pattern.Length; j++)
					{
						if (buffer[i + j] != pattern[j])
						{
							match = false;
							break;
						}
					}
					if (match)
					{
						state.Matches.Add(searchStartOffset + i);
					}
				}

				fileOffset += bytesRead;

				if (bytesRead > 0 && fileOffset < fileSize)
				{
					Array.Copy(buffer, totalBytes - overlap, buffer, 0, overlap);
					carryOver = overlap;
				}
				else
				{
					carryOver = 0;
				}
			}
		}
		finally
		{
			stream.Seek(savedPosition, SeekOrigin.Begin);
		}

		if (state.TotalResults > 0)
			state.CurrentIndex = 0;

		return state;
	}

	/// <summary>
	/// Searches the entire stream for the given byte pattern with ASCII case-insensitive matching.
	/// The stream position is saved and restored.
	/// </summary>
	/// <param name="stream">The stream to search.</param>
	/// <param name="pattern">The byte pattern to find (case-insensitive for ASCII letters).</param>
	/// <param name="searchBufferSize">Size of the internal read buffer.</param>
	/// <returns>A HexSearchState containing all match positions.</returns>
	public static HexSearchState FindCaseInsensitive(Stream stream, byte[] pattern, int searchBufferSize = DefaultSearchBufferSize)
	{
		var state = new HexSearchState(pattern);
		if (pattern.Length == 0) return state;

		long fileSize = stream.Length;
		long savedPosition = stream.Position;

		try
		{
			stream.Seek(0, SeekOrigin.Begin);

			// Pre-compute lowercase pattern
			byte[] lowerPattern = new byte[pattern.Length];
			for (int k = 0; k < pattern.Length; k++)
			{
				byte b = pattern[k];
				lowerPattern[k] = (b >= (byte)'A' && b <= (byte)'Z') ? (byte)(b + 32) : b;
			}

			int bufSize = Math.Max(searchBufferSize, pattern.Length * 4);
			byte[] buffer = new byte[bufSize];
			long fileOffset = 0;
			int overlap = pattern.Length - 1;
			int carryOver = 0;

			while (fileOffset < fileSize)
			{
				int bytesToRead = bufSize - carryOver;
				int bytesRead = ReadFully(stream, buffer, carryOver, bytesToRead);

				int totalBytes = carryOver + bytesRead;
				if (totalBytes < pattern.Length) break;

				long searchStartOffset = fileOffset - carryOver;
				int searchLimit = totalBytes - pattern.Length + 1;

				for (int i = 0; i < searchLimit; i++)
				{
					bool match = true;
					for (int j = 0; j < pattern.Length; j++)
					{
						byte b = buffer[i + j];
						if (b >= (byte)'A' && b <= (byte)'Z')
							b = (byte)(b + 32);
						if (b != lowerPattern[j])
						{
							match = false;
							break;
						}
					}
					if (match)
					{
						state.Matches.Add(searchStartOffset + i);
					}
				}

				fileOffset += bytesRead;

				if (bytesRead > 0 && fileOffset < fileSize)
				{
					Array.Copy(buffer, totalBytes - overlap, buffer, 0, overlap);
					carryOver = overlap;
				}
				else
				{
					carryOver = 0;
				}
			}
		}
		finally
		{
			stream.Seek(savedPosition, SeekOrigin.Begin);
		}

		if (state.TotalResults > 0)
			state.CurrentIndex = 0;

		return state;
	}

	/// <summary>
	/// Advances to the next match (wraps around).
	/// </summary>
	public static void MoveNext(HexSearchState state)
	{
		if (state.TotalResults == 0) return;
		state.CurrentIndex = (state.CurrentIndex + 1) % state.TotalResults;
	}

	/// <summary>
	/// Moves to the previous match (wraps around).
	/// </summary>
	public static void MovePrevious(HexSearchState state)
	{
		if (state.TotalResults == 0) return;
		state.CurrentIndex = (state.CurrentIndex - 1 + state.TotalResults) % state.TotalResults;
	}

	/// <summary>
	/// Gets the file position of the current match, or -1 if no matches.
	/// </summary>
	public static long CurrentMatchPosition(HexSearchState state)
	{
		if (state.CurrentIndex < 0 || state.CurrentIndex >= state.TotalResults)
			return -1;
		return state.Matches[state.CurrentIndex];
	}

	private static int ReadFully(Stream stream, byte[] buffer, int offset, int count)
	{
		int totalRead = 0;
		while (totalRead < count)
		{
			int read = stream.Read(buffer, offset + totalRead, count - totalRead);
			if (read == 0) break;
			totalRead += read;
		}
		return totalRead;
	}
}
