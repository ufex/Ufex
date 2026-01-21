#pragma once

using namespace System;
using namespace System::Collections;
using namespace System::IO;
using namespace System::Windows::Forms;


namespace UniversalFileExplorer
{
	public ref struct UFE_INFO
	{
		String^ title;
		String^ className;
		String^ funcName;
		String^ message;
	};

	public ref struct UFE_ERROR
	{
		String^ title;		
		String^ className;
		String^ funcName;
		String^ message;
	};
	
	public ref struct UFE_EXCEPTION
	{
		Exception^ e;
		String^ className;
		String^ funcName;
		String^ description;
	};

	public ref class UFEDebug
	{
	public:
		UFEDebug(void);
		UFEDebug(String^ logFileName);
		virtual ~UFEDebug(void);

		ArrayList^ GetAllDebugItems() { return m_DebugInfo; };
		int GetNumObjects() { return m_DebugInfo->Count; };

		void SetLogName(String^ fileName);

		void NewInfo(String^ message) { NewInfo(message, "", "", "Debug Info"); };
		void NewInfo(String^ message, String^ className) { NewInfo(message, className, "", "Debug Info"); };
		void NewInfo(String^ message, String^ className, String^ funcName) { NewInfo(message, className, funcName, "Debug Info"); };
		void NewInfo(String^ message, String^ className, String^ funcName, String^ title);

		void NewError(String^ message) { NewError(message, "", "", "Error"); };
		void NewError(String^ message, String^ className) { NewError(message, className, "", "Error"); };
		void NewError(String^ message, String^ className, String^ funcName) { NewError(message, className, funcName, "Error"); };
		void NewError(String^ message, String^ className, String^ funcName, String^ title);

		void NewException(Exception^ e)	{ NewException(e, "", "", ""); };
		void NewException(Exception^ e, String^ description) { NewException(e, description, "", ""); };
		void NewException(Exception^ e, String^ description, String^ funcName){ NewException(e, description, funcName, ""); };
		void NewException(Exception^ e, String^ description, String^ funcName, String^ className);

	private:

		void WriteToLog(String^ type, String^ message);

		int numInfo;
		int numErrors;
		int numExceptions;
		ArrayList^ m_DebugInfo;

		bool m_WriteToLog;
		String^ m_LogFileName;
		String^ m_LogFileDir;
		String^ m_LogFilePath;
		StreamWriter^ m_SW;
	};
};