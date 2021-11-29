using System;
using System.IO;

namespace UniversalFileExplorer
{
	/// <summary>
	/// Summary description for FileTypeIdentifier.
	/// </summary>
	public abstract class FileTypeIdentifier
	{
		private FileTypeDb m_fileTypeDb;
		private static string FT_UNKNOWN = "FT_UNKNOWN";

		public FileTypeDb FileTypes
		{
			get { return m_fileTypeDb; }
			set { m_fileTypeDb = value; }
		}

		public string FileTypeUnknown
		{
			get { return FT_UNKNOWN; }
		}

		public FileTypeIdentifier()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public abstract string GetFileType(string filePath, FileStream fileStream);
	}
}
