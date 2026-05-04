using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ufex.API;
using Ufex.API.Format;
using Ufex.FileType;
using Ufex.Hex;
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

	// Color profile manager - loaded async on startup
	private ColorProfileManager _colorProfileManager = new ColorProfileManager();

	// Currently detected file type ID (for color profile filtering)
	private string? _currentFileTypeId;

	// File type ID chain: [detected type, parent, grandparent, ...]
	private List<string> _currentFileTypeChain = new List<string>();

	public MainWindow()
	{
		InitializeComponent();
		InitializeSettings();
		InitializeFileTypeManager();
		InitializeColorProfiles();

		// Pass settings to child views that need them
		StructureTab.SetSettings(_settings);

		// Subscribe to window close event to save settings
		Closing += OnWindowClosing;

		// Wire up drag-and-drop on the drop panel
		AddHandler(DragDrop.DragOverEvent, OnDragOver);
		AddHandler(DragDrop.DropEvent, OnDrop);

		// Subscribe to InfoTab file type changed event
		InfoTab.FileTypeChanged += OnInfoTabFileTypeChanged;

		UpdateThemeMenuChecks();
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

	private void InitializeColorProfiles()
	{
		var appPath = AppContext.BaseDirectory;
		var colorProfilesDir = Path.Combine(appPath, "config", "color-profiles");
		_colorProfileManager.LoadAsync(colorProfilesDir);
	}

	/// <summary>
	/// Builds a list of file type IDs starting from the detected type
	/// and walking up the parent chain.
	/// </summary>
	private List<string> BuildFileTypeChain(FileTypeRecord? fileType)
	{
		var chain = new List<string>();
		if (fileType == null || _fileTypeManager == null)
			return chain;

		chain.Add(fileType.ID);

		string? currentParentId = fileType.ParentID;
		var visited = new HashSet<string> { fileType.ID };

		while (!string.IsNullOrEmpty(currentParentId) && visited.Add(currentParentId))
		{
			chain.Add(currentParentId);
			_fileTypeManager.FileTypes.FileTypesByID.TryGetValue(currentParentId, out var parent);
			currentParentId = parent?.ParentID;
		}

		return chain;
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

	/// <summary>
	/// Shows the tab control and hides the drop panel (file is open).
	/// </summary>
	private void ShowFileContent()
	{
		DropPanel.IsVisible = false;
		MainTabControl.IsVisible = true;
	}

	/// <summary>
	/// Shows the drop panel and hides the tab control (no file open).
	/// </summary>
	private void ShowDropPanel()
	{
		MainTabControl.IsVisible = false;
		DropPanel.IsVisible = true;
	}

	private async void OnOpenClick(object? sender, RoutedEventArgs e)
	{
		await BrowseAndOpenFileAsync();
	}

	private async void OnBrowseClick(object? sender, RoutedEventArgs e)
	{
		await BrowseAndOpenFileAsync();
	}

	private async Task BrowseAndOpenFileAsync()
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

	private void OnDragOver(object? sender, DragEventArgs e)
	{
		e.DragEffects = e.DataTransfer.Contains(DataFormat.File)
			? DragDropEffects.Copy
			: DragDropEffects.None;
	}

	private async void OnDrop(object? sender, DragEventArgs e)
	{
		if (!e.DataTransfer.Contains(DataFormat.File)) return;

		var files = e.DataTransfer.TryGetFiles();
		if (files == null) return;

		var file = files.FirstOrDefault();
		var filePath = file?.TryGetLocalPath();

		if (filePath != null && File.Exists(filePath))
		{
			await OpenFileAsync(filePath);
		}
	}

	/// <summary>
	/// Opens a file and loads its information asynchronously.
	/// Heavy processing runs on a background thread to keep the UI responsive.
	/// </summary>
	/// <param name="filePath">The full path to the file to open.</param>
	/// <param name="overrideFileType">If set, skip auto-resolution and use this file type directly.</param>
	private async Task OpenFileAsync(string filePath, FileTypeRecord? overrideFileType = null)
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
			ShowFileContent();

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
					Logger.Error(ex, "MainWindow.OpenFileAsync: Failed to get FileInfo");
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
				DetectionResult? detectionResult = await Task.Run(() =>
				{
					try
					{
						return fileTypeManager.DetectFileTypeDetailed(filePath);
					}
					catch (Exception ex)
					{
						Logger.Error(ex, $"File type detection failed: {ex.Message}");
						return null;
					}
				});

				DetectionMatch? selectedMatch = null;

				if (overrideFileType != null && detectionResult != null)
				{
					// User explicitly selected a type — find it in the detection result
					selectedMatch = detectionResult.Matches
						.FirstOrDefault(m => m.FileType.ID == overrideFileType.ID);
					detectedFileType = overrideFileType;
					Logger.Info($"Using override file type: {overrideFileType.ID} - {overrideFileType.Description}");
				}
				else if (detectionResult != null && detectionResult.Count == 1)
				{
					selectedMatch = detectionResult.Best;
					detectedFileType = selectedMatch.FileType;
					Logger.Info($"Detected file type: {detectedFileType.ID} - {detectedFileType.Description}");
				}
				else if (detectionResult != null && detectionResult.Count > 1)
				{
					Logger.Info($"Multiple file types detected ({detectionResult.Count}):");
					foreach (var match in detectionResult.Matches)
					{
						Logger.Info($" - {match.FileType.ID} - {match.FileType.Description}");
					}

					selectedMatch = ResolveMultipleFileTypes(detectionResult);

					// If hierarchy/no-plugin checks didn't resolve it, show selection dialog
					if (selectedMatch == null)
					{
						var selectionWindow = new FileTypeSelectionWindow(detectionResult, fileTypeManager);
						selectedMatch = await selectionWindow.ShowDialog<DetectionMatch?>(this);
					}

					if (selectedMatch != null)
					{
						detectedFileType = selectedMatch.FileType;
					}
				}
				else
				{
					Logger.Info("File type detection returned null");
				}

				// Update Info tab with full detection result
				if (detectionResult != null && detectionResult.Count > 0)
				{
					int selectedIndex = selectedMatch != null 
						? detectionResult.Matches.IndexOf(selectedMatch) 
						: 0;
					InfoTab.SetDetectionResult(detectionResult, selectedIndex);
				}
				else
				{
					fileTypeDescription = "Unknown File Type";
					InfoTab.SetFileType(fileTypeDescription);
				}

				fileTypeDescription = detectedFileType?.Description ?? "Unknown File Type";
			}
			else
			{
				InfoTab.SetFileType("Unknown File Type");
			}

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
			HexTab.SetBufferSize(_settings.Hex.BufferSize);
			HexTab.LoadStream(_openFileStream);

			// Step 8b: Track file type and apply color profile
			_currentFileTypeId = detectedFileType?.ID;
			_currentFileTypeChain = BuildFileTypeChain(detectedFileType);
			await UpdateColorProfileMenuAsync();

			// Step 9: If file type is known, check for associated plugin and load handler
			if (detectedFileType != null && _fileTypeManager != null)
			{
				try
				{
					await LoadFileTypeHandler(detectedFileType, filePath);
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "MainWindow.OpenFileAsync: Failed to load file type handler");
					ShowError("File Type Handler Error", 
						$"Failed to load file type handler for {detectedFileType.Description}.\n\n" +
						$"Error: {ex.Message}\n\n" +
						"Basic file information is still available.");
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
	/// Resolves multiple detected file types without user interaction when possible.
	/// Returns the resolved match, or null if a user selection dialog is needed.
	/// </summary>
	private DetectionMatch? ResolveMultipleFileTypes(DetectionResult result)
	{
		if (result == null || result.Count == 0)
			return null;

		var matches = result.Matches;

		// Rule 1: If the types form a parent/child hierarchy (2-3 types), pick the most specific (first).
		if (matches.Count <= 3 && IsParentChildHierarchy(matches))
		{
			Logger.Info($"File types form a parent/child hierarchy, selecting most specific: {matches[0].FileType.ID}");
			return matches[0];
		}

		// Rule 2: If none of the types have a plugin handler, pick the first (most specific).
		if (_fileTypeManager != null && !matches.Any(m => _fileTypeManager.GetFileTypeClassesByFileType(m.FileType.ID).Length > 0))
		{
			Logger.Info($"No file types have plugin handlers, selecting first: {matches[0].FileType.ID}");
			return matches[0];
		}

		// Rule 3: Need user selection
		return null;
	}

	/// <summary>
	/// Checks whether the detected file types form a single parent/child chain.
	/// The list is expected to be sorted by specificity (most specific first).
	/// </summary>
	private static bool IsParentChildHierarchy(System.Collections.Generic.List<DetectionMatch> matches)
	{
		if (matches.Count < 2)
			return false;

		// Walk the chain: each type (except the last) should have its ParentID
		// equal to the next type's ID.
		for (int i = 0; i < matches.Count - 1; i++)
		{
			var current = matches[i].FileType;
			var next = matches[i + 1].FileType;
			if (string.IsNullOrEmpty(current.ParentID) || current.ParentID != next.ID)
				return false;
		}

		return true;
	}

	/// <summary>
	/// Handles the file type changed event from the InfoTab dropdown.
	/// </summary>
	private async void OnInfoTabFileTypeChanged(object? sender, DetectionMatch? match)
	{
		if (match == null || match.FileType == null || _openFileStream == null)
			return;

		// Get the file path before we close everything
		string filePath = _openFileStream.Name;

		Logger.Info($"User changed file type to: {match.FileType.ID} - {match.FileType.Description}");

		// Re-open the file from scratch with the selected type
		await OpenFileAsync(filePath, match.FileType);
	}

	/// <summary>
	/// Loads and processes a file type handler for the given file type.
	/// </summary>
	private async Task LoadFileTypeHandler(Ufex.FileType.FileTypeRecord fileType, string filePath)
	{
		if (_fileTypeManager == null || _openFileStream == null)
			return;

		SetStatus("Loading file type handler...");

		// Get the file type classes for this file type
		var fileTypeClasses = _fileTypeManager.GetFileTypeClassesByFileType(fileType.ID);
		Logger.Info($"Found {fileTypeClasses.Length} file type classes for {fileType.ID}");

		if (fileTypeClasses == null || fileTypeClasses.Length == 0)
		{
			Logger.Info($"No file type classes found for {fileType.ID}");
			SetStatus($"No plugin available for {fileType.Description}");
			return;
		}

		// Use the first one for now
		var fileTypeClass = fileTypeClasses[0];
		Logger.Info($"Using file type class: {fileTypeClass.ID} - {fileTypeClass.FullTypeName}");

		// Get the file type instance
		_currentFileType = _fileTypeManager.GetNewClassInstance(fileTypeClass.ID);
		if (_currentFileType == null)
		{
			Logger.Error("Failed to create file type handler instance");
			SetStatus("Error loading handler");
			return;
		}

		// Set up logger for the file type with memory logging enabled
		var assemblyName = _currentFileType.GetType().Assembly.GetName().Name ?? "FileType";
		_currentFileType.Logger = new Logger($"{assemblyName}.log", enableMemoryLog: true);
		Logger.Info($"Created instance of file type class: {_currentFileType.GetType().FullName}");

		// Set the file stream on the file type instance
		_currentFileType.FileInStream = _openFileStream;
		_currentFileType.FilePath = filePath;

		// Process the file
		SetStatus("Processing file...");
		_openFileStream.Position = 0;
		await Task.Run(() =>
		{
			try
			{
				_currentFileType.ProcessFile();
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Error processing file with handler");
				throw;
			}
		});

		// Toggle the visible tabs based on what the file type supports
		bool showStructure = _currentFileType.EnableStructure;
		bool showValidation = _currentFileType.EnableValidation;
		bool showVisual = _currentFileType.EnableVisual;
		SetTabVisibility(showVisual, showStructure, showValidation);

		// Set the number format on the file type instance
		_currentFileType.NumFormat = _currentNumberFormat;

		// Load the QuickInfo table
		if (_currentFileType.QuickInfoTable != null)
		{
			InfoTab.LoadQuickInfo(_currentFileType.QuickInfoTable);
		}

		// Load the structure tab if enabled
		if (showStructure && _currentFileType.TreeNodes != null)
		{
			var formatter = new DataFormatter();
			formatter.NumFormat = _currentNumberFormat;
			StructureTab.SetFileType(_currentFileType, formatter);
			StructureTab.LoadTreeNodes(_currentFileType.TreeNodes);
		}

		// Load the validation tab if enabled
		if (showValidation && _currentFileType.ValidationReport != null)
		{
			ValidationTab.LoadValidationReport(_currentFileType.ValidationReport);
		}

		// Load the visual tab if enabled
		if (showVisual && _currentFileType.Visuals != null)
		{
			long fileSize = _openFileStream?.Length ?? 0;
			VisualTab.LoadVisuals(_currentFileType.Visuals, fileSize);
		}

		// Display the plugin's changelog if it has one
		if (_currentFileType.Logger is Logger logger)
		{
			var changelog = logger.Text;
			if (!string.IsNullOrEmpty(changelog))
			{
				Logger.Info($"Plugin log:\n{changelog}");
			}
		}

		SetStatus($"Loaded: {Path.GetFileName(filePath)}");
	}

	/// <summary>
	/// Closes the currently open file, releasing resources.
	/// </summary>
	private void CloseCurrentFile()
	{
		// Clean up the file type instance
		_currentFileType = null;
		_currentFileTypeId = null;
		_currentFileTypeChain.Clear();

		// Clear the hex viewer before closing the stream
		HexTab.ActiveColorProfile = null;
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
		ShowDropPanel();
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

	private async void OnCopyClick(object? sender, RoutedEventArgs e)
	{
		if (await TryCopySelectionToClipboardAsync())
		{
			SetStatus("Copied selection to clipboard");
		}
		else
		{
			SetStatus("Nothing selected to copy");
		}
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
		MainTabControl.SelectedItem = TabHex;
		HexTab.FocusSearchBox();
	}

	private void OnFindNextClick(object? sender, RoutedEventArgs e)
	{
		MainTabControl.SelectedItem = TabHex;
		HexTab.TriggerFindNext();
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
		try
		{
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
			{
				FileName = "https://github.com/ufex/Ufex/wiki",
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			Logger.Error(ex, "Failed to open help URL");
			SetStatus("Failed to open help page");
		}
	}

	private async void OnAboutClick(object? sender, RoutedEventArgs e)
	{
		var aboutWindow = new AboutWindow();
		await aboutWindow.ShowDialog(this);
	}

	// Theme Menu Handlers
	private void OnThemeAutoClick(object? sender, RoutedEventArgs e)
	{
		App.Instance?.SetTheme(ThemeVariant.Default);
		UpdateThemeMenuChecks();
	}

	private void OnThemeLightClick(object? sender, RoutedEventArgs e)
	{
		App.Instance?.SetTheme(ThemeVariant.Light);
		UpdateThemeMenuChecks();
	}

	private void OnThemeDarkClick(object? sender, RoutedEventArgs e)
	{
		App.Instance?.SetTheme(ThemeVariant.Dark);
		UpdateThemeMenuChecks();
	}

	// Color Profile Menu Handlers

	private async Task UpdateColorProfileMenuAsync()
	{
		// Ensure profiles are loaded
		await _colorProfileManager.WaitForLoadAsync();

		// Remove old dynamic menu items (everything after the separator)
		var items = ColorProfileMenu.Items;
		while (items.Count > 2) // "None" + separator
			items.RemoveAt(items.Count - 1);

		string fileTypeId = _currentFileTypeId ?? "";
		var applicableProfiles = _currentFileTypeChain.Count > 0
			? _colorProfileManager.GetProfilesForFileType(_currentFileTypeChain)
			: _colorProfileManager.GetProfilesForFileType(fileTypeId);

		ColorProfileSeparator.IsVisible = applicableProfiles.Count > 0;

		string? preferredProfileId = null;
		if (!string.IsNullOrEmpty(fileTypeId) &&
			_settings.Hex.ColorProfilePreferences.TryGetValue(fileTypeId, out var savedId))
		{
			preferredProfileId = savedId;
		}

		ColorProfile? profileToApply = null;

		foreach (var profile in applicableProfiles)
		{
			var menuItem = new MenuItem
			{
				Header = profile.Name,
				Tag = profile.ID
			};
			menuItem.Click += OnColorProfileItemClick;
			items.Add(menuItem);

			if (profile.ID == preferredProfileId)
				profileToApply = profile;
		}

		// Apply preferred profile or clear
		HexTab.ActiveColorProfile = profileToApply;
		UpdateColorProfileMenuChecks();
	}

	private void OnColorProfileNoneClick(object? sender, RoutedEventArgs e)
	{
		HexTab.ActiveColorProfile = null;

		// Save preference
		if (!string.IsNullOrEmpty(_currentFileTypeId))
		{
			_settings.Hex.ColorProfilePreferences.Remove(_currentFileTypeId);
			_settings.Save();
		}

		UpdateColorProfileMenuChecks();
	}

	private void OnColorProfileItemClick(object? sender, RoutedEventArgs e)
	{
		if (sender is not MenuItem menuItem || menuItem.Tag is not string profileId)
			return;

		var profile = _colorProfileManager.GetProfileById(profileId);
		if (profile == null) return;

		HexTab.ActiveColorProfile = profile;

		// Save preference
		if (!string.IsNullOrEmpty(_currentFileTypeId))
		{
			_settings.Hex.ColorProfilePreferences[_currentFileTypeId] = profileId;
			_settings.Save();
		}

		UpdateColorProfileMenuChecks();
	}

	private void UpdateColorProfileMenuChecks()
	{
		var activeId = HexTab.ActiveColorProfile?.ID;

		foreach (var item in ColorProfileMenu.Items)
		{
			if (item is MenuItem mi)
			{
				bool isChecked;
				if (mi == ColorProfileNoneMenuItem)
					isChecked = activeId == null;
				else
					isChecked = mi.Tag is string id && id == activeId;

				mi.Icon = isChecked
					? new CheckBox { IsChecked = true, IsHitTestVisible = false }
					: null;
			}
		}
	}

	private void UpdateThemeMenuChecks()
	{
		var current = App.Instance?.RequestedThemeVariant;
		ThemeAutoMenuItem.Icon = current == ThemeVariant.Default ? new CheckBox { IsChecked = true, IsHitTestVisible = false } : null;
		ThemeLightMenuItem.Icon = current == ThemeVariant.Light ? new CheckBox { IsChecked = true, IsHitTestVisible = false } : null;
		ThemeDarkMenuItem.Icon = current == ThemeVariant.Dark ? new CheckBox { IsChecked = true, IsHitTestVisible = false } : null;
	}

	private async Task<bool> TryCopySelectionToClipboardAsync()
	{
		var topLevel = TopLevel.GetTopLevel(this);
		if (topLevel?.Clipboard == null)
		{
			return false;
		}

		string? clipboardText = GetSelectedTextFromFocusedTextBox();
		if (string.IsNullOrEmpty(clipboardText))
		{
			clipboardText = GetSelectedRowTextFromFocusedDataGrid();
		}

		if (string.IsNullOrEmpty(clipboardText))
		{
			return false;
		}

		await topLevel.Clipboard.SetTextAsync(clipboardText);
		return true;
	}

	private string? GetSelectedTextFromFocusedTextBox()
	{
		var focusedElement = GetFocusedStyledElement();
		var textBox = FindAncestorOfType<TextBox>(focusedElement);
		if (textBox == null)
		{
			return null;
		}

		return string.IsNullOrEmpty(textBox.SelectedText) ? null : textBox.SelectedText;
	}

	private string? GetSelectedRowTextFromFocusedDataGrid()
	{
		var focusedElement = GetFocusedStyledElement();
		var dataGrid = FindAncestorOfType<DataGrid>(focusedElement);
		if (dataGrid == null)
		{
			return null;
		}

		var selectedItems = GetSelectedDataGridItems(dataGrid)
			.Select(FormatRowForClipboard)
			.Where(text => !string.IsNullOrWhiteSpace(text))
			.ToArray();

		if (selectedItems.Length == 0 && dataGrid.SelectedItem != null)
		{
			string fallbackRow = FormatRowForClipboard(dataGrid.SelectedItem);
			if (!string.IsNullOrWhiteSpace(fallbackRow))
			{
				return fallbackRow;
			}
		}

		return selectedItems.Length == 0 ? null : string.Join(Environment.NewLine, selectedItems);
	}

	private static object[] GetSelectedDataGridItems(DataGrid dataGrid)
	{
		var selectedItemsProperty = dataGrid.GetType().GetProperty("SelectedItems", BindingFlags.Instance | BindingFlags.Public);
		if (selectedItemsProperty?.GetValue(dataGrid) is IEnumerable selectedItems)
		{
			return selectedItems.Cast<object>().Where(item => item != null).ToArray()!;
		}

		return [];
	}

	private static string FormatRowForClipboard(object item)
	{
		var values = item.GetType()
			.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(prop => prop.CanRead)
			.Where(prop => prop.GetIndexParameters().Length == 0)
			.Where(prop => IsSimpleClipboardType(prop.PropertyType))
			.Select(prop => prop.GetValue(item)?.ToString())
			.Where(value => !string.IsNullOrWhiteSpace(value))
			.ToArray();

		if (values.Length == 0)
		{
			return item.ToString() ?? string.Empty;
		}

		return string.Join("\t", values);
	}

	private static bool IsSimpleClipboardType(Type propertyType)
	{
		Type actualType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

		return actualType == typeof(string) ||
			   actualType == typeof(decimal) ||
			   actualType == typeof(DateTime) ||
			   actualType == typeof(DateTimeOffset) ||
			   actualType == typeof(TimeSpan) ||
			   actualType == typeof(Guid) ||
			   actualType.IsPrimitive ||
			   actualType.IsEnum;
	}

	private StyledElement? GetFocusedStyledElement()
	{
		var topLevel = TopLevel.GetTopLevel(this);
		return topLevel?.FocusManager?.GetFocusedElement() as StyledElement;
	}

	private static T? FindAncestorOfType<T>(StyledElement? element) where T : class
	{
		while (element != null)
		{
			if (element is T match)
			{
				return match;
			}

			element = element.Parent;
		}

		return null;
	}
}
