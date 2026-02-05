using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Ufex.API;

namespace Ufex.FileType.Config
{

	public enum OffsetType
	{
		[XmlEnum("absolute")]
		Absolute,
		[XmlEnum("readUInt16Le")]
		ReadUInt16Le,
		[XmlEnum("readUInt32Le")]
		ReadUInt32Le,
		[XmlEnum("readUInt64Le")]
		ReadUInt64Le,
		[XmlEnum("readUInt16Be")]
		ReadUInt16Be,
		[XmlEnum("readUInt32Be")]
		ReadUInt32Be,
		[XmlEnum("readUInt64Be")]
		ReadUInt64Be
	}

	public enum PatternOperator
	{
		[XmlEnum("eq")]
		Equal,
		[XmlEnum("neq")]
		NotEqual,
		[XmlEnum("gt")]
		GreaterThan,
		[XmlEnum("gte")]
		GreaterThanOrEqual,
		[XmlEnum("lt")]
		LessThan,
		[XmlEnum("lte")]
		LessThanOrEqual,
	}

	[XmlType("Pattern"), XmlInclude(typeof(BytePattern)), XmlInclude(typeof(BytesPattern)), XmlInclude(typeof(StringPattern))]
	public abstract class SignaturePattern
	{
		[XmlIgnore]
		public UInt64 Offset = 0;
		[XmlAttribute("offsetType")]
		public OffsetType OffsetType = OffsetType.Absolute;
		[XmlIgnore]
		public UInt32 Range = 1;

		[XmlAttribute("offset")]
		public string _Offset
		{
			get { return "0x" + Offset.ToString("x8"); }
			set { Offset = Parser.UInt64(value); }
		}

		[XmlAttribute("range")]
		public string _Range
		{
			get { return "0x" + Range.ToString("x8"); }
			set { Range = Parser.UInt32(value); }
		}

		public abstract bool Matches(byte[] buffer, FileStream fileStream);

		protected UInt64 ReadOffset(byte[] buffer, FileStream fileStream)
		{
			if (OffsetType == OffsetType.ReadUInt16Le || OffsetType == OffsetType.ReadUInt16Be)
			{
				Endian endian = OffsetType == OffsetType.ReadUInt16Be ? Endian.Big : Endian.Little;
				if (Offset + 2 > (ulong)buffer.Length)
				{
					byte[] tmp = new byte[2];
					fileStream.ReadExactly(tmp, (int)Offset, 2); // TODO: read deeper into the file than 32-bit signed integer supports?
					return ByteUtil.BytesToUInt16(tmp, endian, 0);
				}
				else
				{
					return ByteUtil.BytesToUInt16(buffer, endian, (int)Offset);
				}
			}
			else if (OffsetType == OffsetType.ReadUInt32Le || OffsetType == OffsetType.ReadUInt32Be)
			{
				Endian endian = OffsetType == OffsetType.ReadUInt32Be ? Endian.Big : Endian.Little;
				if (Offset + 4 > (ulong)buffer.Length)
				{
					byte[] tmp = new byte[4];
					fileStream.ReadExactly(tmp, (int)Offset, 4); // TODO: read deeper into the file than 32-bit signed integer supports?
					return ByteUtil.BytesToUInt32(tmp, endian, 0);
				}
				else
				{
					return ByteUtil.BytesToUInt32(buffer, endian, (int)Offset);
				}

			}
			throw new Exception("not implemented");
		}

		private int ReadUntil(FileStream fs, byte b, int startIndex, int count)
		{
			int i = 0;
			fs.Seek(startIndex, SeekOrigin.Begin);
			while(fs.ReadByte() != b)
			{
				i++;
				if(i > count)
				{
					return -1;
				}
			}
			return startIndex + i;
		}

