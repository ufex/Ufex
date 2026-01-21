using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ufex.FileType.Config;

namespace Ufex.FileType
{
	public class FILETYPE_CLASS
	{
		public string ConfigFilePath;
		public string ID;
		public string AssemblyPath;
		public string FullTypeName;
		[XmlArray]
		[XmlArrayItem(ElementName = "ID")]
		public string[] FileTypes = new string[0];
	}

	/// <summary>
	/// File Type Classes Database
	/// </summary>
	public class FileTypeClassesDb : Ufex.FileType.Database
	{
		Dictionary<string, FILETYPE_CLASS> fileTypeClasses;

		public int Count
		{
			get { return FileTypeClasses.Length; }
		}

		public FILETYPE_CLASS[] FileTypeClasses
		{
			get { return FileTypeClassesByID.Values.ToArray(); }
		}

		public Dictionary<string, FILETYPE_CLASS> FileTypeClassesByID
		{
			get
			{
				if (fileTypeClasses == null || fileTypeClasses.Count == 0)
					Load();
				return fileTypeClasses;
			}
		}

		public FileTypeClassesDb(FileInfo[] configFiles) : base(configFiles)
		{
			this.fileTypeClasses = new Dictionary<string, FILETYPE_CLASS>();
		}

		private void Load()
  	{
			fileTypeClasses = new Dictionary<string, FILETYPE_CLASS>();
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Document));
			foreach (FileInfo filePath in configFiles)
			{
				StreamReader reader = new StreamReader(filePath.FullName);
				Document doc = (Document)xmlSerializer.Deserialize(reader);
				if (doc.FileTypeClasses != null)
				{
					foreach (FILETYPE_CLASS fileTypeClass in doc.FileTypeClasses)
					{
						fileTypeClass.ConfigFilePath = filePath.FullName;
						fileTypeClasses[fileTypeClass.ID] = fileTypeClass;
					}
				}
				reader.Close();
			}
		}

		public FILETYPE_CLASS[] GetFileTypeClasses(bool refresh = false)
		{
			if (refresh)
				Load();

			return FileTypeClasses;
		}

		public FILETYPE_CLASS[] GetFileTypeClassesByFileType(string fileTypeId)
		{
			List<FILETYPE_CLASS> results = new List<FILETYPE_CLASS>();
			foreach(FILETYPE_CLASS fileTypeClass in FileTypeClasses)
			{
				if (Array.Exists(fileTypeClass.FileTypes, x => x == fileTypeId))
				{
					results.Add(fileTypeClass);
				}
			}
			return results.ToArray();
		}

		public FILETYPE_CLASS GetFileTypeClass(string classId)
		{
			return FileTypeClassesByID[classId];
		}

		public bool Exists(string classId)
		{
			return FileTypeClassesByID.ContainsKey(classId);
		}
	}
}
