using System;
using System.Reflection;
using System.Collections;
using System.IO;
using Ufex.API;

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
	public class FileTypeManager : IDisposable
	{

		public const string FILETYPE_UNKNOWN = "FT_UNKNOWN";

		private string m_appPath;
		
		private FileTypeDb m_fileTypeDb;
		private FileTypeClassesDb m_fileTypeClassesDb;
		private IDLibsDb m_idLibsDb;

		private ArrayList m_idLibsCache;

		private ArrayList m_assemblyCache;

		private Logger m_debug;

		#region Properties

		public FileTypeDb FileTypes
		{
			get { return m_fileTypeDb; }
		}


		#endregion



		public FileTypeManager(string applicationPath)
		{
			m_debug = new Logger("FileTypeManager.log");

			m_appPath = String.Copy(applicationPath);

			m_assemblyCache = new ArrayList();
			m_idLibsCache = new ArrayList();

			InitializeDatabases();
			LoadIDLibs();
		}

		public void Dispose() 
		{
			Dispose(true);
			GC.SuppressFinalize(this); 
		}
		
		protected virtual void Dispose(bool disposing) 
		{
		}

		public FILETYPE GetFileType(string filePath)
		{
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			m_debug.Info("m_idLibsCache.Count = " + m_idLibsCache.Count.ToString());
			foreach(FileTypeClassifier ftid in m_idLibsCache)
			{
				try
				{
					fs.Position = 0;
					string fileType = ftid.GetFileType(filePath, fs);
					if(!fileType.Equals(FILETYPE_UNKNOWN))
					{
						fs.Close();
						return m_fileTypeDb.GetFileType(fileType);
					}
				}
				catch(Exception e)
				{
					m_debug.NewException(e, "Failed to identify file type using " + ftid.ToString());
				}
			}
			fs.Close();
			return null;
		}

		public FILETYPE_CLASS[] GetFileTypeClassesByFileType(string fileTypeId)
        {
			return m_fileTypeClassesDb.GetFileTypeClassesByFileType(fileTypeId);
        }

		public Ufex.API.FileType GetNewClassInstance(string classId)
		{
			FILETYPE_CLASS fileTypeClass = m_fileTypeClassesDb.GetFileTypeClass(classId);

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
			foreach(CachedAssembly cachedAssembly in m_assemblyCache)
			{
				if(cachedAssembly.path.ToLower().Equals(assemblyPath.ToLower()))
					return cachedAssembly.assembly;
			}

			// Load the assembly into the cache
			Assembly newAssembly = Assembly.LoadFile(fullPath);

			CachedAssembly cachedAss = new CachedAssembly();
			cachedAss.path = fullPath;
			cachedAss.assembly = newAssembly;

			m_assemblyCache.Add(cachedAss);
			return cachedAss.assembly;
		}

		/// <summary>
		/// Loads the File Type Identification Assemblys into memory.
		/// </summary>
		private void LoadIDLibs()
		{
			ID_LIB[] idLibs = m_idLibsDb.GetIDLibs();

			foreach(ID_LIB idLib in idLibs)
			{
				string path = ResolvePath(idLib.assemblyPath);
				if(path != null)
				{
					try
					{
						Assembly tempAssembly = Assembly.LoadFile(path);
						FileTypeClassifier newIDLib = (FileTypeClassifier)(tempAssembly.CreateInstance(idLib.fullTypeName, true));
						newIDLib.FileTypes = m_fileTypeDb;
						m_idLibsCache.Add(newIDLib);
					}
					catch(Exception e)
					{
						m_debug.NewException(e, "FileTypeManager", "LoadIDLibs()", "Failed to load assembly: " + path);
					}
				}
				else
				{
					m_debug.Error("Failed to find idLib: " + idLib.assemblyPath);
				}
			}
		}

		private string ResolvePath(string path)
		{
			string actualPath = path;
			
			// If the file does not exist: try to resolve the path
			if(!File.Exists(actualPath))
			{
				if(actualPath.StartsWith("\\"))
					actualPath = m_appPath + actualPath;
				else
					actualPath = m_appPath + "\\" + actualPath;

				if(!File.Exists(actualPath))
					return null;

			}
			actualPath = Path.GetFullPath(actualPath);
			m_debug.Info("ResolvePath: " + path + ", " + actualPath);
			return actualPath;
		}

		private void InitializeDatabases()
		{
			DirectoryInfo di = new DirectoryInfo("D:\\code\\ufex\\ufex\\config"); // TODO
			m_fileTypeDb = new FileTypeDb(di.GetFiles("*.xml"));
			m_fileTypeClassesDb = new FileTypeClassesDb(di.GetFiles("*.xml"));
			m_idLibsDb = new IDLibsDb();
		}

	}
}
