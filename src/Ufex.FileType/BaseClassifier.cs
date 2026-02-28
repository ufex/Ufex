using System;
using System.IO;
using Ufex.API;

namespace Ufex.FileType;

/// <summary>
/// Base class for file type classifiers
/// </summary>
public abstract class BaseClassifier
{
	private FileTypeDb fileTypeDb;
	public Logger Log { get; private set; }

	public FileTypeDb FileTypes
	{
		get { return fileTypeDb; }
		set { fileTypeDb = value; }
	}

	public BaseClassifier()
	{
		Log = new Logger();
	}

	public BaseClassifier(Logger log)
	{
		Log = log;
	}

	/// <summary>
	/// Get all matching file types for the given file
	/// </summary>
	/// <param name="filePath">The absolute path to the file</param>
	/// <param name="fileStream">A read only FileStream</param>
	/// <returns>An array of file type ID's</returns>
	public abstract string[] DetectFileType(string filePath, FileStream fileStream);
}
