#pragma once

using namespace::System;
using namespace::System::Collections;

namespace UniversalFileExplorer
{
	public ref class FileCheckInfo
	{
	public:
		FileCheckInfo(void);
		virtual ~FileCheckInfo(void);
		void NewMessage(String^ message);
		void NewWarning(String^ message);
		void NewError(String^ message);

		bool HasErrors() { return (m_NumErrors > 0); };
		bool HasWarnings() { return (m_NumWarnings > 0); };

		array<String^>^ GetInfo();

	private:
		ArrayList^ m_FileCheckData;

		// NYI
		bool m_ShowSummary;


		int m_NumWarnings;
		int m_NumErrors;
	};
};