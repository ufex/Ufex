using System;
using System.Globalization;

namespace Ufex.FileType.Config
{
	class Parser
	{
		private static string NormalizeNumber(string value, out NumberStyles style)
		{
			if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				style = NumberStyles.HexNumber;
				return value.Substring(2);
			}

			style = NumberStyles.Integer;
			return value;
		}

		// TODO: Convert.FromHexString
		public static byte[] ByteArray(string input)
		{
			byte[] output;
			if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				if (input.StartsWith("0X", StringComparison.Ordinal))
				{
					input = "0x" + input.Substring(2);
				}
				input = input.Replace("0x", "").Replace(" ", "");
				int outputLength = input.Length / 2;
				output = new byte[outputLength];
				var numeral = new char[2];
				for (int i = 0; i < outputLength; i++)
				{
					input.CopyTo(i * 2, numeral, 0, 2);
					output[i] = Convert.ToByte(new string(numeral), 16);
				}
			}
			else
			{
				string[] nums = input.Split(' ');
				output = new byte[nums.Length];
				for (var i = 0; i < nums.Length; i++)
				{
					output[i] = byte.Parse(nums[i]);
				}
			}
			return output;
		}

		public static Byte Byte(string value)
		{
			string normalized = NormalizeNumber(value, out NumberStyles style);
			return byte.Parse(normalized, style);
		}

		public static Int16 Int16(string value)
		{
			string normalized = NormalizeNumber(value, out NumberStyles style);
			return System.Int16.Parse(normalized, style);
		}

		public static UInt16 UInt16(string value)
		{
			string normalized = NormalizeNumber(value, out NumberStyles style);
			return System.UInt16.Parse(normalized, style);
		}

		public static Int32 Int32(string value)
		{
			string normalized = NormalizeNumber(value, out NumberStyles style);
			return System.Int32.Parse(normalized, style);
		}

		public static UInt32 UInt32(string value)
		{
			string normalized = NormalizeNumber(value, out NumberStyles style);
			return System.UInt32.Parse(normalized, style);
		}

		public static Int64 Int64(string value)
		{
			string normalized = NormalizeNumber(value, out NumberStyles style);
			return System.Int64.Parse(normalized, style);
		}

		public static UInt64 UInt64(string value)
		{
			string normalized = NormalizeNumber(value, out NumberStyles style);
			return System.UInt64.Parse(normalized, style);
		}
	}
}
