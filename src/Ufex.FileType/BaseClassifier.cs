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
	protected static string FT_UNKNOWN = "FT_UNKNOWN";

	public FileTypeDb FileTypes
	{
		get { return fileTypeDb; }
		set { fileTypeDb = value; }
	}

	public string FileTypeUnknown
	{
		get { return FT_UNKNOWN; }
	}

	public BaseClassifier()
	{
		Log = new Logger();
	}

	public BaseClassifier(Logger log)
	{
		Log = log;
	}

	public abstract string GetFileType(string filePath, FileStream fileStream);
}
