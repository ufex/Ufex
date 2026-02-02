using System;
using System.Collections;
using System.Diagnostics;
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
	private ArrayList logEntries;

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
			foreach(object entry in logEntries)
			{
				sb.AppendLine(entry.ToString());
			}
			return sb.ToString();
		}
	}

	public Logger()
	{
		logEntries = new ArrayList(1);
		logFileName = null;
		writeToLog = false;
	}

	public Logger(string logFileName)
	{
		logEntries = new ArrayList(1);
		writeToLog = false;
		this.logFileName = null;
		SetLogName(logFileName);
	}

	public ArrayList GetAllDebugItems() 
	{ 
		return logEntries;
	}

	public int GetNumObjects() 
	{ 
		return logEntries.Count; 
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

		// Add the object to the logEntries ArrayList
		logEntries.Add(tmpInfo);

		WriteToLog("Info", message);
	}

	public void Error(string message, string className = "", string funcName = "", string title = "Error")
	{
		// Create a new ErrorLogEntry object
		ErrorLogEntry tmpError = new ErrorLogEntry();

		tmpError.message = message;
		tmpError.className = className;
		tmpError.funcName = funcName;
		tmpError.title = title;
		logEntries.Add(tmpError);
		if(writeToLog)
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
		logEntries.Add(tmpException);
		if(writeToLog)
		{
			string message;
			message = String.Format("{0},{1},{2}", e.Message, description, funcName);
			// Append call stack (with file/line numbers when PDBs are available)
			message = String.Concat(message, ", StackTrace: ", FormatStackTrace(e));
			WriteToLog("Exception", message);
		}
	}

	private static string FormatStackTrace(Exception e)
	{
		try
		{
			var st = new StackTrace(e, true);
			var frames = st.GetFrames();
			if (frames == null || frames.Length == 0)
				return e.StackTrace ?? string.Empty;

			var sb = new StringBuilder();
			for (int i = 0; i < frames.Length; i++)
			{
				var frame = frames[i];
				var method = frame.GetMethod();
				string methodText = method != null ? method.ToString() ?? string.Empty : string.Empty;
				string file = frame.GetFileName() ?? string.Empty;
				int line = frame.GetFileLineNumber();

				if (sb.Length > 0)
					sb.Append(" \n ");

				if (!string.IsNullOrEmpty(file) && line > 0)
					sb.Append($"{methodText} in {file}:line {line}");
				else
					sb.Append(methodText);
			}

			return sb.ToString();
		}
		catch
		{
			return e.StackTrace ?? string.Empty;
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
