using System;
namespace Ufex.API.Format;

/// <summary>
/// Utility class for formatting byte counts into human-readable strings.
/// </summary>
public static class ByteCountFormatter
{
	// Define the units to use
	static readonly string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB" };
	
	public static string Format(uint numBytes)
	{
		return Format((ulong)numBytes);
	}

	public static string Format(ulong numBytes)
	{
		int counter = 0;
		decimal number = numBytes;
		while (Math.Round(number / 1024) >= 1)
		{
			number = number / 1024;
			counter++;
		}
		string format = counter == 0 ? "{0} {1}" : "{0:n1} {1}";
		return string.Format(format, number, suffixes[counter]);
	}
}