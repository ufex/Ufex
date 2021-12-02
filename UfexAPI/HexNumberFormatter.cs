using System;
using System.Globalization;
using System.Text;

namespace Ufex.API
{
	public class HexNumberFormatter
	{
		private static string[] hexCharsL = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };
		private static string[] hexCharsU = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

		private bool caps = true;
		private bool lead0X = true;
		private bool leadZeros = true;
		private string prefix = "0x";
		private string toStringFormat = "X";
		private NumberFormatInfo numberFormatInfo;
		private Endian endian = Endian.Big;

		public bool Caps
		{
			get { return caps; }
			set 
			{ 
				caps = value;
				toStringFormat = caps ? "X" : "x";
			}
		}

		public bool LeadX
		{
			get { return lead0X; }
			set 
			{ 
				lead0X = value; 
				prefix = lead0X ? "0x" : ""; 
			}
		}

		public bool LeadZeros
		{
			get { return leadZeros; }
			set {  leadZeros = value; }
		}

		public NumberFormatInfo NFI
		{
			get { return numberFormatInfo; }
			set { numberFormatInfo = value; }
		}
		public Endian Endian
		{
			get { return endian; }
			set { endian = value; }
		}

		public HexNumberFormatter(bool caps = true, bool lead0X = true, bool leadZeros = true, Endian endian = Endian.Big)
		{
			this.Caps = caps;
			this.LeadX = lead0X;
			this.LeadZeros = leadZeros;
			this.Endian = endian;
		}

		public string UInt8(Byte x)
		{
			Byte n1 = (byte)(x / 16);
			Byte n2 = (byte)(x % 16);

			string[] chars = caps ? hexCharsU : hexCharsL;

			if(x < 16 && !leadZeros)
				return String.Concat(prefix, chars[n2]);
			else
				return String.Concat(prefix, chars[n1], chars[n2]);
		}

		public string UInt16(UInt16 x)
		{
			if(endian == Endian.Little)
            {
				x = DataManip.SwapEndian(x);
            }
			UInt16 t = x;
			Byte n1 = (byte)(t % 16);
			t /= 16;
			Byte n2 = (byte)(t % 16);
			t /= 16;
			Byte n3 = (byte)(t % 16);
			t /= 16;
			Byte n4 = (byte)(t % 16);
			if(caps)
			{
				if(!leadZeros)
				{
					if(x < 16)
						return String.Concat(prefix, hexCharsU[n1]); 
					else if(x < 256)
						return String.Concat(prefix, hexCharsU[n2], hexCharsU[n1]); 
					else if(x < 4096)
						return String.Concat(String.Concat(prefix, hexCharsU[n3]), String.Concat(hexCharsU[n2], hexCharsU[n1]));
					else
						return String.Concat(String.Concat(prefix, hexCharsU[n4]), String.Concat(hexCharsU[n3], hexCharsU[n2], hexCharsU[n1])); 
				}
				else
				{
					return String.Concat(String.Concat(prefix, hexCharsU[n4]), String.Concat(hexCharsU[n3], hexCharsU[n2], hexCharsU[n1]));
				}
			}
			else
			{
				if (!leadZeros)
				{
					if (x < 16)
						return String.Concat(prefix, hexCharsL[n1]);
					else if (x < 256)
						return String.Concat(prefix, hexCharsL[n2], hexCharsL[n1]);
					else if (x < 4096)
						return String.Concat(String.Concat(prefix, hexCharsL[n3]), String.Concat(hexCharsL[n2], hexCharsL[n1]));
					else
						return String.Concat(String.Concat(prefix, hexCharsL[n4]), String.Concat(hexCharsL[n3], hexCharsL[n2], hexCharsL[n1]));
				}
				else
				{
					return String.Concat(prefix, hexCharsL[n4]) + String.Concat(hexCharsL[n3], hexCharsL[n2], hexCharsL[n1]);
				}
			}
		}

		public String UInt32(UInt32 x)
		{
			if (endian == Endian.Little)
			{
				x = DataManip.SwapEndian(x);
			}
			string padding = "";
			if (leadZeros)
			{
				if (x < 0x10)
					padding = "0000000";
				else if (x < 0x100)
					padding = "000000";
				else if (x < 0x1000)
					padding = "00000";
				else if (x < 0x10000)
					padding = "0000";
				else if (x < 0x100000)
					padding = "000";
				else if (x < 0x1000000)
					padding = "00";
				else if (x < 0x10000000)
					padding = "0";
			}

			return prefix + padding + x.ToString(toStringFormat, numberFormatInfo);
		}

		public string UInt64(UInt64 x)
		{
			if (endian == Endian.Little)
			{
				x = DataManip.SwapEndian(x);
			}
			return prefix + x.ToString(toStringFormat + (leadZeros ? "16" : ""), numberFormatInfo);
		}

		public string SInt8(SByte x)
		{
			string padding = "";
			if (leadZeros && x >= 0 && x < 0x10)
			{
				padding = "0";
			}
			return prefix + padding + x.ToString(toStringFormat, numberFormatInfo);
		}

		public string SInt16(Int16 x)
		{
			if (endian == Endian.Little)
			{
				x = DataManip.SwapEndian(x);
			}
			string padding = "";
			if (leadZeros && x >= 0)
			{
				if (x < 0x000F)
					padding = "000";
				else if (x < 0x00FF)
					padding = "00";
				else if (x < 0x0FFF)
					padding = "0";
			}

			return prefix + padding + x.ToString(toStringFormat, numberFormatInfo);
		}

		public string SInt32(Int32 x)
		{
			if (endian == Endian.Little)
			{
				x = DataManip.SwapEndian(x);
			}
			string padding = "";
			if (leadZeros && x >= 0)
			{
				if (x < 0xF)
					padding = "0000000";
				else if (x < 0xFF)
					padding = "000000";
				else if (x < 0xFFF)
					padding = "00000";
				else if (x < 0xFFFF)
					padding = "0000";
				else if (x < 0xFFFFF)
					padding = "000";
				else if (x < 0xFFFFFF)
					padding = "00";
				else if (x < 0xFFFFFFF)
					padding = "0";
			}

			return prefix + padding + x.ToString(toStringFormat, numberFormatInfo);
		}

		public string SInt64(Int64 x)
		{
			if (endian == Endian.Little)
			{
				x = DataManip.SwapEndian(x);
			}
			return prefix + x.ToString(toStringFormat + (leadZeros ? "16" : ""), numberFormatInfo);
		}

	}
}
