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

	private bool writeToLog;
	private string logFileName;
	private string logFileDir;
	private string logFilePath;
	private StreamWriter streamWriter;

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
		logFileName = null;
		writeToLog = false;
	}
	public Logger(string logFileName)
	{
		m_DebugInfo = new ArrayList(1);
		writeToLog = false;
		this.logFileName = null;
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
		logFileName = fileName;

		// Enable writing to the log file
		writeToLog = true;

		// Determine the log file directory
		logFileDir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"ufex",
			"Logs"
		);

		// Create the path if it doesnt exist
		if (!Directory.Exists(logFileDir))
			Directory.CreateDirectory(logFileDir);

		// Set the log file full path (dir + fileName)
		logFilePath = String.Concat(logFileDir, "\\", logFileName);
	}

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
		if (writeToLog)
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
		if (writeToLog)
		{
			string message;
			message = String.Format("{0},{1},{2}", e.Message, description, funcName);
			WriteToLog("Exception", message);
		}
	}

	private void WriteToLog(string type, string message)
	{
		if (writeToLog && logFilePath != null)
		{
			streamWriter = File.AppendText(logFilePath);
			streamWriter.WriteLine(String.Format("{0}: {1}", type, message));
			streamWriter.Close();
		}
	}
}
