// FileType.h

#pragma once

#include ".\FileCheckInfo.h"

using namespace System;
using namespace System::Windows::Forms;
using namespace System::Collections;
using namespace System::Data;
using namespace System::IO;
using namespace System::Drawing;
using namespace System::Text;
using namespace System::Resources;
using namespace System::Reflection;

namespace UniversalFileExplorer
{


// Endian Values
#define DEFAULT_ENDIAN	0x00
#define BIG_ENDIAN		0x01
#define LITTLE_ENDIAN	0x02

	public ref struct INFO
	{
		int type;		// 0 = info, 1, = warning, 2 = error
		bool exception;
		String^ time;
		String^ className;
		String^ funcName;
		String^ title;
		String^ message;
		Exception^ e;
	};

	public ref struct PROPERTIES
	{
		int numProperties;
		array<String^>^ properties;
		array<String^>^ values;
	};
		

	public ref class FileType
	{	
	public:
		// Default Constructor
		FileType(void);

		// Deconstructor
		virtual ~FileType(void);	

		// Virtual function to process the file
		virtual bool ProcessFile();

		// Returns the data table that corresponds to the treenode
		virtual TableData^ GetData(TreeNode^ tn);
		
		// Returns the Quick Info for the file
		virtual QuickInfoTable^ GetQuickInfo();

		// Returns the File's Creator
		virtual String^ GetFileCreator();
		
		// Returns an image representation of the file
		virtual Image^ GetImage();

		// Add an icon - returns the icon id number
		int AddIcon(Icon^ ico) { return m_Icons->Add(ico); };

		int GetNumIcons() { return m_Icons->Count; };
		
		Icon^ GetIcon(int i) { return static_cast<Icon^>(m_Icons[i]); };

		// Functions for interfacing with the ArrayToNum Class
		void SetATNEndian(Endian endian) { m_atn->DataEndian = endian; };


		String^ m_DebugText;

		String^ m_AppPath;

		FileStream^ m_FileStream;

	/************************************************************************************************
		
				Properties

	***********************************************************************************************/

	public:		
		property String^ FilePath {
			String^ get() { return m_filePath; }
			void set(String^ filePath) { m_filePath = filePath;  }
		}

	public:		
		property FileStream^ FileInStream {
			FileStream^ get() { return m_FileStream;  }
		}

		property NumberFormat NumFormat {
			void set(NumberFormat numFormat) { m_nts->SetNumFormat(numFormat); };
		}

		property TreeNodeCollection^ TreeNodes {
			TreeNodeCollection^ get() { return m_rootTreeNode->Nodes; }
		}

		property FileCheckInfo^ FileCheck {
			FileCheckInfo^ get() { return m_fileCheckInfo; }
		}

		property NumToString^ NTS {
			protected: NumToString^ get() { return m_nts; }
		}

		property ArrayToNum^ ATN {
			protected: ArrayToNum^ get() { return m_atn; }
		}

		property String^ ApplicationPath {
			public: String^ get() { return m_appPath; }
		}

		property String^ Description {
			public: String^ get() { return m_description; }
			protected: void set(String^ description) { m_description = description; }
		}

		property Boolean UseTreeViewIcons {
			public: Boolean get() { return m_useIcons; }
			protected: void set(Boolean useIcons) { m_useIcons = useIcons; }
		}
		
		// Determines whether the Technical View Tab is displayed
		property Boolean ShowTechnical {
			public: Boolean get() { return m_showTechnical; }
			protected: void set(Boolean showTab) { m_showTechnical = showTab; }
		}
		
		property Boolean ShowGraphic {
			public: Boolean get() { return m_showGraphic; }
			protected: void set(Boolean showTab) { m_showGraphic = showTab; }
		}

		property Boolean ShowFileCheck {
			public: Boolean get() { return m_showFileCheck; }
			protected: void set(Boolean showTab) { m_showFileCheck = showTab; }
		}

		property UFEDebug^ Debug {
			protected: UFEDebug^ get() { return m_debug; }
		}

	protected:

		// File Check Info Stuff
		void FCMessage(String^ message) { m_fileCheckInfo->NewMessage(message); };
		void FCError(String^ message) { m_fileCheckInfo->NewError(message); };
		void FCWarning(String^ message) { m_fileCheckInfo->NewWarning(message); };
		
		// Other debugging stuff
		void DebugOut(String^);
		void ExceptionOut(Exception^);
		void ExceptionOut(Exception^, String^);
		void ExceptionOut(Exception^, String^, String^);
		
		// Convert Numbers To Strings
		String^ GetStrBool(const bool x) { return m_nts->GetStrBool(x); };
		String^ GetStrU8(const Byte x) { return m_nts->GetStrByte(x); };
		String^ GetStrU16(const UInt16 x) { return m_nts->GetStrUInt16(x); };
		String^ GetStrU32(const UInt32 x) { return m_nts->GetStrUInt32(x); };
		String^ GetStrU64(const UInt64 x) { return m_nts->GetStrUInt64(x); };
		String^ GetStrI16(const Int16 x) { return m_nts->GetStrInt16(x); };
		String^ GetStrI32(const Int32 x) { return m_nts->GetStrInt32(x); };
		String^ GetStrI64(const Int64 x) { return m_nts->GetStrInt64(x); };

		String^ GetStrU8Array(array<Byte>^ x) { return m_nts->GetStrU8Array(x); };
		String^ GetStrU16Array(array<UInt16>^ x) { return m_nts->GetStrU16Array(x); };
		
		// Convert ASCII String to Unicode String
		//String* GetUniStr(unsigned char __gc[]);
		
		

	private:
		
		void LoadIcons();
		
		String^ m_filePath;

		TreeNode^ m_rootTreeNode;

		String^ m_description;

		NumberFormat m_numFormat;
		
		FileCheckInfo^ m_fileCheckInfo;
		
		ArrayToNum^ m_atn;
		NumToString^ m_nts;

		ArrayList^ m_Icons;
		
		bool m_useIcons;
		
		String^ m_appPath;

		// Tabs to be displayed
		bool m_showTechnical;
		bool m_showGraphic;
		bool m_showFileCheck;

		// 
		UFEDebug^ m_debug;
	};
};