#pragma unmanaged

typedef unsigned char       BYTE;
typedef unsigned short      WORD;
typedef unsigned long       DWORD;

namespace UniversalFileExplorer
{
	class StringPack
	{
		public:
			StringPack();
			virtual ~StringPack();
			bool Unpack(BYTE in[], char* &out, int size);
			bool Pack(char* in, BYTE out[], int size);

		private:
			BYTE GetCodeA(BYTE);
			BYTE GetCodeB(BYTE);
			BYTE setBit(BYTE&, BYTE, bool);
			bool getBit(BYTE, BYTE);

			unsigned long m_stringKey;
			unsigned long m_numberKey;
	};
};