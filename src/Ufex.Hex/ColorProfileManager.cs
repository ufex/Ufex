using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ufex.Hex;

/// <summary>
/// Manages loading and querying of color profiles.
/// Profiles are loaded from .ufexcolors files in a background thread on startup.
/// </summary>
public class ColorProfileManager
{
	private List<ColorProfile> profiles = new List<ColorProfile>();
	private readonly object lockObj = new object();
	private Task? loadTask;
	private List<string> loadErrors = new List<string>();

	/// <summary>
	/// Gets whether the profiles have finished loading.
	/// </summary>
	public bool IsLoaded => loadTask?.IsCompleted ?? false;

	/// <summary>
	/// Gets any errors that occurred during loading.
	/// </summary>
	public IReadOnlyList<string> LoadErrors
	{
		get
		{
			lock (lockObj)
			{
				return loadErrors.ToList();
			}
		}
	}

	/// <summary>
	/// Starts loading all .ufexcolors files from the specified directory in a background thread.
	/// </summary>
	/// <param name="colorProfilesDirectory">The directory containing .ufexcolors files.</param>
	public void LoadAsync(string colorProfilesDirectory)
	{
		loadTask = Task.Run(() => LoadProfiles(colorProfilesDirectory));
	}

	/// <summary>
	/// Loads all .ufexcolors files from the specified directory synchronously.
	/// </summary>
	/// <param name="colorProfilesDirectory">The directory containing .ufexcolors files.</param>
	public void Load(string colorProfilesDirectory)
	{
		LoadProfiles(colorProfilesDirectory);
	}

	/// <summary>
	/// Waits for the background load to complete.
	/// </summary>
	public void WaitForLoad()
	{
		loadTask?.Wait();
	}

	/// <summary>
	/// Waits for the background load to complete asynchronously.
	/// </summary>
	public async Task WaitForLoadAsync()
	{
		if (loadTask != null)
			await loadTask;
	}

	/// <summary>
	/// Returns all loaded color profiles.
	/// </summary>
	public IReadOnlyList<ColorProfile> GetAllProfiles()
	{
		lock (lockObj)
		{
			return profiles.ToList();
		}
	}

	/// <summary>
	/// Returns all color profiles that apply to the given file type ID.
	/// A profile applies if its FileTypePatterns list is empty (global)
	/// or if any pattern matches the file type ID.
	/// </summary>
	/// <param name="fileTypeId">The file type ID to match against (e.g. "TEXT_PLAIN", "PNG").</param>
	/// <returns>A list of matching profiles.</returns>
	public List<ColorProfile> GetProfilesForFileType(string fileTypeId)
	{
		lock (lockObj)
		{
			var result = new List<ColorProfile>();
			foreach (var profile in profiles)
			{
				if (profile.AppliesToFileType(fileTypeId))
					result.Add(profile);
			}
			return result;
		}
	}

	/// <summary>
	/// Returns all color profiles that apply to any of the given file type IDs.
	/// Use this overload to include ancestor type IDs in the match.
	/// </summary>
	/// <param name="fileTypeIds">The file type ID and its ancestors to match against.</param>
	/// <returns>A list of matching profiles.</returns>
	public List<ColorProfile> GetProfilesForFileType(IReadOnlyList<string> fileTypeIds)
	{
		lock (lockObj)
		{
			var result = new List<ColorProfile>();
			foreach (var profile in profiles)
			{
				if (profile.AppliesToFileType(fileTypeIds))
					result.Add(profile);
			}
			return result;
		}
	}

	/// <summary>
	/// Gets a color profile by its ID, or null if not found.
	/// </summary>
	public ColorProfile? GetProfileById(string id)
	{
		lock (lockObj)
		{
			foreach (var profile in profiles)
			{
				if (profile.ID == id)
					return profile;
			}
			return null;
		}
	}

	private void LoadProfiles(string directory)
	{
		var loadedProfiles = new List<ColorProfile>();
		var errors = new List<string>();
		var seenIds = new HashSet<string>();

		if (!Directory.Exists(directory))
		{
			lock (lockObj)
			{
				profiles = loadedProfiles;
				loadErrors = errors;
			}
			return;
		}

		string[] files = Directory.GetFiles(directory, "*.ufexcolors");

		foreach (string file in files)
		{
			try
			{
				var profile = ColorProfileParser.ParseFile(file);

				if (seenIds.Contains(profile.ID))
				{
					errors.Add($"Duplicate profile ID '{profile.ID}' in file: {file}");
					continue;
				}

				seenIds.Add(profile.ID);
				loadedProfiles.Add(profile);
			}
			catch (ColorProfileParseException ex)
			{
				errors.Add($"Error parsing '{file}': {ex.Message}");
			}
			catch (Exception ex)
			{
				errors.Add($"Error reading '{file}': {ex.Message}");
			}
		}

		lock (lockObj)
		{
			profiles = loadedProfiles;
			loadErrors = errors;
		}
	}
}
