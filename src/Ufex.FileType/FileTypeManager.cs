using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Runtime.Loader;
using Ufex.API;
using System.Collections.Concurrent;
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
	private readonly string pluginDirectory;

	// Shared cache for plugin dependency assemblies (e.g. EXIF) that may be
	// needed by multiple plugins. Loading the same DLL file into separate
	// collectible ALCs can fail when the first ALC already holds the image.
	// Note: holding Assembly refs here prevents collectible ALC unloading,
	// but that's acceptable since nothing currently unloads plugins.
	private static readonly ConcurrentDictionary<string, Assembly> sharedDependencyCache = new(StringComparer.OrdinalIgnoreCase);

	public PluginLoadContext(string pluginPath) : base(isCollectible: true)
	{
		resolver = new AssemblyDependencyResolver(pluginPath);
		hostAssembly = typeof(BaseClassifier).Assembly;
		pluginDirectory = Path.GetDirectoryName(pluginPath)!;
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

			// Return a previously-loaded shared dependency (e.g. EXIF used by both JPEG and ISOBMFF)
			if (sharedDependencyCache.TryGetValue(assemblyName.Name, out Assembly? cached))
			{
				return cached;
			}
		}

		// Try to resolve from the plugin's deps.json
		string? assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
		if (assemblyPath != null)
		{
			Assembly asm = LoadFromAssemblyPath(assemblyPath);
			if (assemblyName.Name != null)
				sharedDependencyCache.TryAdd(assemblyName.Name, asm);
			return asm;
		}

		// Fallback: look for the assembly in the plugin's directory
		// This handles project-reference dependencies that the resolver may not resolve
		if (assemblyName.Name != null)
		{
			string localPath = Path.Combine(pluginDirectory, assemblyName.Name + ".dll");
			if (File.Exists(localPath))
			{
				Assembly asm = LoadFromAssemblyPath(localPath);
				sharedDependencyCache.TryAdd(assemblyName.Name, asm);
				return asm;
			}
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

		ApplicationPath = applicationPath;
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
					return SortBySpecificity(fileTypeRecords);
				}
			}
			catch(Exception e)
			{
				Logger.Error(e, "FileTypeManager.DetectFileType: Failed to identify file type using {FileType}", ftid.ToString());
			}
		}
		fs.Close();
		return null;
	}

	public DetectionResult DetectFileTypeDetailed(string filePath)
	{
		var result = new DetectionResult();

		// Collect matches from all classifiers
		Dictionary<string, DetectionMatch> matchMap = new();

		FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		Logger.Info("DetectFileTypeDetailed: idLibsCache.Count = " + idLibsCache.Count.ToString());

		foreach (BaseClassifier classifier in idLibsCache)
		{
			try
			{
				fs.Position = 0;
				DetectionMatch[] classifierMatches = classifier.DetectFileTypeDetailed(filePath, fs);

				foreach (var match in classifierMatches)
				{
					if (match.FileType == null)
						continue;

					if (matchMap.TryGetValue(match.FileType.ID, out var existing))
					{
						// Merge: combine MatchMethod flags and details
						existing.Method |= match.Method;
						if (match.MatchedExtension != null)
							existing.MatchedExtension = match.MatchedExtension;
						existing.SignatureDetails.AddRange(match.SignatureDetails);
					}
					else
					{
						matchMap[match.FileType.ID] = match;
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, "FileTypeManager.DetectFileTypeDetailed: classifier failed: {Classifier}",
					classifier.ToString());
			}
		}

		fs.Close();

		// Convert to array, look up FileTypeRecords, sort by specificity
		var records = matchMap.Values
			.Where(m => m.FileType != null)
			.ToList();

		// Sort by specificity (reuse existing logic)
		var recordArray = records.Select(m => m.FileType).ToArray();
		SortBySpecificity(recordArray);

		// Rebuild list in sorted order
		result.Matches = recordArray
			.Select(ft => matchMap[ft.ID])
			.ToList();

		return result;
	}

	/// <summary>
	/// Sorts file type records so that child types appear before their parents.
	/// For example, WAVE (child of RIFF) will be sorted before RIFF since it's more specific.
	/// </summary>
	private FileTypeRecord[] SortBySpecificity(FileTypeRecord[] records)
	{
		if (records == null || records.Length <= 1)
			return records;

		var recordIds = new HashSet<string>(records.Select(r => r.ID));

		Array.Sort(records, (a, b) =>
		{
			// If a is a child of b, a is more specific and should come first
			if (!string.IsNullOrEmpty(a.ParentID) && a.ParentID == b.ID)
				return -1;

			// If b is a child of a, b is more specific and should come first
			if (!string.IsNullOrEmpty(b.ParentID) && b.ParentID == a.ID)
				return 1;

			// For indirect relationships, count ancestry depth among the detected types:
			// deeper (more specific) types come first
			int depthA = GetAncestryDepth(a, recordIds);
			int depthB = GetAncestryDepth(b, recordIds);
			if (depthA != depthB)
				return depthB.CompareTo(depthA);

			return 0;
		});

		return records;
	}

	/// <summary>
	/// Counts how many ancestors of this record are present in the detected set.
	/// A higher depth means the type is more specific.
	/// </summary>
	private int GetAncestryDepth(FileTypeRecord record, HashSet<string> detectedIds)
	{
		int depth = 0;
		string currentId = record.ParentID;

		while (!string.IsNullOrEmpty(currentId) && detectedIds.Contains(currentId))
		{
			depth++;
			FileTypeRecord parent = FileTypes.FileTypesByID.TryGetValue(currentId, out var p) ? p : null;
			currentId = parent?.ParentID;
		}

		return depth;
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
				Logger.Error(e, "FileTypeManager.LoadIDLibs: Failed to load classifier: {FileType}", idLib.fullTypeName);
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
