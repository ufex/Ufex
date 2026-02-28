using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Ufex.API;
using Ufex.API.Format;
using Ufex.FileType;
using UfexFileInfo = Ufex.API.FileInfo;
using UfexFileType = Ufex.API.FileType;

namespace Ufex.Desktop;

public partial class MainWindow : Window
{
	// Track which tabs are currently visible
	private bool _visualTabVisible = true;
	private bool _structureTabVisible = true;
	private bool _validationTabVisible = true;

	// File Type Manager instance
	private FileTypeManager? _fileTypeManager;

	// Currently open file stream (kept open for hex editor)
	private FileStream? _openFileStream;

	// Current file type handler instance
	private UfexFileType? _currentFileType;

	// Current number format setting
	private NumberFormat _currentNumberFormat = NumberFormat.Default;

	private Logger Logger = new Logger("Desktop_MainWindow.log");

	// Application settings
	private DesktopSettings _settings = null!;

	public MainWindow()
	{
		InitializeComponent();
		InitializeSettings();
		InitializeFileTypeManager();

		// Pass settings to child views that need them
		StructureTab.SetSettings(_settings);

		// Subscribe to window close event to save settings
		Closing += OnWindowClosing;
	}

	private void InitializeSettings()
	{
		_settings = DesktopSettings.Load();

		// Apply loaded settings
		_currentNumberFormat = _settings.NumberFormat.DefaultNumberFormat;

		// Apply window size if not maximized
		if (!_settings.WindowMaximized)
		{
			Width = _settings.WindowWidth;
			Height = _settings.WindowHeight;
		}
		else
		{
			WindowState = WindowState.Maximized;
		}
	}

	private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
	{
		SaveSettings();
	}

	private void SaveSettings()
	{
		// Save window state
		_settings.WindowMaximized = WindowState == WindowState.Maximized;

		if (WindowState != WindowState.Maximized)
		{
			_settings.WindowWidth = Width;
			_settings.WindowHeight = Height;
		}

		// Save current number format
		_settings.NumberFormat.DefaultNumberFormat = _currentNumberFormat;

		_settings.Save();
	}

	private void InitializeFileTypeManager()
	{
		try
		{
			// Use AppContext.BaseDirectory instead of Assembly.Location because
			// Assembly.Location returns empty string for single-file publish (.NET 5+),
			// which breaks path resolution on macOS .app bundles where CWD != exe dir.
			var appPath = AppContext.BaseDirectory;
			var configDirectories = new[]
			{
				Path.Combine(appPath, "config"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ufex", "config")
			};
			_fileTypeManager = new FileTypeManager(appPath, configDirectories);
		}
		catch (Exception ex)
		{
			// Log error but don't crash - file type detection will be unavailable
			Console.WriteLine($"Failed to initialize FileTypeManager: {ex.Message}");
		}
	}

	/// <summary>
	/// Sets the status bar text.
	/// </summary>
	public void SetStatus(string message)
	{
		Logger.Info(message);
		StatusBar.Text = message;
	}

	/// <summary>
	/// Shows or hides tabs based on the loaded file type.
	/// Info and Hex tabs are always visible.
	/// </summary>
	public void SetTabVisibility(bool showVisual, bool showStructure, bool showValidation)
	{
		SetTabVisible(TabVisual, showVisual, ref _visualTabVisible);
		SetTabVisible(TabStructure, showStructure, ref _structureTabVisible);
		SetTabVisible(TabValidation, showValidation, ref _validationTabVisible);
	}

	private void SetTabVisible(TabItem tab, bool visible, ref bool currentState)
	{
		if (visible && !currentState)
		{
			// Add tab back - need to find the correct position
			if (!MainTabControl.Items.Contains(tab))
			{
				MainTabControl.Items.Add(tab);
			}
			currentState = true;
		}
		else if (!visible && currentState)
		{
			// Remove tab
			if (MainTabControl.Items.Contains(tab))
			{
				// If this tab is selected, switch to Info first
				if (MainTabControl.SelectedItem == tab)
				{
					MainTabControl.SelectedItem = TabInfo;
				}
				MainTabControl.Items.Remove(tab);
			}
			currentState = false;
		}
	}

	/// <summary>
	/// Resets tabs to show only Info and Hex (for when no file is open or file type is unknown).
	/// </summary>
	public void ResetTabs()
	{
		SetTabVisibility(false, false, false);
		MainTabControl.SelectedItem = TabInfo;
	}

	/// <summary>
	/// Shows all tabs (for when a fully supported file is opened).
	/// </summary>
	public void ShowAllTabs()
	{
		SetTabVisibility(true, true, true);
	}

	private async void OnOpenClick(object? sender, RoutedEventArgs e)
	{
		var topLevel = GetTopLevel(this);
		if (topLevel == null) return;

		var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = "Open File",
			AllowMultiple = false,
			FileTypeFilter = new[]
			{
				new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
			}
		});

