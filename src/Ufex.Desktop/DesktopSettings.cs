using System.Collections.Generic;
using Ufex.API;
using Ufex.API.Settings;
using Ufex.API.Format;

namespace Ufex.Desktop;

/// <summary>
/// Settings for the hex viewer.
/// </summary>
public class HexSettings
{
	/// <summary>
	/// The font family name for the hex viewer.
	/// </summary>
	public string FontFamily { get; set; } = "Consolas";
}

/// <summary>
/// Settings for number formatting in the structure view.
/// </summary>
public class NumberFormatSettings
{
	/// <summary>
	/// The default number format for displaying numeric values.
	/// </summary>
	public NumberFormat DefaultNumberFormat { get; set; } = NumberFormat.Default;

	/// <summary>
	/// Whether to prefix hexadecimal numbers with "0x".
	/// </summary>
	public bool PrefixHexWith0x { get; set; } = false;

	/// <summary>
	/// Whether to show leading zeros for hexadecimal numbers.
	/// </summary>
	public bool ShowLeadingZerosForHex { get; set; } = false;
}

/// <summary>
/// Settings for the Ufex Desktop application.
/// </summary>
public class DesktopSettings : SettingsBase
{
	private const string SETTINGS_FILE = "settings.json";

	public override string FileName => SETTINGS_FILE;

	/// <summary>
	/// Hex viewer settings.
	/// </summary>
	public HexSettings Hex { get; set; } = new();

	/// <summary>
	/// Number format settings for structure view.
	/// </summary>
	public NumberFormatSettings NumberFormat { get; set; } = new();

	/// <summary>
	/// Window width on last close.
	/// </summary>
	public double WindowWidth { get; set; } = 1024;

	/// <summary>
	/// Window height on last close.
	/// </summary>
	public double WindowHeight { get; set; } = 768;

	/// <summary>
	/// Whether the window was maximized on last close.
	/// </summary>
	public bool WindowMaximized { get; set; } = false;

	/// <summary>
	/// List of recently opened files.
	/// </summary>
	public List<string> RecentFiles { get; set; } = new();

	/// <summary>
	/// Maximum number of recent files to remember.
	/// </summary>
	public int MaxRecentFiles { get; set; } = 10;

	/// <summary>
	/// Saved column widths for structure view tables, keyed by template name.
	/// Each entry maps a template name to a list of column widths.
	/// </summary>
	public Dictionary<string, List<double>> StructureColumnWidths { get; set; } = new();

	/// <summary>
	/// Loads the desktop settings from disk.
	/// </summary>
	public static DesktopSettings Load()
	{
		return SettingsManager.Load<DesktopSettings>(SETTINGS_FILE);
	}

	/// <summary>
	/// Adds a file to the recent files list.
	/// </summary>
	public void AddRecentFile(string filePath)
	{
		// Remove if already exists (will be re-added at top)
		RecentFiles.Remove(filePath);

		// Insert at beginning
		RecentFiles.Insert(0, filePath);

		// Trim to max size
		while (RecentFiles.Count > MaxRecentFiles)
		{
			RecentFiles.RemoveAt(RecentFiles.Count - 1);
		}
	}
}
