using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using Ufex.API;
using Ufex.API.Format;

namespace Ufex.Desktop;

public partial class OptionsWindow : Window
{
	private readonly DesktopSettings _settings;
	private readonly List<string> _monospacedFonts;

	public OptionsWindow(DesktopSettings settings)
	{
		InitializeComponent();
		_settings = settings;
		_monospacedFonts = GetMonospacedFonts();
		LoadSettings();
	}

	private List<string> GetMonospacedFonts()
	{
		// Common monospaced fonts that are likely available
		var commonMonospacedFonts = new[]
		{
			"Consolas",
			"Courier New",
			"Lucida Console",
			"Monaco",
			"Menlo",
			"DejaVu Sans Mono",
			"Liberation Mono",
			"Source Code Pro",
			"Fira Code",
			"JetBrains Mono",
			"Cascadia Code",
			"Cascadia Mono",
			"Ubuntu Mono",
			"Roboto Mono",
			"Inconsolata",
			"Hack",
			"Droid Sans Mono",
			"SF Mono",
			"Andale Mono"
		};

		var installedFonts = FontManager.Current.SystemFonts
			.Select(f => f.Name)
			.ToHashSet();

		// Filter to only fonts that are installed
		var availableFonts = commonMonospacedFonts
			.Where(f => installedFonts.Contains(f))
			.OrderBy(f => f)
			.ToList();

		// If none of our known monospaced fonts are available, add Courier as fallback
		if (availableFonts.Count == 0)
		{
			availableFonts.Add("Courier New");
		}

		return availableFonts;
	}

	private void LoadSettings()
	{
		// Load font settings
		FontComboBox.ItemsSource = _monospacedFonts;
		var currentFont = _settings.Hex.FontFamily;
		var fontIndex = _monospacedFonts.IndexOf(currentFont);
		FontComboBox.SelectedIndex = fontIndex >= 0 ? fontIndex : 0;

		// Load number format settings
		var formatTag = _settings.NumberFormat.DefaultNumberFormat.ToString();
		for (int i = 0; i < NumberFormatComboBox.Items.Count; i++)
		{
			if (NumberFormatComboBox.Items[i] is ComboBoxItem item && 
			    item.Tag?.ToString() == formatTag)
			{
				NumberFormatComboBox.SelectedIndex = i;
				break;
			}
		}

		PrefixHexCheckBox.IsChecked = _settings.NumberFormat.PrefixHexWith0x;
		LeadingZerosCheckBox.IsChecked = _settings.NumberFormat.ShowLeadingZerosForHex;
	}

	private void SaveSettings()
	{
		// Save font settings
		if (FontComboBox.SelectedItem is string selectedFont)
		{
			_settings.Hex.FontFamily = selectedFont;
		}

		// Save number format settings
		if (NumberFormatComboBox.SelectedItem is ComboBoxItem selectedFormat &&
		    selectedFormat.Tag is string formatTag)
		{
			_settings.NumberFormat.DefaultNumberFormat = formatTag switch
			{
				"Default" => NumberFormat.Default,
				"Hexadecimal" => NumberFormat.Hexadecimal,
				"Decimal" => NumberFormat.Decimal,
				"Binary" => NumberFormat.Binary,
				"Ascii" => NumberFormat.Ascii,
				_ => NumberFormat.Default
			};
		}

		_settings.NumberFormat.PrefixHexWith0x = PrefixHexCheckBox.IsChecked ?? false;
		_settings.NumberFormat.ShowLeadingZerosForHex = LeadingZerosCheckBox.IsChecked ?? false;

		// Persist to disk
		_settings.Save();
	}

	private void OnOkClick(object? sender, RoutedEventArgs e)
	{
		SaveSettings();
		Close(true);
	}

	private void OnCancelClick(object? sender, RoutedEventArgs e)
	{
		Close(false);
	}
}