		if (files.Count > 0)
		{
			var file = files[0];
			var filePath = file.TryGetLocalPath();
			
			if (filePath != null)
			{
				await OpenFileAsync(filePath);
			}
			else
			{
				SetStatus("Error: Could not get local file path");
			}
		}
	}

	/// <summary>
	/// Opens a file and loads its information asynchronously.
	/// Heavy processing runs on a background thread to keep the UI responsive.
	/// </summary>
	/// <param name="filePath">The full path to the file to open.</param>
	private async Task OpenFileAsync(string filePath)
	{
		try
		{
			// Step 1: Display a loading message in the status bar
			SetStatus("Opening file...");

			// Step 2: Reset the tabs and content (UI thread)
			CloseCurrentFile();
			InfoTab.Clear();
			StructureTab.Clear();
			ValidationTab.Clear();
			VisualTab.Clear();
			ResetTabs();

			// Step 3: Update the title bar
			var fileName = Path.GetFileName(filePath);
			Title = $"ufex - {fileName}";

			// Add to recent files
			_settings.AddRecentFile(filePath);

			// Step 4: Get the FileInfo - run on background thread
			SetStatus("Reading file information...");
			UfexFileInfo? fileInfo = await Task.Run(() =>
			{
				try
				{
					return UfexFileInfo.FromFile(filePath);
				}
				catch (Exception ex)
				{
					Logger.Error($"Failed to get FileInfo: {ex.Message}");
					return null;
				}
			});

			// Update the file info/attributes on the Info tab (UI thread)
			if (fileInfo != null)
			{
				InfoTab.LoadFileInfo(fileInfo);
			}
			else
			{
				// Fallback to basic file info if Ufex.API.FileInfo fails
				InfoTab.LoadFileInfo(filePath);
			}

			// Step 5: Determine the file type using FileTypeManager - run on background thread
			SetStatus("Identifying file type...");
			FileTypeRecord? detectedFileType = null;
			string fileTypeDescription = "Unknown File Type";
			
			if (_fileTypeManager != null)
			{
				var fileTypeManager = _fileTypeManager;
				var result = await Task.Run(() =>
				{
					try
					{
						var detectedTypes = fileTypeManager.DetectFileType(filePath);
						if(detectedTypes.Length == 1)
						{
							var detected = detectedTypes[0];
							Logger.Info($"Detected file type: {detected.ID} - {detected.Description}");
							return (detected, detected.Description);
						}
						else if(detectedTypes.Length > 1)
						{
							Logger.Info($"Multiple file types detected ({detectedTypes.Length}):");
							foreach(var dt in detectedTypes)
							{
								Logger.Info($" - {dt.ID} - {dt.Description}");
							}
							// TODO: display a dialog box to let the user choose
							// Just return the first one for now
							var detected = detectedTypes[0];
							return (detected, detected.Description);
						}
						else
						{
							Logger.Info("File type detection returned null");
							return ((FileTypeRecord?)null, "Unknown File Type");
						}
					}
					catch (Exception ex)
					{
						Logger.Error($"File type detection failed: {ex.Message}");
						return ((FileTypeRecord?)null, "Unknown File Type");
					}
				});
				detectedFileType = result.Item1;
				fileTypeDescription = result.Item2;
			}

			// Step 6: Display the file type on the Info tab (UI thread)
			InfoTab.SetFileType(fileTypeDescription);

			// Step 7: Open a read-only file stream for the file
			SetStatus("Opening file stream...");
			try
			{
				_openFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			catch (Exception ex)
			{
				ShowError("Failed to open file", ex.Message);
				SetStatus("Error opening file");
				return;
			}

			// Step 8: Initialize the hex viewer with the file stream (UI thread)
			HexTab.LoadStream(_openFileStream);

			// Step 9: If file type is known, check for associated plugin
			if (detectedFileType != null && _fileTypeManager != null)
			{
				SetStatus("Loading file type handler...");
				try
				{
					// Get the file type classes for this file type
					var fileTypeClasses = _fileTypeManager.GetFileTypeClassesByFileType(detectedFileType.ID);
					Logger.Info($"Found {fileTypeClasses.Length} file type classes for {detectedFileType.ID}");
					if (fileTypeClasses != null && fileTypeClasses.Length > 0)
					{
						// Use the first one for now
						var fileTypeClass = fileTypeClasses[0];
						Logger.Info($"Using file type class: {fileTypeClass.ID} - {fileTypeClass.FullTypeName}");

						// Get the file type instance
						_currentFileType = _fileTypeManager.GetNewClassInstance(fileTypeClass.ID);
						if (_currentFileType != null)
						{
							// Set up logger for the file type with memory logging enabled
							var assemblyName = _currentFileType.GetType().Assembly.GetName().Name ?? "FileType";
							_currentFileType.Logger = new Logger($"{assemblyName}.log", enableMemoryLog: true);

							Logger.Info($"Created instance of file type class: {_currentFileType.GetType().FullName}");

							// Set the file stream on the file type instance
							_openFileStream.Seek(0, SeekOrigin.Begin);
							_currentFileType.FileInStream = _openFileStream;
							_currentFileType.FilePath = filePath;

							// Step 10: Run the ProcessFile function on background thread
							SetStatus("Processing file...");
							var fileType = _currentFileType;
							bool processResult = await Task.Run(() => fileType.ProcessFile());

							if (!processResult)
							{
								Logger.Error("ProcessFile returned false");
							}

							// Step 11: Toggle visible tabs based on file type settings (UI thread)
							SetTabVisibility(
								_currentFileType.ShowGraphic,      // Visual tab
								_currentFileType.ShowTechnical,    // Structure tab
								_currentFileType.ShowFileCheck     // Validation tab
							);

							// Step 12: Set number format on the file type instance
							_currentFileType.NumFormat = _currentNumberFormat;

							// Step 13: Call QuickInfoTable property to populate the data grid on the Info tab
							try
							{
								var quickInfo = _currentFileType.QuickInfoTable;
								if (quickInfo != null)
								{
									InfoTab.LoadQuickInfo(quickInfo);
								}
							}
							catch (Exception ex)
							{
								Logger.Error($"Failed to get QuickInfoTable: {ex.Message}");
							}

							// Step 14: Populate the structure tab with TreeNodes if ShowTechnical is true
							if (_currentFileType.ShowTechnical)
							{
								try
								{
									var formatter = new DataFormatter();
									formatter.NumFormat = _currentNumberFormat;
									StructureTab.SetFileType(_currentFileType, formatter);
									StructureTab.LoadTreeNodes(_currentFileType.TreeNodes);
								}
								catch (Exception ex)
								{
									Logger.Error($"Failed to load structure: {ex.Message}");
								}
							}

							// Step 15: Retrieve the ValidationReport and display on Validation tab
							if (_currentFileType.ShowFileCheck)
							{
								try
								{
									var validationReport = _currentFileType.ValidationReport;
									ValidationTab.LoadValidationReport(validationReport);
								}
								catch (Exception ex)
								{
									Logger.Error($"Failed to load validation report: {ex.Message}");
								}
							}

							// Step 16: If ShowGraphic is true, build the Visual tab based on the Visuals property
							if (_currentFileType.ShowGraphic)
							{
								try
								{
									var visuals = _currentFileType.Visuals;
									long fileSize = _openFileStream?.Length ?? 0;
									VisualTab.LoadVisuals(visuals, fileSize);
								}
								catch (Exception ex)
								{
									Logger.Error($"Failed to load visuals: {ex.Message}");
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.NewException(ex, description: "Failed to load file type handler", funcName: nameof(OpenFileAsync), className: nameof(MainWindow));
					Logger.Error($"Failed to load file type handler:\n{ex}");
					// Continue without the plugin - basic file info is still shown
				}
			}

			// Update status bar with completion message
			SetStatus($"Loaded: {fileName}");
		}
		catch (Exception ex)
		{
			ShowError("Error opening file", ex.Message);
			SetStatus("Error opening file");
		}
	}

	/// <summary>
	/// Opens a file and loads its information.
	/// </summary>
	/// <param name="filePath">The full path to the file to open.</param>
	[Obsolete("Use OpenFileAsync instead for better UI responsiveness")]
	private void OpenFile(string filePath)
	{
		// Fire and forget - for backwards compatibility
		_ = OpenFileAsync(filePath);
	}

	/// <summary>
	/// Closes the currently open file, releasing resources.
	/// </summary>
	private void CloseCurrentFile()
	{
		// Clean up the file type instance
		_currentFileType = null;

		// Clear the hex viewer before closing the stream
		HexTab.Clear();

		// Clear the validation tab
		ValidationTab.Clear();

		// Close the file stream
		if (_openFileStream != null)
		{
			try
			{
				_openFileStream.Close();
				_openFileStream.Dispose();
			}
			catch
			{
				// Ignore errors during cleanup
			}
			_openFileStream = null;
		}
	}

	/// <summary>
	/// Shows an error dialog to the user.
	/// </summary>
	private async void ShowError(string title, string message)
	{
		var errorWindow = new Window
		{
			Title = title,
			Width = 400,
			Height = 200,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			Content = new TextBlock
			{
				Text = message,
				Margin = new Thickness(16),
				TextWrapping = Avalonia.Media.TextWrapping.Wrap
			}
		};

		await errorWindow.ShowDialog(this);
	}

	private void OnCloseClick(object? sender, RoutedEventArgs e)
	{
		CloseCurrentFile();
		Title = "ufex - Universal File Explorer";
		InfoTab.Clear();
		StructureTab.Clear();
		ValidationTab.Clear();
		VisualTab.Clear();
		ResetTabs();
		SetStatus("Ready");
	}

	private void OnExitClick(object? sender, RoutedEventArgs e)
	{
		Close();
	}

	// Edit Menu Handlers
	private void OnCutClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Implement cut functionality
	}

	private void OnCopyClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Implement copy functionality
	}

	private void OnPasteClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Implement paste functionality
	}

	private void OnDeleteClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Implement delete functionality
	}

	// View Menu Handlers - Number Format
	private void OnNumberFormatDefaultClick(object? sender, RoutedEventArgs e)
	{
		SetNumberFormat(NumberFormat.Default);
	}

	private void OnNumberFormatHexClick(object? sender, RoutedEventArgs e)
	{
		SetNumberFormat(NumberFormat.Hexadecimal);
	}

	private void OnNumberFormatDecClick(object? sender, RoutedEventArgs e)
	{
		SetNumberFormat(NumberFormat.Decimal);
	}

	private void OnNumberFormatBinClick(object? sender, RoutedEventArgs e)
	{
		SetNumberFormat(NumberFormat.Binary);
	}

	private void OnNumberFormatAsciiClick(object? sender, RoutedEventArgs e)
	{
		SetNumberFormat(NumberFormat.Ascii);
	}

	/// <summary>
	/// Sets the current number format and applies it to the current file type if one is loaded.
	/// </summary>
	private void SetNumberFormat(NumberFormat format)
	{
		_currentNumberFormat = format;
		
		if (_currentFileType != null)
		{
			_currentFileType.NumFormat = format;
		}

		StructureTab.SetNumberFormat(format);
	}

	// Search Menu Handlers
	private void OnFindClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Open find dialog
	}

	private void OnFindNextClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Find next occurrence
	}

	// Tools Menu Handlers
	private async void OnFileTypeManagerClick(object? sender, RoutedEventArgs e)
	{
		if (_fileTypeManager == null)
		{
			SetStatus("File Type Manager is not available.");
			return;
		}

		var fileTypeManagerWindow = new FileTypeManagerWindow(_fileTypeManager);
		await fileTypeManagerWindow.ShowDialog(this);
	}

	private async void OnOptionsClick(object? sender, RoutedEventArgs e)
	{
		var optionsWindow = new OptionsWindow(_settings);
		var result = await optionsWindow.ShowDialog<bool?>(this);

		if (result == true)
		{
			// Reload settings that may have changed
			_currentNumberFormat = _settings.NumberFormat.DefaultNumberFormat;

			// Apply number format if a file type is loaded
			if (_currentFileType != null)
			{
				_currentFileType.NumFormat = _currentNumberFormat;
			}

			StructureTab.SetNumberFormat(_currentNumberFormat);
		}
	}

	// Help Menu Handlers
	private void OnHelpClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Open help
	}

	private async void OnAboutClick(object? sender, RoutedEventArgs e)
	{
		var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

		var textContent = new TextBlock
		{
			Text = $"ufex - Universal File Explorer\n\nVersion: {version}\n\n" +
				   "A cross-platform desktop application for viewing internal data structures and metadata of file formats.",
			Margin = new Thickness(16)
		};

		var aboutWindow = new Window
		{
			Title = "About ufex",
			Width = 400,
			Height = 300,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			Content = textContent
		};

		await aboutWindow.ShowDialog(this);
	}

	// Theme Toggle Handler
	private void OnThemeToggleClick(object? sender, RoutedEventArgs e)
	{
		App.Instance?.ToggleTheme();
		UpdateThemeButtonText();
	}

	private void UpdateThemeButtonText()
	{
		if (App.Instance != null)
		{
			ThemeButtonText.Text = App.Instance.IsDarkTheme ? "Light" : "Dark";
			// Update icon - sun for light mode available, moon for dark mode available
			ThemeIcon.Symbol = App.Instance.IsDarkTheme
				? FluentIcons.Common.Symbol.WeatherSunny
				: FluentIcons.Common.Symbol.WeatherMoon;
		}
	}
}
