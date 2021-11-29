#include "StdAfx.h"
#include ".\UFEDebug.h"

#ifdef CreateDirectory
	#undef CreateDirectory
#endif

//#define LOGOUT(text)	if(m_WriteToLog) 

namespace UniversalFileExplorer
{
	UFEDebug::UFEDebug(void)
	{
		m_DebugInfo = gcnew ArrayList(1);
		m_LogFileName = nullptr;
		m_WriteToLog = false;
	}
	
	UFEDebug::UFEDebug(String^ logFileName)
	{
		m_DebugInfo = gcnew ArrayList(1);
		m_WriteToLog = false;
		m_LogFileName = nullptr; 
		SetLogName(logFileName);
	}

	UFEDebug::~UFEDebug(void)
	{
		// Caused a problem
		//m_DebugInfo = NULL;
	}
	
	void UFEDebug::SetLogName(String^ fileName) 
	{ 
		// Set the log file name
		m_LogFileName = fileName;

		// Enable writing to the log file
		m_WriteToLog = true;

		// Determine the log file directory
		m_LogFileDir = String::Concat(Path::GetDirectoryName(Application::ExecutablePath), "\\Logs");

		// Create the path if it doesnt exist
		if(!Directory::Exists(m_LogFileDir))
			Directory::CreateDirectory(m_LogFileDir);

		// Set the log file full path (dir + fileName)
		m_LogFilePath = String::Concat(m_LogFileDir, "\\", m_LogFileName);
	}
	
	
	void UFEDebug::NewInfo(String^ message, String^ className, String^ funcName, String^ title)
	{
		// Create a new UFE_INFO object
		UFE_INFO^ tmpInfo = gcnew UFE_INFO;
		
		// Set the member vars
		tmpInfo->message = message;
		tmpInfo->className = className;
		tmpInfo->funcName = funcName;
		tmpInfo->title = title;
		
		// Add the object to the m_DebugInfo ArrayList
		m_DebugInfo->Add(tmpInfo);
		
		WriteToLog("Info", message);
	}

	void UFEDebug::NewError(String^ message, String^ className, String^ funcName, String^ title)
	{		
		// Create a new UFE_ERROR object
		UFE_ERROR^ tmpError = gcnew UFE_ERROR;

		tmpError->message = message;
		tmpError->className = className;
		tmpError->funcName = funcName;
		tmpError->title = title;
		m_DebugInfo->Add(tmpError);
		if(m_WriteToLog)
		{
			String^ logmessage;
			logmessage = String::Format("{0},{1}", message, funcName);
			WriteToLog("Error", logmessage);
		}
	}

	void UFEDebug::NewException(Exception^ e, String^ description, String^ funcName, String^ className)
	{
		UFE_EXCEPTION^ tmpException = gcnew UFE_EXCEPTION;
		tmpException->e = e;
		tmpException->className = className;
		tmpException->funcName = funcName;
		tmpException->description = description;
		m_DebugInfo->Add(tmpException);
		if(m_WriteToLog)
		{
			String^ message;
			message = String::Format("{0},{1},{2}", e->Message, description, funcName);
			WriteToLog("Exception", message);
		}
	}

	void UFEDebug::WriteToLog(String^ type, String^ message)
	{
		if(m_WriteToLog && m_LogFilePath != nullptr)
		{
			m_SW = File::AppendText(m_LogFilePath);
			m_SW->WriteLine(String::Format("<{0},{1}>", type, message));
			m_SW->Close();
		}
	}

};