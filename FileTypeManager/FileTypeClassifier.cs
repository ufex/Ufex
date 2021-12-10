using System;
using System.IO;

namespace UniversalFileExplorer
{
	/// <summary>
	/// Summary description for FileTypeIdentifier.
	/// </summary>
	public abstract class FileTypeClassifier
	{
		private FileTypeDb fileTypeDb;
		private static string FT_UNKNOWN = "FT_UNKNOWN";

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

		}

		public abstract string GetFileType(string filePath, FileStream fileStream);
	}
}
