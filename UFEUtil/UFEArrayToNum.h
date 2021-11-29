// UFEArrayToNum.h
#pragma once

using namespace System;
using namespace System::Text;

namespace UniversalFileExplorer
{

	public enum class Endian : int
	{
		Big,
		Little
	};

	public ref class ArrayToNum sealed
	{
	public:

		ArrayToNum();
		ArrayToNum(Endian endian);
		
		UInt16 GetUInt16(array<Byte>^ data) { return GetUInt16(data, 0); };
		UInt16 GetUInt16(array<Byte>^ data, int offset);
		
		UInt32 GetUInt32(array<Byte>^ data) { return GetUInt32(data, 0); };
		UInt32 GetUInt32(array<Byte>^ data, int offset);
		
		UInt64 GetUInt64(array<Byte>^ data) { return GetUInt64(data, 0); };
		UInt64 GetUInt64(array<Byte>^ data, int offset);

		Int16 GetInt16(array<Byte>^ data) { return GetInt16(data, 0); };
		Int16 GetInt16(array<Byte>^ data, int offset);
		
		Int32 GetInt32(array<Byte>^ data) { return GetInt32(data, 0); };
		Int32 GetInt32(array<Byte>^ data, int offset);
		
		// Properties
		property Endian DataEndian {
			Endian get() { return m_endian; }
			void set(Endian endian) { m_endian = endian; }
		}
		
	private:
		Endian m_endian;
		
		// Throws an exception because the endian is not valid
		int BadEndian();

	};
};