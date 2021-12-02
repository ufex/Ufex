using System;
using System.Data.OleDb;
using System.Reflection;
using System.Collections;
using System.IO;

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

		/// <summary>
		/// The database connection.
		/// </summary>
		private OleDbConnection m_dbConn;

		private string m_dbConnectionString;
		
		private FileTypeDb m_fileTypeDb;
		private FileTypeClassesDb m_fileTypeClassesDb;
		private IDLibsDb m_idLibsDb;

		private ArrayList m_idLibsCache;

		private ArrayList m_assemblyCache;

		private UFEDebug m_debug;

		#region Properties

		public FileTypeDb FileTypes
		{
			get { return m_fileTypeDb; }
		}


		#endregion



		public FileTypeManager(string applicationPath)
		{
			m_debug = new UFEDebug("FileTypeManager.log");

			m_appPath = String.Copy(applicationPath);

			m_assemblyCache = new ArrayList();
			m_idLibsCache = new ArrayList();

			// Get the database settings
			m_dbConnectionString = Settings.GetSetting("Database", "ConnectionString");

			OpenDatabaseConnection();
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
			if (disposing) 
			{			
				if(m_dbConn != null)	
				{
					try
					{
						m_dbConn.Close();
					}
					catch(Exception e)
					{
						m_debug.NewException(e, "Failed to close connection to database");
					}
				}
			}
		}

		public FILETYPE GetFileType(string filePath)
		{
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			m_debug.NewInfo("m_idLibsCache.Count = " + m_idLibsCache.Count.ToString());
			foreach(FileTypeIdentifier ftid in m_idLibsCache)
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

		public Ufex.API.FileType GetNewClassInstance(string classId)
		{
			FILETYPE_CLASS fileTypeClass = m_fileTypeClassesDb.GetFileTypeClass(classId);

			if(fileTypeClass == null)
				return null;

			// Get the assembly
			Assembly assembly = GetAssembly(fileTypeClass.assemblyPath);

			if(assembly == null)
				return null;

			return (Ufex.API.FileType)(assembly.CreateInstance(fileTypeClass.fullTypeName, true));
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
						FileTypeIdentifier newIDLib = (FileTypeIdentifier)(tempAssembly.CreateInstance(idLib.fullTypeName, true));
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
					m_debug.NewError("Failed to find idLib: " + idLib.assemblyPath);
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
			m_debug.NewInfo("ResolvePath: " + path + ", " + actualPath);
			return actualPath;
		}

		private void OpenDatabaseConnection()
		{
			m_dbConn = new OleDbConnection(m_dbConnectionString);
			m_dbConn.Open();
		}

		private void InitializeDatabases()
		{
			m_fileTypeDb = new FileTypeDb(m_dbConn);
			m_fileTypeClassesDb = new FileTypeClassesDb(m_dbConn);
			m_idLibsDb = new IDLibsDb(m_dbConn);
		}

	}
}
