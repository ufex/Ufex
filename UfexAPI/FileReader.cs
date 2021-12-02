using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ufex.API
{
	public class FileReader
	{
		public FileReader(System.IO.Stream input) 
		{ 
			Init(input, true, new UTF8Encoding()); 
		}

		public FileReader(System.IO.Stream input, bool littleEndian) 
		{ 
			Init(input, littleEndian, new UTF8Encoding()); 
		}
		
		public FileReader(System.IO.Stream input, bool littleEndian, System.Text.Encoding encoding) 
		{ 
			Init(input, littleEndian, encoding); 
		}

		~FileReader()
		{
			this.Close();
			m_buffer = null;
			m_decoder = null;
			m_charBytes = null;
			m_singleChar = null;
			m_charBuffer = null;
			m_disposed = true;
		}

		public void Close()
		{
			System.IO.Stream tmpStream = m_stream;
			m_stream = null;
			if (tmpStream != null)
			{
				tmpStream.Close();
			}
		}

		public Int32 Read()
		{
			if (m_stream == null)
				FileNotOpen();

			return InternalReadOneChar();
		}

		public bool ReadBoolean()
		{
			FillBuffer(1);
			return !(m_buffer[0] == 0);
		}

		public Byte[] ReadBytes(Int32 count)
		{
			Byte[] arr;

			arr = new Byte[count];
			m_stream.Read(arr, 0, count);

			return arr;
		}
		public Byte[] ReadBytes(UInt16 count) 
		{ 
			return ReadBytes((int)count); 
		}
		public Char ReadChar()
		{
			Int32 i;
			i = this.Read();
			if (i == -1)
			{
				EndOfFile();
			}
			return (Char)i;
		}
		public Char[] ReadChars(Int32 count)
		{
			Char[] arr;
			Int32 i;
			Char[] arr1;

			if (m_stream == null)
			{
				FileNotOpen();
			}

			if (count < 0)
			{
				throw new System.ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
			}

			arr = new Char[count];
			i = InternalReadChars(arr, 0, count);

			if (i != count)
			{
				arr1 = new Char[i];
				Buffer.BlockCopy(arr, 0, arr1, 0, (2 * i));
				arr = arr1;
			}
			return arr;
		}

		// Read the signed integer types
		public Int16 ReadInt16()
		{
			FillBuffer(2);
			if (m_littleEndian)
				return (Int16)((m_buffer[0] & 255) | (m_buffer[1] << 8));
			else
				return (Int16)((m_buffer[1] & 255) | (m_buffer[0] << 8));
		}
		public Int32 ReadInt32()
		{
			FillBuffer(4);
			if (m_littleEndian)
				return (((m_buffer[0] & 255) | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24);
			else
				return (((m_buffer[3] & 255) | (m_buffer[2] << 8)) | (m_buffer[1] << 16)) | (m_buffer[0] << 24);
		}
		public Int64 ReadInt64()
		{
			UInt32 a, b;

			// Read 8 bytes
			FillBuffer(8);
			if (m_littleEndian)
			{
				a = (UInt32)(((m_buffer[0] | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24));
				b = (UInt32)(((m_buffer[4] | (m_buffer[5] << 8)) | (m_buffer[6] << 16)) | (m_buffer[7] << 24));
			}
			else
			{
				a = (UInt32)(((m_buffer[7] | (m_buffer[6] << 8)) | (m_buffer[5] << 16)) | (m_buffer[4] << 24));
				b = (UInt32)(((m_buffer[3] | (m_buffer[2] << 8)) | (m_buffer[1] << 16)) | (m_buffer[0] << 24));
			}
			return (Int64)((((UInt64)(b)) << 32) | ((UInt64)(a)));
		}
		public SByte ReadSByte()
		{
			FillBuffer(1);
			return (SByte)m_buffer[0];
		}

		// Read the unsinged integer types
		public Byte ReadByte()
		{
			FillBuffer(1);
			return m_buffer[0];
		}

		public UInt16 ReadUInt16()
		{
			FillBuffer(2);
			if (m_littleEndian)
				return (UInt16)((m_buffer[0] & 255) | (m_buffer[1] << 8));
			else
				return (UInt16)((m_buffer[0] << 8) | (m_buffer[1] & 255));
		}
		public UInt32 ReadUInt32()
		{
			FillBuffer(4);
			if (m_littleEndian)
				return (UInt32)(((m_buffer[0] | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24));
			else
				return (UInt32)(((m_buffer[3] | (m_buffer[2] << 8)) | (m_buffer[1] << 16)) | (m_buffer[0] << 24));
		}
		public UInt64 ReadUInt64()
		{
			UInt32 a, b;
			FillBuffer(8);
			if (m_littleEndian)
			{
				a = (UInt32)(((m_buffer[0] | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24));
				b = (UInt32)(((m_buffer[4] | (m_buffer[5] << 8)) | (m_buffer[6] << 16)) | (m_buffer[7] << 24));
			}
			else
			{
				a = (UInt32)(((m_buffer[7] | (m_buffer[6] << 8)) | (m_buffer[5] << 16)) | (m_buffer[4] << 24));
				b = (UInt32)(((m_buffer[3] | (m_buffer[2] << 8)) | (m_buffer[1] << 16)) | (m_buffer[0] << 24));
			}
			return (((UInt64)(b)) << 32) | ((UInt64)(a));
		}

		// Reads a null terminated string
		public String ReadNullTermString()
		{
			Byte lastByte;
			StringBuilder sb = new StringBuilder();

			do
			{
				lastByte = (Byte)m_stream.ReadByte();

				if (lastByte != 0x00)
					sb.Append((Char)lastByte);

			} while (lastByte != 0x00);

			return sb.ToString();
		}

		public Guid ReadGuid()
		{
			FillBuffer(16);
			return new System.Guid(m_buffer);
		}

		public Stream BaseStream 
		{
			get { return m_stream; }
			set { m_stream = value; }
		}

		protected void Init(System.IO.Stream input, bool littleEndian, System.Text.Encoding encoding)
		{
			m_stream = input;

			m_decoder = encoding.GetDecoder();

			m_littleEndian = littleEndian;

			m_charBuffer = null;
			m_charBytes = null;

			m_buffer = new Byte[16];

			m_disposed = false;
		}

		private void FillBuffer(Int32 numBytes)
		{
			Int32 i1 = 0;
			Int32 i = 0;

			if (this.m_stream == null)
			{
				// Error File Not Open
				FileNotOpen();
			}

			if (numBytes == 1)
			{
				i1 = this.m_stream.ReadByte();
				if (i1 == -1)
				{
					EndOfFile();
				}
				this.m_buffer[0] = (Byte)i1;
				return;
			}

			do
			{
				i1 = this.m_stream.Read(this.m_buffer, i, (numBytes - i));
				if (i1 == 0)
				{
					EndOfFile();
				}
				i += i1;
			} while (i < numBytes);
		}

		private Int32 InternalReadOneChar()
		{
			Int32 i = 0;
			Int32 i1 = 0;
			Int32 i2;

			if (m_charBytes == null)
			{
				m_charBytes = new Byte[MaxCharBytesSize];
			}

			if (m_singleChar == null)
			{
				m_singleChar = new Char[1];
			}

			while (i == 0)
			{
				i1 = m_2BytesPerChar ? 2 : 1;
				i2 = m_stream.ReadByte();
				m_charBytes[0] = (Byte)i2;
				if (i2 == -1)
				{
					i1 = 0;
				}

				if (i1 == 2)
				{
					i2 = m_stream.ReadByte();
					m_charBytes[1] = (Byte)i2;
					if (i2 == -1)
					{
						i1 = 1;
					}
				}

				if (i1 != 0)
				{
					i = m_decoder.GetChars(m_charBytes, 0, i1, m_singleChar, 0);
				}
				else
				{
					return -1;
				}
			}
			if (i != 0)
			{
				return m_singleChar[0];
			}

			return -1;
		}
		private Int32 InternalReadChars(Char[] buffer, Int32 index, Int32 count)
		{
			int i = 0;
			int i1 = 0;
			int i2 = count;
			if (m_charBytes == null)
			{
				m_charBytes = new Byte[i2];
			}
			do
			{
				i1 = i2;
				if (m_2BytesPerChar)
				{
					i1 = i1 << 1;
				}
				if (i1 > 128)
				{
					i1 = 128;
				}

				i1 = m_stream.Read(m_charBytes, 0, i1);
				if (i1 == 0)
				{
					return count - i2;
				}

				i = m_decoder.GetChars(m_charBytes, 0, i1, buffer, index);
				i2 -= i;
				index += i;
			} while (i2 > 0);

			return count;
		}

		private void EndOfFile() 
		{ 
			throw new Exception("EndOfFile"); 
		}
		private void FileNotOpen() 
		{ 
			throw new Exception("FileNotOpen"); 
		}

		private bool m_2BytesPerChar;
		private Byte[] m_buffer;
		private System.Char[] m_charBuffer;
		private Byte[] m_charBytes;
		private System.Text.Decoder m_decoder;
		private System.Char[] m_singleChar;
		const Int32 MaxCharBytesSize = 128;

		private Stream m_stream;
		private bool m_littleEndian;
		private bool m_disposed;
	};

}
