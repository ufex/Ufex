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

	// Short convenience methods because "LogInformation" is a mouthful
	public void Info(string message) => this.Log(LogLevel.Information, 0, message, null, (s, e) => s);
	public void Warn(string message) => this.Log(LogLevel.Warning, 0, message, null, (s, e) => s);
	public void Error(string message) => this.Log(LogLevel.Error, 0, message, null, (s, e) => s);
	public void Debug(string message) => this.Log(LogLevel.Debug, 0, message, null, (s, e) => s);
	public void Trace(string message) => this.Log(LogLevel.Trace, 0, message, null, (s, e) => s);
	public void Fatal(string message) => this.Log(LogLevel.Critical, 0, message, null, (s, e) => s);
	public void Error(string message, Exception ex) => this.Log(LogLevel.Error, 0, message, ex, (s, e) => s);
	public void Fatal(string message, Exception ex) => this.Log(LogLevel.Critical, 0, message, ex, (s, e) => s);

	/// <summary>
	/// Formats and writes a debug log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Debug(0, exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Debug(EventId eventId, Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Debug, eventId, exception, message, args);
	}

	/// <summary>
	/// Formats and writes a debug log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Debug(0, "Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Debug(EventId eventId, string? message, params object?[] args)
	{
		this.Log(LogLevel.Debug, eventId, message, args);
	}

	/// <summary>
	/// Formats and writes a debug log message.
	/// </summary>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Debug(exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Debug(Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Debug, exception, message, args);
	}

	/// <summary>
	/// Formats and writes a debug log message.
	/// </summary>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Debug("Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Debug(string? message, params object?[] args)
	{
		this.Log(LogLevel.Debug, message, args);
	}

	//------------------------------------------TRACE------------------------------------------//

	/// <summary>
	/// Formats and writes a trace log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Trace(0, exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Trace(EventId eventId, Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Trace, eventId, exception, message, args);
	}

	/// <summary>
	/// Formats and writes a trace log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Trace(0, "Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Trace(EventId eventId, string? message, params object?[] args)
	{
		this.Log(LogLevel.Trace, eventId, message, args);
	}

	/// <summary>
	/// Formats and writes a trace log message.
	/// </summary>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Trace(exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Trace(Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Trace, exception, message, args);
	}

	/// <summary>
	/// Formats and writes a trace log message.
	/// </summary>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Trace("Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Trace(string? message, params object?[] args)
	{
		this.Log(LogLevel.Trace, message, args);
	}

	//------------------------------------------INFORMATION------------------------------------------//

	/// <summary>
	/// Formats and writes an informational log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Info(0, exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Info(EventId eventId, Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Information, eventId, exception, message, args);
	}

	/// <summary>
	/// Formats and writes an informational log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Info(0, "Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Info(EventId eventId, string? message, params object?[] args)
	{
		this.Log(LogLevel.Information, eventId, message, args);
	}

	/// <summary>
	/// Formats and writes an informational log message.
	/// </summary>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Info(exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Info(Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Information, exception, message, args);
	}

	/// <summary>
	/// Formats and writes an informational log message.
	/// </summary>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Info("Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Info(string? message, params object?[] args)
	{
		this.Log(LogLevel.Information, message, args);
	}

	//------------------------------------------WARNING------------------------------------------//

	/// <summary>
	/// Formats and writes a warning log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Warning(0, exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Warning(EventId eventId, Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Warning, eventId, exception, message, args);
	}

	/// <summary>
	/// Formats and writes a warning log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Warning(0, "Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Warning(EventId eventId, string? message, params object?[] args)
	{
		this.Log(LogLevel.Warning, eventId, message, args);
	}

	/// <summary>
	/// Formats and writes a warning log message.
	/// </summary>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Warning(exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Warning(Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Warning, exception, message, args);
	}

	/// <summary>
	/// Formats and writes a warning log message.
	/// </summary>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Warning("Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Warning(string? message, params object?[] args)
	{
		this.Log(LogLevel.Warning, message, args);
	}

	//------------------------------------------ERROR------------------------------------------//

	/// <summary>
	/// Formats and writes an error log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Error(0, exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Error(EventId eventId, Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Error, eventId, exception, message, args);
	}

	/// <summary>
	/// Formats and writes an error log message.
	/// </summary>
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Error(0, "Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Error(EventId eventId, string? message, params object?[] args)
	{
		this.Log(LogLevel.Error, eventId, message, args);
	}

	/// <summary>
	/// Formats and writes an error log message.
	/// </summary>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Error(exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Error(Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Error, exception, message, args);
	}

	/// <summary>
	/// Formats and writes an error log message.
	/// </summary>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Error("Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Error(string? message, params object?[] args)
	{
		this.Log(LogLevel.Error, message, args);
	}

	//------------------------------------------CRITICAL------------------------------------------//

	/// <summary>
	/// Formats and writes a critical log message.
	/// </summary>
	
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Critical(0, exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Critical(EventId eventId, Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Critical, eventId, exception, message, args);
	}

	/// <summary>
	/// Formats and writes a critical log message.
	/// </summary>
	
	/// <param name="eventId">The event id associated with the log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Critical(0, "Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Critical(EventId eventId, string? message, params object?[] args)
	{
		this.Log(LogLevel.Critical, eventId, message, args);
	}

	/// <summary>
	/// Formats and writes a critical log message.
	/// </summary>
	/// <param name="exception">The exception to log.</param>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Critical(exception, "Error while processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Critical(Exception? exception, string? message, params object?[] args)
	{
		this.Log(LogLevel.Critical, exception, message, args);
	}

	/// <summary>
	/// Formats and writes a critical log message.
	/// </summary>
	/// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	/// <example>
	/// <code language="csharp">
	/// logger.Critical("Processing request from {Address}", address)
	/// </code>
	/// </example>
	public void Critical(string? message, params object?[] args)
	{
		this.Log(LogLevel.Critical, message, args);
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
