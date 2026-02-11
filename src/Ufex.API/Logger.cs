using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

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

/// <summary>
/// A logger that implements Microsoft.Extensions.Logging.ILogger and supports
/// both file-based logging and in-memory log storage for GUI display.
/// </summary>
public class Logger : ILogger
{
	private ArrayList _entries;

	private readonly string _filePath;
	private readonly bool _writeToFile;
	private static readonly object _lock = new object();

	/// <summary>
	/// Gets the combined text of all in-memory log entries.
	/// </summary>
	public string Text
	{
		get
		{
			if (_entries == null)
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			lock (_lock)
			{
				foreach (object entry in _entries)
				{
					sb.AppendLine(entry.ToString());
				}
			}
			return sb.ToString();
		}
	}

	/// <summary>
	/// Gets the in-memory log entries collection.
	/// </summary>
	public ArrayList Entries
	{
		get { return _entries; }
	}

	[Obsolete("The log file name can only be set via the constructor. This method is a no-op and will be removed in a future version.")]
	public void SetLogName(string _)
	{
		// No-op - log file is set via constructor and cannot be changed.
	}

	/// <summary>
	/// Creates a logger with in-memory logging enabled and no file logging.
	/// </summary>
	public Logger() : this(true, null)
	{
	}

	/// <summary>
	/// Creates a logger with a log file in the default ufex logs directory.
	/// Memory logging is disabled by default to prevent memory leaks in long-lived loggers.
	/// </summary>
	/// <param name="fileName">The name of the log file (e.g., "Desktop_MainWindow.log")</param>
	public Logger(string fileName) : this(false, GetDefaultLogFilePath(fileName))
	{
	}

	/// <summary>
	/// Creates a logger with a log file in the default ufex logs directory and optional memory logging.
	/// </summary>
	/// <param name="fileName">The name of the log file (e.g., "Desktop_MainWindow.log")</param>
	/// <param name="enableMemoryLog">Whether to enable in-memory log storage (use for short-lived loggers only)</param>
	public Logger(string fileName, bool enableMemoryLog) : this(enableMemoryLog, GetDefaultLogFilePath(fileName))
	{
	}

	/// <summary>
	/// Creates a logger with optional in-memory logging.
	/// </summary>
	/// <param name="enableMemoryLog">Whether to enable in-memory log storage</param>
	public Logger(bool enableMemoryLog) : this(enableMemoryLog, null)
	{
	}

	/// <summary>
	/// Creates a logger with the specified settings.
	/// </summary>
	/// <param name="enableMemoryLog">Whether to enable in-memory log storage</param>
	/// <param name="filePath">Full path to the log file, or null to disable file logging</param>
	public Logger(bool enableMemoryLog, string filePath)
	{
		if (enableMemoryLog)
			_entries = new ArrayList(100);
		else
			_entries = null;

		_filePath = filePath;
		_writeToFile = !string.IsNullOrEmpty(filePath);

		// Ensure log directory exists if file logging is enabled
		if (_writeToFile)
		{
			var logDir = Path.GetDirectoryName(_filePath);
			if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}
		}
	}

	/// <summary>
	/// Gets the default log file path in the user's AppData folder.
	/// </summary>
	private static string GetDefaultLogFilePath(string fileName)
	{
		var logDir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"ufex",
			"Logs"
		);
		return Path.Combine(logDir, fileName);
	}

	public IDisposable BeginScope<TState>(TState state) => null;

	public bool IsEnabled(LogLevel logLevel)
	{
		// Log everything, or filter here if you want
		return logLevel != LogLevel.None;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		if (!IsEnabled(logLevel)) return;

		var message = formatter(state, exception);
		var logRecord = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] {message}";

		if (exception != null)
		{
			logRecord += Environment.NewLine + FormatStackTrace(exception);
		}

		lock (_lock)
		{
			// Append to in-memory store
			if (_entries != null)
			{
				_entries.Add(logRecord);
			}

			// Write to file if enabled
			if (_writeToFile)
			{
				try
				{
					File.AppendAllText(_filePath, logRecord + Environment.NewLine);
				}
				catch
				{
					// Silently ignore file write errors to avoid breaking the application
				}
			}
		}
	}

	/// <summary>
	/// Logs an informational message.
	/// </summary>
	[Obsolete("Use the ILogger extension method LogInformation instead")]
	public void Info(string message, string className = "", string funcName = "", string title = "Debug Info")
	{
		// Create a legacy entry for backwards compatibility
		InfoLogEntry tmpInfo = new InfoLogEntry();
		tmpInfo.message = message;
		tmpInfo.className = className;
		tmpInfo.funcName = funcName;
		tmpInfo.title = title;

		// Store legacy entry and log via ILogger
		lock (_lock)
		{
			if (_entries != null)
			{
				_entries.Add(tmpInfo);
			}
		}

		// Build formatted message including context info
		string formattedMessage = !string.IsNullOrEmpty(className) || !string.IsNullOrEmpty(funcName)
			? $"{message} ({className}.{funcName})"
			: message;

		this.Log(LogLevel.Information, 0, formattedMessage, null, (s, e) => s);
	}

	/// <summary>
	/// Logs an error message.
	/// </summary>
	[Obsolete("Use the ILogger extension method LogError instead")]
	public void Error(string message, string className = "", string funcName = "", string title = "Error")
	{
		// Create a legacy entry for backwards compatibility
		ErrorLogEntry tmpError = new ErrorLogEntry();
		tmpError.message = message;
		tmpError.className = className;
		tmpError.funcName = funcName;
		tmpError.title = title;

		// Store legacy entry
		lock (_lock)
		{
			if (_entries != null)
			{
				_entries.Add(tmpError);
			}
		}

		// Build formatted message including context info
		string formattedMessage = !string.IsNullOrEmpty(className) || !string.IsNullOrEmpty(funcName)
			? $"{message} ({className}.{funcName})"
			: message;

		this.Log(LogLevel.Error, 0, formattedMessage, null, (s, e) => s);
	}

	/// <summary>
	/// Logs an exception with optional description and context.
	/// </summary>
	public void NewException(Exception e, string description = "", string funcName = "", string className = "")
	{
		// Create a legacy entry for backwards compatibility
		ExceptionLogEntry tmpException = new ExceptionLogEntry();
		tmpException.e = e;
		tmpException.className = className;
		tmpException.funcName = funcName;
		tmpException.description = description;

		// Store legacy entry
		lock (_lock)
		{
			if (_entries != null)
			{
				_entries.Add(tmpException);
			}
		}

		// Build formatted message including context info
		string formattedMessage = !string.IsNullOrEmpty(description)
			? $"{description} ({className}.{funcName})"
			: $"Exception in {className}.{funcName}";

		this.Log(LogLevel.Error, 0, formattedMessage, e, (s, ex) => s);
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
}
