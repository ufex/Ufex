using System;
using System.IO;
using Ufex.API;

namespace UniversalFileExplorer
{
	/// <summary>
	/// Summary description for FileTypeIdentifier.
	/// </summary>
	public abstract class FileTypeClassifier
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

		public FileTypeClassifier()
		{
			Log = new Logger();
		}

		public FileTypeClassifier(Logger log)
        {
			Log = log;
        }

		public abstract string GetFileType(string filePath, FileStream fileStream);
	}
}
