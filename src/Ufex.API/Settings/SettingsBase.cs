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
	/// </summary>
	public void Save()
	{
		SettingsManager.Save(this, FileName);
	}
}
