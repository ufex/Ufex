using System;
using System.Globalization;
using System.Text;

namespace Ufex.Hex;

/// <summary>
/// Shared hex/ASCII formatting utilities for byte data display.
/// </summary>
public static class HexFormat
{
	/// <summary>
	/// Pre-formatted uppercase hex lookup: HexUpper[0x4B] == "4B".
	/// </summary>
	public static readonly string[] HexUpper = new string[256];

	/// <summary>
	/// Pre-formatted lowercase hex lookup: HexLower[0x4B] == "4b".
	/// </summary>
	public static readonly string[] HexLower = new string[256];

	/// <summary>
	/// ASCII display lookup: printable chars map to themselves, others to ".".
	/// </summary>
	public static readonly string[] AsciiChars = new string[256];

	static HexFormat()
	{
		for (int i = 0; i < 256; i++)
		{
			HexUpper[i] = i.ToString("X2");
			HexLower[i] = i.ToString("x2");
			AsciiChars[i] = (i >= 32 && i < 127) ? ((char)i).ToString() : ".";
		}
	}

	/// <summary>
	/// Formats a range of bytes as space-separated uppercase hex (e.g. "50 4B 03 04").
	/// </summary>
	/// <param name="data">Source byte array.</param>
	/// <param name="offset">Start offset within data.</param>
	/// <param name="count">Number of bytes to format.</param>
	/// <returns>The formatted hex string.</returns>
	public static string FormatBytes(byte[] data, int offset, int count)
	{
		if (count <= 0) return string.Empty;

		var sb = new StringBuilder(count * 3 - 1);
		int end = Math.Min(offset + count, data.Length);
		for (int i = offset; i < end; i++)
		{
			if (i > offset) sb.Append(' ');
			sb.Append(HexUpper[data[i]]);
		}
		return sb.ToString();
	}

	/// <summary>
	/// Parses a hex string (e.g. "504B0304" or "50 4B 03 04") into a byte array.
	/// Accepts spaces, dashes, and colons as separators.
	/// Returns null if the input is invalid.
	/// </summary>
	public static byte[]? ParseHexString(string hex)
	{
		var cleaned = new StringBuilder();
		foreach (char c in hex)
		{
			if (IsHexChar(c))
			{
				cleaned.Append(c);
			}
			else if (c == ' ' || c == '-' || c == ':')
			{
				// Skip separators
			}
			else
			{
				return null;
			}
		}

		string hexStr = cleaned.ToString();

		if (hexStr.Length == 0 || hexStr.Length % 2 != 0)
			return null;

		byte[] bytes = new byte[hexStr.Length / 2];
		for (int i = 0; i < bytes.Length; i++)
		{
			if (!byte.TryParse(hexStr.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
			{
				return null;
			}
		}

		return bytes;
	}

	/// <summary>
	/// Returns true if the character is a valid hexadecimal digit.
	/// </summary>
	public static bool IsHexChar(char c)
	{
		return (c >= '0' && c <= '9') ||
		       (c >= 'a' && c <= 'f') ||
		       (c >= 'A' && c <= 'F');
	}
}
