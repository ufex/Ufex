#include "StdAfx.h"
#include ".\FileReader.h"
#using <mscorlib.dll>


namespace UniversalFileExplorer
{
/*	
	FileReader::FileReader(IO::Stream* input)
	{
		FileReader(input, new Text::UTF8Encoding);
		return;
	}
*/		
	void FileReader::Init(IO::Stream^ input, bool littleEndian, Text::Encoding^ encoding)
	{

		m_stream = input;

		m_decoder = encoding->GetDecoder();

		m_littleEndian = littleEndian;
		
		m_charBuffer = nullptr;
		m_charBytes = nullptr;
		
		m_buffer = gcnew array<Byte>(16);
		
		m_disposed = false;
	}

	/*void FileReader::Dispose()
	{
		Dispose(true);
	}

	void FileReader::Dispose(Boolean disposing)
	{
		IO::Stream^ stream;
		if(disposing)
		{
			stream = m_stream;
			m_stream = nullptr;
			if(stream != nullptr)
			{
				stream->Close();
			}
		}
		m_stream = nullptr;
		m_buffer = nullptr;
		m_decoder = nullptr;
		m_charBytes = nullptr;
		m_singleChar = nullptr;
		m_charBuffer = nullptr;
		return;
	}*/

	void FileReader::Close()
	{
		this->!FileReader();
		m_disposed = true;
	}

	void FileReader::FillBuffer(int numBytes)
	{
		Int32 i1;
		Int32 i = 0;
		i1 = 0;

		if(this->m_stream == nullptr)
		{
			// Error File Not Open
			FileNotOpen();
		}

		if(numBytes == 1)
		{
			i1 = this->m_stream->ReadByte();
			if(i1 == -1)
			{
				EndOfFile();
			}
			this->m_buffer[0] = (System::Byte)(i1);
  			return;
		}

		do
		{
			i1 = this->m_stream->Read(this->m_buffer, i, (numBytes - i));
			if(i1 == 0)
			{
				EndOfFile();
			}
			i += i1;
		} while(i < numBytes);

		return;
	}


	Int32 FileReader::Read()
	{
		if(m_stream == nullptr)
			FileNotOpen();

		return InternalReadOneChar();
	}


	System::Boolean FileReader::ReadBoolean()
	{
		FillBuffer(1);
		return ((m_buffer[0] == 0) == 0);
	}
	
	Byte FileReader::ReadByte()
	{
		FillBuffer(1);
		return m_buffer[0];
	}

	array<Byte>^ FileReader::ReadBytes(Int32 count)
	{
		array<Byte>^ arr;

		arr = gcnew array<System::Byte>(count);
		m_stream->Read(arr, 0, count);
		
		return arr;
	}

	Char FileReader::ReadChar()
	{
		Int32 i;
		i = this->Read();
		if (i == -1)
		{
			EndOfFile();
		}
		return (UInt16)i;
	}

	array<Char>^ FileReader::ReadChars(Int32 count)
	{
		array<System::Char>^ arr;
		System::Int32 i;
		array<System::Char>^ arr1;
  
		if(m_stream == nullptr)
		{
			FileNotOpen();
		}
		
		if(count < 0)
		{
			throw gcnew System::ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
		}
		
		arr = gcnew array<System::Char>(count);
		i = InternalReadChars(arr, 0, count);
  
		if(i != count)
		{
			arr1 = gcnew array<System::Char>(i);
			Buffer::BlockCopy(arr, 0, arr1, 0, (2 * i));
			arr = arr1;
		}
		return arr;
	}

	Decimal FileReader::ReadDecimal()
	{
		return NULL;
	}

	Double FileReader::ReadDouble()
	{
		return NULL;
	}


	Int16 FileReader::ReadInt16()
	{
		FillBuffer(2);
		if(m_littleEndian)
			return (Int16)((m_buffer[0] & 255) | (m_buffer[1] << 8));
		else
			return (Int16)((m_buffer[1] & 255) | (m_buffer[0] << 8));
	}

