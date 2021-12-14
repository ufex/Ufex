using System;
using System.Reflection;
using System.Collections;
using System.IO;
using Ufex.API;
using System.Collections.Generic;

namespace UniversalFileExplorer
{
	public struct CachedAssembly
	{
		public string path;
		public Assembly assembly;
	}

	/// <summary>
	/// Summary description for FileTypeManager.
	/// </summary>
	public class FileTypeManager
	{
		public const string FILETYPE_UNKNOWN = "FT_UNKNOWN";

		private FileTypeClassesDb fileTypeClassesDb;
		private IDLibsDb idLibsDb;
		private ArrayList idLibsCache;
		private ArrayList assemblyCache;

		#region Properties

		public FileTypeDb FileTypes { get; protected set; }
		public string ApplicationPath { get; protected set; }
		public string[] ConfigDirectories { get; set; }
		public Logger Logger { get; protected set; }

		#endregion

		public FileTypeManager(string applicationPath, string[] configDirectories)
		{
			Logger = new Logger("FileTypeManager.log");

			ApplicationPath = string.Copy(applicationPath);
			ConfigDirectories = (string[])configDirectories.Clone();

			assemblyCache = new ArrayList();
			idLibsCache = new ArrayList();

			InitializeDatabases();
			LoadIDLibs();
		}

		public FILETYPE GetFileType(string filePath)
		{
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			Logger.Info("m_idLibsCache.Count = " + idLibsCache.Count.ToString());
			foreach(FileTypeClassifier ftid in idLibsCache)
			{
				try
				{
					fs.Position = 0;
					string fileType = ftid.GetFileType(filePath, fs);
					if(!fileType.Equals(FILETYPE_UNKNOWN))
					{
						fs.Close();
						return FileTypes.GetFileType(fileType);
					}
				}
				catch(Exception e)
				{
					Logger.NewException(e, "Failed to identify file type using " + ftid.ToString());
				}
			}
			fs.Close();
			return null;
		}

		public FILETYPE_CLASS[] GetFileTypeClassesByFileType(string fileTypeId)
		{
			return fileTypeClassesDb.GetFileTypeClassesByFileType(fileTypeId);
		}

		public Ufex.API.FileType GetNewClassInstance(string classId)
		{
			FILETYPE_CLASS fileTypeClass = fileTypeClassesDb.GetFileTypeClass(classId);

			if(fileTypeClass == null)
				return null;

			// Get the assembly
			Assembly assembly = GetAssembly(fileTypeClass.AssemblyPath);

			if(assembly == null)
				return null;

			return (Ufex.API.FileType)(assembly.CreateInstance(fileTypeClass.FullTypeName, true));
		}

		private Assembly GetAssembly(string assemblyPath)
		{
			string fullPath = ResolvePath(assemblyPath);

			if(fullPath == null)
				throw new Exception("Failed to load assembly: File Not Found");

			// Look for the assembly in the cache
			foreach(CachedAssembly cachedAssembly in assemblyCache)
			{
				if(cachedAssembly.path.ToLower().Equals(assemblyPath.ToLower()))
					return cachedAssembly.assembly;
			}

			// Load the assembly into the cache
			Assembly newAssembly = Assembly.LoadFile(fullPath);

			CachedAssembly cachedAss = new CachedAssembly();
			cachedAss.path = fullPath;
			cachedAss.assembly = newAssembly;

			assemblyCache.Add(cachedAss);
			return cachedAss.assembly;
		}

		/// <summary>
		/// Loads the File Type Identification Assemblys into memory.
		/// </summary>
		private void LoadIDLibs()
		{
			ID_LIB[] idLibs = idLibsDb.GetIDLibs();

			foreach(ID_LIB idLib in idLibs)
			{
				string path = ResolvePath(idLib.assemblyPath);
				if(path != null)
				{
					try
					{
						Assembly tempAssembly = Assembly.LoadFile(path);
						FileTypeClassifier newIDLib = (FileTypeClassifier)(tempAssembly.CreateInstance(idLib.fullTypeName, true));
						newIDLib.FileTypes = FileTypes;
						idLibsCache.Add(newIDLib);
					}
					catch(Exception e)
					{
						Logger.NewException(e, "FileTypeManager", "LoadIDLibs()", "Failed to load assembly: " + path);
					}
				}
				else
				{
					Logger.Error("Failed to find idLib: " + idLib.assemblyPath);
				}
			}
		}

		private string ResolvePath(string path)
		{
			string actualPath = path;
			
			// If the file does not exist: try to resolve the path
			if(!File.Exists(actualPath))
			{
				actualPath = Path.Combine(ApplicationPath, path);
				if(!File.Exists(actualPath))
					actualPath = Path.Combine(ApplicationPath, "modules", path);
					if (!File.Exists(actualPath))
						return null;
			}
			actualPath = Path.GetFullPath(actualPath);
			Logger.Info("ResolvePath: " + path + ", " + actualPath);
			return actualPath;
		}

		private void InitializeDatabases()
		{
			List<FileInfo> configFiles = new List<FileInfo>();
			foreach(string path in ConfigDirectories)
            {
				DirectoryInfo di = new DirectoryInfo(path);
				if (di.Exists)
				{
					configFiles.AddRange(di.GetFiles("*.xml"));
				}
			}
			FileTypes = new FileTypeDb(configFiles.ToArray());
			fileTypeClassesDb = new FileTypeClassesDb(configFiles.ToArray());
			idLibsDb = new IDLibsDb();
		}
	}
}
