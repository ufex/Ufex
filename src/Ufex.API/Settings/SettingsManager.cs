using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Ufex.API.Settings;

/// <summary>
/// Manages loading and saving settings to JSON files in the ApplicationData folder.
/// </summary>
public static class SettingsManager
{
	private static readonly string AppDataPath;
	private static readonly JsonSerializerOptions JsonOptions;

	static SettingsManager()
	{
		AppDataPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"ufex"
		);

		JsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
	}

	/// <summary>
	/// Gets the full path to the settings directory.
	/// </summary>
	public static string SettingsDirectory => AppDataPath;

	/// <summary>
	/// Gets the full path for a settings file.
	/// </summary>
	/// <param name="fileName">The settings file name (e.g., "settings.json" or "Ufex.FileTypes.ZIP.json")</param>
	public static string GetSettingsFilePath(string fileName)
	{
		return Path.Combine(AppDataPath, fileName);
	}

	/// <summary>
	/// Gets the shared JsonSerializerOptions (for use by source-generated contexts).
	/// </summary>
	public static JsonSerializerOptions SharedJsonOptions => JsonOptions;

	/// <summary>
	/// Loads settings from a JSON file using a source-generated JsonTypeInfo (trim-safe).
	/// </summary>
	public static T Load<T>(string fileName, JsonTypeInfo<T> typeInfo) where T : new()
	{
		var filePath = GetSettingsFilePath(fileName);

		if (!File.Exists(filePath))
		{
			return new T();
		}

		try
		{
			var json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize(json, typeInfo) ?? new T();
		}
		catch (Exception)
		{
			return new T();
		}
	}

	/// <summary>
	/// Saves settings to a JSON file using a source-generated JsonTypeInfo (trim-safe).
	/// </summary>
	public static void Save<T>(T settings, string fileName, JsonTypeInfo<T> typeInfo)
	{
		var filePath = GetSettingsFilePath(fileName);

		Directory.CreateDirectory(AppDataPath);

		var json = JsonSerializer.Serialize(settings, typeInfo);
		File.WriteAllText(filePath, json);
	}

	/// <summary>
	/// Loads settings from a JSON file. Returns a new instance if file doesn't exist.
	/// Note: This reflection-based overload may fail in trimmed/AOT builds. Prefer
	/// the overload accepting JsonTypeInfo&lt;T&gt; for trim-safe serialization.
	/// </summary>
	/// <typeparam name="T">The settings type to deserialize</typeparam>
	/// <param name="fileName">The settings file name</param>
	/// <returns>The loaded settings or a new default instance</returns>
	public static T Load<T>(string fileName) where T : new()
	{
		var filePath = GetSettingsFilePath(fileName);

		if (!File.Exists(filePath))
		{
			return new T();
		}

		try
		{
			var json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
		}
		catch (Exception)
		{
			// If deserialization fails, return default settings
			return new T();
		}
	}

	/// <summary>
	/// Saves settings to a JSON file.
	/// Note: This reflection-based overload may fail in trimmed/AOT builds. Prefer
	/// the overload accepting JsonTypeInfo&lt;T&gt; for trim-safe serialization.
	/// </summary>
	/// <typeparam name="T">The settings type to serialize</typeparam>
	/// <param name="settings">The settings instance to save</param>
	/// <param name="fileName">The settings file name</param>
	public static void Save<T>(T settings, string fileName)
	{
		var filePath = GetSettingsFilePath(fileName);

		// Ensure directory exists
		Directory.CreateDirectory(AppDataPath);

		var json = JsonSerializer.Serialize(settings, JsonOptions);
		File.WriteAllText(filePath, json);
	}

	/// <summary>
	/// Gets the settings file name for a plugin based on its namespace.
	/// </summary>
	/// <param name="pluginNamespace">The plugin's namespace (e.g., "Ufex.FileTypes.ZIP")</param>
	/// <returns>The settings file name (e.g., "Ufex.FileTypes.ZIP.json")</returns>
	public static string GetPluginSettingsFileName(string pluginNamespace)
	{
		return $"{pluginNamespace}.json";
	}

	/// <summary>
	/// Gets the settings file name for a plugin based on its type.
	/// </summary>
	/// <param name="pluginType">A type from the plugin assembly</param>
	/// <returns>The settings file name based on the type's namespace</returns>
	public static string GetPluginSettingsFileName(Type pluginType)
	{
		var ns = pluginType.Namespace ?? pluginType.Assembly.GetName().Name ?? "Unknown";
		return GetPluginSettingsFileName(ns);
	}

	/// <summary>
	/// Loads settings for a plugin based on its namespace.
	/// </summary>
	/// <typeparam name="T">The settings type</typeparam>
	/// <param name="pluginType">A type from the plugin assembly</param>
	/// <returns>The loaded settings or a new default instance</returns>
	public static T LoadPluginSettings<T>(Type pluginType) where T : new()
	{
		var fileName = GetPluginSettingsFileName(pluginType);
		return Load<T>(fileName);
	}

	/// <summary>
	/// Saves settings for a plugin based on its namespace.
	/// </summary>
	/// <typeparam name="T">The settings type</typeparam>
	/// <param name="settings">The settings to save</param>
	/// <param name="pluginType">A type from the plugin assembly</param>
	public static void SavePluginSettings<T>(T settings, Type pluginType)
	{
		var fileName = GetPluginSettingsFileName(pluginType);
		Save(settings, fileName);
	}
}