		protected bool BytesMatch(byte[] valueBytes, byte[] buffer, FileStream fileStream)
		{
			UInt64 bytesNeeded;
			UInt64 offset = this.Offset;
			if (OffsetType != OffsetType.Absolute)
			{
				offset = ReadOffset(buffer, fileStream);
			}
			bytesNeeded = offset + this.Range + (ulong)valueBytes.Length;
			if (bytesNeeded > (UInt64)fileStream.Length)
			{
				// TODO: should search what's available if limited by the range
				// Not enough bytes in the file to perform matching
				return false;
			}

			int valueLength = valueBytes.Length;
			if (bytesNeeded > (UInt64)buffer.Length)
			{
				// Process from file
				int seek = 0, startIndex;
				while ((startIndex = ReadUntil(fileStream, valueBytes[0], seek + (int)offset, (int)Range)) != -1)
				{
					byte[] buffer2 = new byte[valueLength];
					fileStream.Seek(startIndex, SeekOrigin.Begin);
					int bytesRead = fileStream.Read(buffer2, 0, valueLength);
					if(bytesRead < valueLength)
					{
						return false;
					}
					bool match = true;
					for (int i = 0; i < valueLength; i++)
					{
						if (buffer2[i] != valueBytes[i])
						{
							match = false;
							break;
						}
					}
					if (match)
					{
						return true;
					}
					seek += 1;
				}
				return false;
			}
			else
			{
				// Process with buffer
				int seek = 0, startIndex;
				while ((startIndex = Array.IndexOf(buffer, valueBytes[0], seek + (int)offset, (int)Range)) != -1)
				{
					bool match = true;
					for (int i = 0; i < valueLength; i++)
					{
						if (buffer[i] != valueBytes[i - startIndex])
						{
							match = false;
							break;
						}
					}
					if (match)
					{
						return true;
					}
					seek += 1;
				}
				return false;
			}
		}
	}


	[XmlType("byte")]
	public class BytePattern : SignaturePattern
	{
		[XmlIgnore]
		public byte Value;

		[XmlAttribute("operator")]
		public PatternOperator Operator = PatternOperator.Equal;

		[XmlText]
		public string _Value
		{
			get { return "0x" + Value.ToString("x2"); }
			set { Value = Parser.Byte(value); }
		}

		public BytePattern()
		{
		}

		public BytePattern(byte value, PatternOperator op = PatternOperator.Equal, UInt64 offset = 0, OffsetType offsetType = OffsetType.Absolute, UInt32 range = 0)
		{
			this.Value = value;
			this.Operator = op;
			this.Offset = offset;
			this.OffsetType = offsetType;
			this.Range = range;
		}

		public override bool Matches(byte[] buffer, FileStream fileStream)
		{
			UInt64 offset = this.Offset;
			if (OffsetType != OffsetType.Absolute)
			{
				offset = ReadOffset(buffer, fileStream);
			}
			UInt64 minBytesNeeded = offset + 1;
			if (minBytesNeeded > (UInt64)fileStream.Length)
			{
				// Not enough bytes in the file to perform matching
				return false;
			}

			if (minBytesNeeded + Range > (UInt64)buffer.Length)
			{
				// Process from file
			}
			else
			{
				// Process from buffer
				for(var i = offset; i < offset + Range; i++)
				{
					bool match = false;
					switch(Operator)
					{
						case PatternOperator.Equal:
							match = buffer[i] == Value;
							break;
						case PatternOperator.NotEqual:
							match = buffer[i] != Value;
							break;
						case PatternOperator.LessThan:
							match = buffer[i] < Value;
							break;
						case PatternOperator.LessThanOrEqual:
							match = buffer[i] <= Value;
							break;
						case PatternOperator.GreaterThan:
							match = buffer[i] > Value;
							break;
						case PatternOperator.GreaterThanOrEqual:
							match = buffer[i] >= Value;
							break;
					}
					if (match)
					{
						return true;
					}
				}
				return false;
			}

			throw new NotImplementedException();
		}
	}

	[XmlType("bytes")]
	public class BytesPattern : SignaturePattern
	{
		[XmlIgnore]
		public byte[] Value;

		[XmlText]
		public string _Value
		{
			get { return string.Join(" ", Value.Select(x => x.ToString("x2"))); }
			set { Value = Parser.ByteArray(value); }
		}

		public BytesPattern()
		{
		}

		public BytesPattern(byte[] value, UInt64 offset = 0, OffsetType offsetType = OffsetType.Absolute, UInt32 range = 0)
		{
			Value = value;
			Offset = offset;
			OffsetType = offsetType;
			Range = range;
		}

		public override bool Matches(byte[] buffer, FileStream fileStream)
		{
			return BytesMatch(Value, buffer, fileStream);
		}
	}

	[XmlType("string")]
	public class StringPattern : SignaturePattern
	{
		[XmlText]
		public string Value;

		public StringPattern()
		{
		}

		public StringPattern(string value = null, UInt64 offset = 0, OffsetType offsetType = OffsetType.Absolute, UInt32 range = 0)
		{
			this.Value = value;
			this.Offset = offset;
			this.OffsetType = offsetType;
			this.Range = range;
		}

		public override bool Matches(byte[] buffer, FileStream fileStream)
		{
			return BytesMatch(Encoding.ASCII.GetBytes(Value), buffer, fileStream);
		}
	}
}
