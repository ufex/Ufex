using System;

namespace Ufex.API.Settings;

/// <summary>
/// Base class for settings that provides common functionality.
/// </summary>
public abstract class SettingsBase
{
	/// <summary>
	/// Gets the file name for this settings instance.
	/// Override this in derived classes to specify a custom file name.
	/// </summary>
	public abstract string FileName { get; }

	/// <summary>
	/// Saves this settings instance to disk.
	/// Derived classes should override <see cref="SaveCore"/> to provide
	/// trim-safe serialization via a source-generated JsonTypeInfo.
	/// </summary>
	public void Save()
	{
		SaveCore();
	}

	/// <summary>
	/// Override in derived classes to call <see cref="SettingsManager.Save{T}(T, string, System.Text.Json.Serialization.Metadata.JsonTypeInfo{T})"/>
	/// with the appropriate source-generated JsonTypeInfo for trim-safe serialization.
	/// The default implementation falls back to reflection-based serialization which
	/// may fail in trimmed/AOT builds.
	/// </summary>
	protected virtual void SaveCore()
	{
		SettingsManager.Save(this, FileName);
	}
}
