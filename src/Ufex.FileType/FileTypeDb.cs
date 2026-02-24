using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ufex.API;
using Ufex.FileType.Config;

namespace Ufex.FileType;

public class FileTypeRecord
{
	public string ConfigFilePath;
	public string ID;
	public string ParentID;
	public string Description;
	public string Category;
	public string SubCategory;
	public string SpecificationURL;
	public string Wikipedia;

	[XmlArray]
	[XmlArrayItem(ElementName = "MIMEType")]
	public string[] MIMETypes { get; set; } = new string[0];

	[XmlArray]
	[XmlArrayItem(ElementName = "Extension")]
	public string[] Extensions { get; set; } = new string[0];

	[XmlArray]
	[XmlArrayItem(ElementName = "Signature")]
	public List<Ufex.FileType.Config.SignaturePattern[]> Signatures { get; set; }
}


/// <summary>
/// Summary description for FileTypeDb.
/// </summary>
public class FileTypeDb : FileType.Database
{
	private Dictionary<string, FileTypeRecord> fileTypes;

	public int Count
	{
		get 
		{
			return FileTypesByID.Count; 
		}
	}

	public FileTypeRecord[] FileTypes
	{
		get { return FileTypesByID.Values.ToArray(); }
	}

	public Dictionary<string, FileTypeRecord> FileTypesByID
	{
		get 
		{
			if (fileTypes == null || fileTypes.Count == 0)
				Load();
			return fileTypes; 
		}
	}

	public FileTypeDb(System.IO.FileInfo[] configFiles) : base(configFiles)
	{
		fileTypes = new Dictionary<string, FileTypeRecord>();
		Debug = new Logger("FileTypeDb");
	}

	private void Load()
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(Document));
		foreach (System.IO.FileInfo filePath in configFiles)
		{
			StreamReader reader = new StreamReader(filePath.FullName);
			Document doc = (Document)xmlSerializer.Deserialize(reader);
			if (doc.FileTypes != null)
			{
				foreach (FileTypeRecord fileType in doc.FileTypes)
				{
					fileType.ConfigFilePath = filePath.FullName;
					fileTypes[fileType.ID] = fileType;
				}
			}
			reader.Close();
		}
	}

	public FileTypeRecord GetFileType(string fileTypeId)
	{
		return FileTypesByID[fileTypeId];
	}

	public FileTypeRecord[] GetFileTypes(bool refresh = false)
	{
		if (refresh)
			Load();

		return FileTypes;
	}
	
	/// <summary>
	/// Gets all file types with the specified extension.
	/// </summary>
	/// <param name="extension">The extension to search for</param>
	/// <returns>An array of fileTypes with the specified extension.</returns>
	public FileTypeRecord[] GetFileTypesByExtension(string extension)
	{
		if(extension == null)
			throw new NullReferenceException("extension cannot be null");

		List<FileTypeRecord> fileTypes = new List<FileTypeRecord> { };
		foreach(FileTypeRecord fileType in FileTypes)
		{
			if(Array.Exists(fileType.Extensions, x => x == extension))
			{
				fileTypes.Add(fileType);
			}
		}

		return fileTypes.ToArray();
	}

	public bool AddFileType(FileTypeRecord fileType)
	{
		fileTypes[fileType.ID] = fileType;
		// TODO
		return true;
	}

	public bool SetFileType(string fileTypeId, FileTypeRecord fileType)
	{
		return false;
	}

	/// <summary>
	/// Removes the file type from the database.
	/// </summary>
	/// <param name="fileTypeId">The file types id.</param>
	/// <returns>Returns true if the file type was removed, otherwise it returns false.</returns>
	public bool RemoveType(string fileTypeId)
	{
		if(fileTypeId == null)
			throw new NullReferenceException("FileTypeID cannot be null");

		if(fileTypeId.IndexOf("*") != -1)
			throw new Exception("Invalid FileTypeID");

		return false;
	}
}