	Int32 FileReader::ReadInt32()
	{
		FillBuffer(4);
		if(m_littleEndian)
			return (((m_buffer[0] & 255) | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24);
		else
			return (((m_buffer[3] & 255) | (m_buffer[2] << 8)) | (m_buffer[1] << 16)) | (m_buffer[0] << 24);
	}

	Int64 FileReader::ReadInt64()
	{
		UInt32 a;
		UInt32 b;

		// Read 8 bytes
		FillBuffer(8);
		if(m_littleEndian)
		{
			a = ((m_buffer[0] | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24);
			b = ((m_buffer[4] | (m_buffer[5] << 8)) | (m_buffer[6] << 16)) | (m_buffer[7] << 24);
		}
		else
		{
			a = ((m_buffer[7] | (m_buffer[6] << 8)) | (m_buffer[5] << 16)) | (m_buffer[4] << 24);
			b = ((m_buffer[3] | (m_buffer[2] << 8)) | (m_buffer[1] << 16)) | (m_buffer[0] << 24);
		}
		return (((UInt64)(b)) << 32) | ((UInt64)(a));
	}

	SByte FileReader::ReadSByte()
	{
		FillBuffer(1);
		return ((System::SByte)(m_buffer[0]));
	}

	Single FileReader::ReadSingle()
	{

		return NULL;
	}

	String^ FileReader::ReadString()
	{
		return "";
	}

	String^ FileReader::ReadNullTermString()
	{
		Byte lastByte;
		StringBuilder^ sb = gcnew StringBuilder();

		do
		{
			lastByte = m_stream->ReadByte();	
		
			if(lastByte != 0x00)
				sb->Append((Char)lastByte);
		
		} while(lastByte != 0x00);

		return sb->ToString();
	}

	UInt16 FileReader::ReadUInt16()
	{
		this->FillBuffer(2);
		if(m_littleEndian)
			return (UInt16)((this->m_buffer[0] & 255) | (this->m_buffer[1] << 8));
		else
			return (UInt16)((this->m_buffer[0] << 8) | (this->m_buffer[1] & 255));
	}

	UInt32 FileReader::ReadUInt32()
	{
		this->FillBuffer(4);
		if(m_littleEndian)
			return (UInt32)(((m_buffer[0] | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24));
		else
			return (UInt32)(((m_buffer[3] | (m_buffer[2] << 8)) | (m_buffer[1] << 16)) | (m_buffer[0] << 24));
	}

	UInt64 FileReader::ReadUInt64()
	{
		UInt32 a;
		UInt32 b;
		FillBuffer(8);
		if(m_littleEndian)
		{
			a = ((m_buffer[0] | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24);
			b = ((m_buffer[4] | (m_buffer[5] << 8)) | (m_buffer[6] << 16)) | (m_buffer[7] << 24);
		}
		else
		{
			a = ((m_buffer[7] | (m_buffer[6] << 8)) | (m_buffer[5] << 16)) | (m_buffer[4] << 24);
			b = ((m_buffer[3] | (m_buffer[2] << 8)) | (m_buffer[1] << 16)) | (m_buffer[0] << 24);
		}
		return (((UInt64)(b)) << 32) | ((UInt64)(a));
	}

	Guid FileReader::ReadGuid()
	{
		FillBuffer(16);
		return Guid(m_buffer);
	}

	Int32 FileReader::InternalReadOneChar()
	{
		Int32 i;
		Int32 i1;
		Int32 i2;
		i = 0;
		i1 = 0;
		if(m_charBytes == nullptr)
		{
			m_charBytes = gcnew array<Byte>(MaxCharBytesSize);
		}
  
		if(m_singleChar == nullptr)
		{
			m_singleChar = gcnew array<Char>(1);
		}

		while(i == 0)
		{
			i1 = ((m_2BytesPerChar != 0) ? 2 : 1);
			i2 = m_stream->ReadByte();
			m_charBytes[0] = (Byte)i2;
			if(i2 == -1)
			{
				i1 = 0;
			}
			
			if(i1 == 2)
			{
				i2 = m_stream->ReadByte();
				m_charBytes[1] = (Byte)i2;
				if(i2 == -1)
				{
					i1 = 1;
				}
			}

			if(i1 != 0)
			{
				i = m_decoder->GetChars(m_charBytes, 0, i1, m_singleChar, 0);
			}
			else
			{
				return -1;
			}
		}
		if(i != 0)
		{
			return m_singleChar[0]; 
		}

		return -1;
	}


	Int32 FileReader::InternalReadChars(array<Char>^ buffer, Int32 index, Int32 count)
	{
		int i = 0;
		int i1 = 0;
		int i2 = count;
		if(m_charBytes == nullptr)
		{
			m_charBytes = gcnew array<System::Byte>(i2);
		}
ILO_0020:
  i1 = i2;
  if (m_2BytesPerChar)
  {
    i1 = i1 << 1;
  }
  if (i1 > 128)
  {
    i1 = 128;
  }
  
  i1 = m_stream->Read(m_charBytes, 0, i1);
	if(i1 == 0)
	{
		return count - i2;
	}
 
  i = m_decoder->GetChars(m_charBytes, 0, i1, buffer, index);
  i2 -= i;
  index += i;

  if (i2 > 0)
  {
    goto ILO_0020;
  }
  return count;
	}


};
