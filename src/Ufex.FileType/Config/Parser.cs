using System;
using System.Globalization;

namespace Ufex.FileType.Config
{
	class Parser
	{
		// TODO: Convert.FromHexString
		public static byte[] ByteArray(string input)
		{
			byte[] output;
			if (input.StartsWith("0x"))
			{
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
			return byte.Parse(value.Replace("0x", ""), value.StartsWith("0x") ? NumberStyles.HexNumber : NumberStyles.Integer);
		}

		public static UInt32 UInt32(string value)
		{
			return System.UInt32.Parse(value.Replace("0x", ""), value.StartsWith("0x") ? NumberStyles.HexNumber : NumberStyles.Integer);
		}

		public static UInt64 UInt64(string value)
		{
			return System.UInt64.Parse(value.Replace("0x", ""), value.StartsWith("0x") ? NumberStyles.HexNumber : NumberStyles.Integer);
		}
	}
}
