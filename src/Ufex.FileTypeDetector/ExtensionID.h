using namespace System;
using namespace System::IO;
using namespace System::Text;

namespace UniversalFileExplorer
{
	public ref class ExtensionID : public FileTypeClassifier
	{
	public:
		ExtensionID(void);
		String^ GetFileType(String^ fp, FileStream^ fs) override;
	
	private:		
		FileStream^ m_FileStream;
		BinaryReader^ m_BinaryReader;

	};
};