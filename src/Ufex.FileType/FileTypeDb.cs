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
	public List<Ufex.FileType.Config.Signature> Signatures { get; set; }
}


/// <summary>
/// Summary description for FileTypeDb.
/// </summary>
public class FileTypeDb : FileType.Database
{
	private Dictionary<string, FileTypeRecord> fileTypes;
	private Dictionary<string, RuleDefinition> ruleDefinitions;

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

	public Dictionary<string, RuleDefinition> RuleDefinitions
	{
		get
		{
			if (fileTypes == null || fileTypes.Count == 0)
				Load();
			return ruleDefinitions;
		}
	}

	public FileTypeDb(System.IO.FileInfo[] configFiles) : base(configFiles)
	{
		fileTypes = new Dictionary<string, FileTypeRecord>();
		ruleDefinitions = new Dictionary<string, RuleDefinition>();
		Debug = new Logger("FileTypeDb");
	}

	private void Load()
	{
		fileTypes = new Dictionary<string, FileTypeRecord>();
		ruleDefinitions = new Dictionary<string, RuleDefinition>();

		XmlSerializer xmlSerializer;
		try
		{
			xmlSerializer = new XmlSerializer(typeof(Document));
		}
		catch (Exception ex)
		{
			Debug.Warning("FileTypeDb.Load: Default XmlSerializer failed, retrying with explicit known types. {Error}", GetExceptionChain(ex));
			xmlSerializer = new XmlSerializer(typeof(Document), new Type[]
			{
				typeof(Signature),
				typeof(Rule),
				typeof(SearchRule),
				typeof(RuleGroup),
				typeof(RuleRef),
				typeof(RuleDefinition),
			});
		}

		foreach (System.IO.FileInfo filePath in configFiles)
		{
			using StreamReader reader = new StreamReader(filePath.FullName);
			Document doc;
			try
			{
				doc = (Document)xmlSerializer.Deserialize(reader);
			}
			catch (Exception ex)
			{
				Debug.Error(ex, "FileTypeDb.Load: Failed to deserialize config file: {ConfigFilePath} | {Error}", filePath.FullName, GetExceptionChain(ex));
				continue;
			}

			if (doc.RuleDefinitions != null)
			{
				foreach (RuleDefinition ruleDefinition in doc.RuleDefinitions)
				{
					if (String.IsNullOrWhiteSpace(ruleDefinition.Name))
					{
						Debug.Warning("FileTypeDb.Load: RuleDefinition with empty name in config file: {ConfigFilePath}", filePath.FullName);
						continue;
					}

					if (ruleDefinitions.ContainsKey(ruleDefinition.Name))
					{
						Debug.Warning("FileTypeDb.Load: Duplicate RuleDefinition '{RuleDefinitionName}' in config file: {ConfigFilePath}", ruleDefinition.Name, filePath.FullName);
						continue;
					}

					ruleDefinitions[ruleDefinition.Name] = ruleDefinition;
				}
			}

			if (doc.FileTypes != null)
			{
				foreach (FileTypeRecord fileType in doc.FileTypes)
				{
					fileType.ConfigFilePath = filePath.FullName;
					fileTypes[fileType.ID] = fileType;
				}
			}
		}
	}

	private static string GetExceptionChain(Exception ex)
	{
		List<string> messages = new List<string>();
		Exception current = ex;
		while (current != null)
		{
			messages.Add(current.Message);
			current = current.InnerException;
		}
		return string.Join(" => ", messages);
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
