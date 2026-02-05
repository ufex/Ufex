using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Runtime.Loader;
using Ufex.API;
using System.Collections.Generic;

namespace Ufex.FileType;

public struct CachedAssembly
{
	public string path;
	public Assembly assembly;
}

/// <summary>
/// Custom AssemblyLoadContext that shares types from the host assembly
/// to ensure plugins can inherit from BaseClassifier correctly.
/// </summary>
public class PluginLoadContext : AssemblyLoadContext
{
	private readonly AssemblyDependencyResolver resolver;
	private readonly Assembly hostAssembly;

	public PluginLoadContext(string pluginPath) : base(isCollectible: true)
	{
		resolver = new AssemblyDependencyResolver(pluginPath);
		hostAssembly = typeof(BaseClassifier).Assembly;
	}

	protected override Assembly? Load(AssemblyName assemblyName)
	{
		// If the plugin needs Ufex.FileType or Ufex.API, return the host's already-loaded assembly
		// This ensures type identity is preserved for BaseClassifier and other shared types
		if (assemblyName.Name != null)
		{
			if (assemblyName.Name.Equals("Ufex.FileType", StringComparison.OrdinalIgnoreCase))
			{
				return typeof(BaseClassifier).Assembly;
			}
			if (assemblyName.Name.Equals("Ufex.API", StringComparison.OrdinalIgnoreCase))
			{
				return typeof(Logger).Assembly;
			}
		}

		// Try to resolve from the plugin's directory
		string? assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
		if (assemblyPath != null)
		{
			return LoadFromAssemblyPath(assemblyPath);
		}

		// Fall back to default context
		return null;
	}
}

/// <summary>
/// Summary description for FileTypeManager.
/// </summary>
public class FileTypeManager
{
	public const string FileTypeRecord_UNKNOWN = "FT_UNKNOWN";

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
		Logger = new Logger("FileType.log");

		ApplicationPath = string.Copy(applicationPath);
		ConfigDirectories = (string[])configDirectories.Clone();

		assemblyCache = new ArrayList();
		idLibsCache = new ArrayList();

		InitializeDatabases();
		LoadIDLibs();
	}

	public FileTypeRecord[] DetectFileType(string filePath)
	{
		FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		Logger.Info("m_idLibsCache.Count = " + idLibsCache.Count.ToString());
		foreach(BaseClassifier ftid in idLibsCache)
		{
			try
			{
				fs.Position = 0;
				string[] fileTypes = ftid.DetectFileType(filePath, fs);
				// TODO: should we exit early if we get a match or iterate over all the classifiers?
				if(fileTypes.Length > 0)
				{
					fs.Close();
					FileTypeRecord[] fileTypeRecords = new FileTypeRecord[fileTypes.Length];
					for(int i = 0; i < fileTypes.Length; i++)
					{
						fileTypeRecords[i] = FileTypes.GetFileType(fileTypes[i]);
					}
					return fileTypeRecords;
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

	public FileTypeClassRecord[] GetFileTypeClassesByFileType(string fileTypeId)
	{
		return fileTypeClassesDb.GetFileTypeClassesByFileType(fileTypeId);
	}

	public Ufex.API.FileType GetNewClassInstance(string classId)
	{
		FileTypeClassRecord fileTypeClass = fileTypeClassesDb.GetFileTypeClass(classId);

		if(fileTypeClass == null)
			return null;

		// Get the assembly
		Logger.Info("Loading assembly for FileTypeClass: " + fileTypeClass.ID + ", " + fileTypeClass.AssemblyPath);
		Assembly assembly = GetAssembly("plugins/" + fileTypeClass.AssemblyPath);

		if(assembly == null)
			return null;

		return (Ufex.API.FileType)assembly.CreateInstance(fileTypeClass.FullTypeName, true);
	}

	private Assembly GetAssembly(string assemblyPath)
	{
		string? fullPath = ResolvePath(assemblyPath);

		if (fullPath == null)
			throw new Exception("Failed to load assembly: File Not Found");

		// Look for the assembly in the cache
		foreach (CachedAssembly cachedAssembly in assemblyCache)
		{
			if (cachedAssembly.path.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
				return cachedAssembly.assembly;
		}

		// Check if this is the current assembly
		Assembly currentAssembly = typeof(BaseClassifier).Assembly;
		string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
		if (currentAssembly.GetName().Name!.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
		{
			return currentAssembly;
		}

		// Load the plugin assembly using PluginLoadContext to ensure shared types work correctly
		var loadContext = new PluginLoadContext(fullPath);
		Assembly newAssembly = loadContext.LoadFromAssemblyPath(fullPath);

		CachedAssembly cachedAss = new CachedAssembly();
		cachedAss.path = fullPath;
		cachedAss.assembly = newAssembly;

		assemblyCache.Add(cachedAss);
		return cachedAss.assembly;
	}

	/// <summary>
	/// Loads the File Type Identification Assemblies into memory.
	/// </summary>
	private void LoadIDLibs()
	{
		ID_LIB[] idLibs = idLibsDb.GetIDLibs();
		Assembly currentAssembly = typeof(BaseClassifier).Assembly;

		foreach (ID_LIB idLib in idLibs)
		{
			try
			{
				Assembly targetAssembly;

				// Check if this is the current assembly - if so, use the already-loaded one
				// to avoid type identity issues from loading a duplicate
				string assemblyName = Path.GetFileNameWithoutExtension(idLib.assemblyPath);
				if (currentAssembly.GetName().Name!.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
				{
					targetAssembly = currentAssembly;
				}
				else
				{
					string? path = ResolvePath(idLib.assemblyPath);
					if (path == null)
					{
						Logger.Error("Failed to find idLib: " + idLib.assemblyPath);
						continue;
					}
					// Use PluginLoadContext for external assemblies to share types properly
					var loadContext = new PluginLoadContext(path);
					targetAssembly = loadContext.LoadFromAssemblyPath(path);
				}

				object[] args = new object[] { Logger };
				BaseClassifier? newIDLib = (BaseClassifier?)targetAssembly.CreateInstance(
					idLib.fullTypeName, true, BindingFlags.Default, null, args, null, null);

				if (newIDLib != null)
				{
					newIDLib.FileTypes = FileTypes;
					idLibsCache.Add(newIDLib);
				}
				else
				{
					Logger.Error("Failed to create instance of: " + idLib.fullTypeName);
				}
			}
			catch (Exception e)
			{
				Logger.NewException(e, "FileTypeManager", "LoadIDLibs()", "Failed to load classifier: " + idLib.fullTypeName);
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
		List<System.IO.FileInfo> configFiles = new List<System.IO.FileInfo>();
		foreach(string path in ConfigDirectories)
		{
			Logger.Info("Loading config files from: " + path);
			DirectoryInfo di = new DirectoryInfo(path);
			if (di.Exists)
			{
				Logger.Info("Directory exists: " + path);
				configFiles.AddRange(di.GetFiles("*.xml"));
			}
		}
		FileTypes = new FileTypeDb(configFiles.ToArray());
		fileTypeClassesDb = new FileTypeClassesDb(configFiles.ToArray());
		idLibsDb = new IDLibsDb();
	}
}
