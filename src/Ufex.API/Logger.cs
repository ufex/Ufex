using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Ufex.API;

public struct InfoLogEntry
{
	public string title;
	public string className;
	public string funcName;
	public string message;

	public override string ToString()
	{
		return title + ": " + message + " (" + className + "." + funcName + ")";
	}
}

public struct ErrorLogEntry
{
	public string title;		
	public string className;
	public string funcName;
	public string message;
	public override string ToString()
	{
		return title + ": " + message + " (" + className + "." + funcName + ")";
	}
}

public struct ExceptionLogEntry
{
	public Exception e;
	public string className;
	public string funcName;
	public string description;
	public override string ToString()
	{
		return "ERROR: " + e.ToString() + ", " + description + " (" + className + "." + funcName + ")";
	}
}

public class Logger
{
	private int numInfo;
	private int numErrors;
	private int numExceptions;
	private ArrayList m_DebugInfo;

	private bool m_WriteToLog;
	private string m_LogFileName;
	private string m_LogFileDir;
	private string m_LogFilePath;
	private StreamWriter m_SW;

	public string Text
	{
		get
		{
			StringBuilder sb = new StringBuilder();
			foreach(object entry in m_DebugInfo)
			{
				sb.AppendLine(entry.ToString());
			}
			return sb.ToString();
		}
	}

	public Logger()
	{
		m_DebugInfo = new ArrayList(1);
		m_LogFileName = null;
		m_WriteToLog = false;
	}
	public Logger(string logFileName)
	{
		m_DebugInfo = new ArrayList(1);
		m_WriteToLog = false;
		m_LogFileName = null;
		SetLogName(logFileName);
	}
	~Logger()
	{

	}

	public ArrayList GetAllDebugItems() 
	{ 
		return m_DebugInfo;
	}
	public int GetNumObjects() 
	{ 
		return m_DebugInfo.Count; 
	}

	public void SetLogName(string fileName)
	{
		// Set the log file name
		m_LogFileName = fileName;

		// Enable writing to the log file
		m_WriteToLog = true;

		// Determine the log file directory
		m_LogFileDir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"ufex",
			"Logs"
		);

		// Create the path if it doesnt exist
		if (!Directory.Exists(m_LogFileDir))
			Directory.CreateDirectory(m_LogFileDir);

		// Set the log file full path (dir + fileName)
		m_LogFilePath = String.Concat(m_LogFileDir, "\\", m_LogFileName);
	}

	[Obsolete("Use Info instead")]
	public void NewInfo(string message, string className = "", string funcName = "", string title = "Debug Info") {  Info(message, className, funcName, title); }

	[Obsolete("Use Error instead")]
	public void NewError(string message, string className = "", string funcName = "", string title = "Error") { Error(message, className, funcName, title); }

	public void Info(string message, string className = "", string funcName = "", string title = "Debug Info")
	{
		// Create a new UFE_INFO object
		InfoLogEntry tmpInfo = new InfoLogEntry();

		// Set the member vars
		tmpInfo.message = message;
		tmpInfo.className = className;
		tmpInfo.funcName = funcName;
		tmpInfo.title = title;

		// Add the object to the m_DebugInfo ArrayList
		m_DebugInfo.Add(tmpInfo);

		WriteToLog("Info", message);
	}

	public void Error(string message, string className = "", string funcName = "", string title = "Error")
	{
		// Create a new UFE_ERROR object
		ErrorLogEntry tmpError = new ErrorLogEntry();

		tmpError.message = message;
		tmpError.className = className;
		tmpError.funcName = funcName;
		tmpError.title = title;
		m_DebugInfo.Add(tmpError);
		if (m_WriteToLog)
		{
			string logmessage;
			logmessage = String.Format("{0},{1}", message, funcName);
			WriteToLog("Error", logmessage);
		}
	}

	public void NewException(Exception e, string description = "", string funcName = "", string className = "")
	{
		ExceptionLogEntry tmpException = new ExceptionLogEntry();
		tmpException.e = e;
		tmpException.className = className;
		tmpException.funcName = funcName;
		tmpException.description = description;
		m_DebugInfo.Add(tmpException);
		if (m_WriteToLog)
		{
			string message;
			message = String.Format("{0},{1},{2}", e.Message, description, funcName);
			WriteToLog("Exception", message);
		}
	}

	private void WriteToLog(string type, string message)
	{
		if (m_WriteToLog && m_LogFilePath != null)
		{
			m_SW = File.AppendText(m_LogFilePath);
			m_SW.WriteLine(String.Format("<{0},{1}>", type, message));
			m_SW.Close();
		}
	}
}
