#pragma once

using namespace System;
using namespace System::Data;
using namespace System::IO;
using namespace System::Collections;
using namespace System::Text;

namespace UniversalFileExplorer
{
	public ref class FileReader : public System::Object, public IDisposable
	{
	public:
		FileReader(IO::Stream^ input) { Init(input, true, gcnew Text::UTF8Encoding); };
		FileReader(IO::Stream^ input, bool littleEndian) { Init(input, littleEndian, gcnew Text::UTF8Encoding); };
		FileReader(IO::Stream^ input, bool littleEndian, Text::Encoding^ encoding) { Init(input, littleEndian, encoding); };

		void Close();

		Int32 Read();

		System::Boolean ReadBoolean();

		array<Byte>^ ReadBytes(Int32 count);
		array<Byte>^ ReadBytes(UInt16 count) { return ReadBytes((int)count); };
		Char ReadChar();
		array<Char>^ ReadChars(Int32 count);
		Decimal ReadDecimal();
		Double ReadDouble();
		Single ReadSingle();
		String^ ReadString();

		// Read the signed integer types
		Int16 ReadInt16();
		Int32 ReadInt32();
		Int64 ReadInt64();
		SByte ReadSByte();
		
		// Read the unsinged integer types
		Byte ReadByte();
		UInt16 ReadUInt16();
		UInt32 ReadUInt32();
		UInt64 ReadUInt64();

		// Reads a null terminated string
		String^ ReadNullTermString();

		Guid ReadGuid();

		
		~FileReader() {
			if (m_disposed) {
				return;
			}
			this->!FileReader();
			m_disposed = true;
		}

		property Stream^ BaseStream 
		{
			Stream^ get() { return m_stream; }
			void set(Stream^ baseStream) { m_stream = baseStream; }
		}


	protected:
		inline void Init(IO::Stream^ input, bool littleEndian, Text::Encoding^ encoding);

		!FileReader() {
			IO::Stream^ stream;
			stream = m_stream;
			m_stream = nullptr;
			if (stream != nullptr)
			{
				stream->Close();
			}
			m_stream = nullptr;
			m_buffer = nullptr;
			m_decoder = nullptr;
			m_charBytes = nullptr;
			m_singleChar = nullptr;
			m_charBuffer = nullptr;
			return;
		};


	private:
		void FillBuffer(Int32 numBytes);

		Int32 InternalReadOneChar();
		Int32 InternalReadChars(array<Char>^ buffer, Int32 index, Int32 count);


		void EndOfFile() { throw gcnew Exception("EndOfFile"); };
		void FileNotOpen() { throw gcnew Exception("FileNotOpen"); };

		bool m_2BytesPerChar;
		array<Byte>^ m_buffer;
		array<System::Char>^ m_charBuffer;
		array<System::Byte>^ m_charBytes;
		Text::Decoder^ m_decoder;
		array<System::Char>^ m_singleChar;
		static const Int32 MaxCharBytesSize = 128;

		Stream^ m_stream;
		bool m_littleEndian;
		bool m_disposed;
	};


};