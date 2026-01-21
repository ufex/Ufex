#include "Stdafx.h"
#include "BinManip.h"
#include "UFEArrayToNum.h"

namespace UniversalFileExplorer
{	
	ArrayToNum::ArrayToNum()
	{
		if(BitConverter::IsLittleEndian)
			m_endian = Endian::Little;
		else
			m_endian = Endian::Big;
	}

	ArrayToNum::ArrayToNum(Endian endian)
	{
		m_endian = endian;
	}

	UInt16 ArrayToNum::GetUInt16(array<Byte>^ data, int offset)
	{
		if(m_endian == Endian::Little)
			return data[offset] + (data[offset + 1] * 0x100);
		else if(m_endian == Endian::Big)
			return (data[offset] * 0x100) + data[offset + 1];
		else
			return BadEndian();
	}
	
	UInt32 ArrayToNum::GetUInt32(array<Byte>^ data, int offset)
	{
		if(m_endian == Endian::Little)
		{
			return (data[offset    ]) 
				+  (data[offset + 1] * 0x100) 
				+  (data[offset + 2] * 0x10000) 
				+  (data[offset + 3] * 0x1000000);
		}
		else if(m_endian == Endian::Big)
		{
			return (data[offset    ] * 0x1000000)
				+  (data[offset + 1] * 0x10000) 
				+  (data[offset + 2] * 0x100) 
				+  (data[offset + 3]);
		}
		return 0;
	}
	
	UInt64 ArrayToNum::GetUInt64(array<Byte>^ data, int off)
	{
		UInt32 a = 0, b = 0;
		if(m_endian == Endian::Little)
		{
			a = ((data[off + 0] | (data[off + 1] << 8)) | (data[off + 2] << 16)) | (data[off + 3] << 24);
			b = ((data[off + 4] | (data[off + 5] << 8)) | (data[off + 6] << 16)) | (data[off + 7] << 24);
		}
		else if(m_endian == Endian::Big)
		{
			a = ((data[off + 7] | (data[off + 6] << 8)) | (data[off + 5] << 16)) | (data[off + 4] << 24);
			b = ((data[off + 3] | (data[off + 2] << 8)) | (data[off + 1] << 16)) | (data[off + 0] << 24);
		}
		return (((UInt64)(b)) << 32) | ((UInt64)(a));
	}

	Int16 ArrayToNum::GetInt16(array<Byte>^ data, int off)
	{
		if(m_endian == Endian::Little)
			return (Int16)((data[off + 0] & 255) | (data[off + 1] << 8));
		else if(m_endian == Endian::Big)
			return (Int16)((data[off + 1] & 255) | (data[off + 0] << 8));
		else
			return BadEndian();
	}


	Int32 ArrayToNum::GetInt32(array<Byte>^ data, int off)
	{
		if(m_endian == Endian::Little)
			return (((data[off + 0] & 255) | (data[off + 1] << 8)) | (data[off + 2] << 16)) | (data[off + 3] << 24);
		else if(m_endian == Endian::Big)
			return (((data[off + 3] & 255) | (data[off + 2] << 8)) | (data[off + 1] << 16)) | (data[off] << 24);
		else
			return BadEndian();
	}

	int ArrayToNum::BadEndian()
	{
		throw gcnew Exception("Invalid Endian");
		return 0;
	}

};