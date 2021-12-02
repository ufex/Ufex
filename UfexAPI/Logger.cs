using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Ufex.API
{
	public struct InfoLogEntry
	{
		public string title;
		public string className;
		public string funcName;
		public string message;
	}

	public struct ErrorLogEntry
	{
		public string title;		
		public string className;
		public string funcName;
		public string message;
	}

	public struct ExceptionLogEntry
	{
		public Exception e;
		public string className;
		public string funcName;
		public string description;
	}

	public class Logger
	{
		private int numInfo;
		private int numErrors;
		private int numExceptions;
		private ArrayList m_DebugInfo;

		private bool m_WriteToLog;
		private String m_LogFileName;
		private String m_LogFileDir;
		private String m_LogFilePath;
		private StreamWriter m_SW;

		public Logger()
		{
			m_DebugInfo = new ArrayList(1);
			m_LogFileName = null;
			m_WriteToLog = false;
		}
		public Logger(String logFileName)
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

		public void SetLogName(String fileName)
		{
			// Set the log file name
			m_LogFileName = fileName;

			// Enable writing to the log file
			m_WriteToLog = true;

			// Determine the log file directory
			m_LogFileDir = Path.GetDirectoryName(Application.ExecutablePath) + "\\Logs";

			// Create the path if it doesnt exist
			if (!Directory.Exists(m_LogFileDir))
				Directory.CreateDirectory(m_LogFileDir);

			// Set the log file full path (dir + fileName)
			m_LogFilePath = String.Concat(m_LogFileDir, "\\", m_LogFileName);
		}

		[Obsolete("Use Info instead")]
		public void NewInfo(String message, String className = "", String funcName = "", String title = "Debug Info") {  Info(message, className, funcName, title); }

		[Obsolete("Use Error instead")]
		public void NewError(String message, String className = "", String funcName = "", String title = "Error") { Error(message, className, funcName, title); }

		public void Info(String message, String className = "", String funcName = "", String title = "Debug Info")
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

		public void Error(String message, String className = "", String funcName = "", String title = "Error")
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
				String logmessage;
				logmessage = String.Format("{0},{1}", message, funcName);
				WriteToLog("Error", logmessage);
			}
		}

		public void NewException(Exception e, String description = "", String funcName = "", String className = "")
		{
			ExceptionLogEntry tmpException = new ExceptionLogEntry();
			tmpException.e = e;
			tmpException.className = className;
			tmpException.funcName = funcName;
			tmpException.description = description;
			m_DebugInfo.Add(tmpException);
			if (m_WriteToLog)
			{
				String message;
				message = String.Format("{0},{1},{2}", e.Message, description, funcName);
				WriteToLog("Exception", message);
			}
		}


		private void WriteToLog(String type, String message)
		{
			if (m_WriteToLog && m_LogFilePath != null)
			{
				m_SW = File.AppendText(m_LogFilePath);
				m_SW.WriteLine(String.Format("<{0},{1}>", type, message));
				m_SW.Close();
			}
		}
	}
}
