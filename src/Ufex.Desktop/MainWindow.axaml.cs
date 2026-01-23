using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Ufex.API;
using Ufex.FileType;
using UfexFileInfo = Ufex.API.FileInfo;
using UfexFileType = Ufex.API.FileType;

namespace Ufex.Desktop;

public partial class MainWindow : Window
{
	// Track which tabs are currently visible
	private bool _previewTabVisible = true;
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

	public MainWindow()
	{
		InitializeComponent();
		InitializeFileTypeManager();
	}

	private void InitializeFileTypeManager()
	{
		try
		{
			var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
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
	public void SetTabVisibility(bool showPreview, bool showStructure, bool showValidation)
	{
		SetTabVisible(TabPreview, showPreview, ref _previewTabVisible);
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
				OpenFile(filePath);
			}
			else
			{
				SetStatus("Error: Could not get local file path");
			}
		}
	}

	/// <summary>
	/// Opens a file and loads its information.
	/// </summary>
	/// <param name="filePath">The full path to the file to open.</param>
	private void OpenFile(string filePath)
	{
		try
		{
			// Step 1: Display a loading message in the status bar
			SetStatus("Opening file...");

			// Step 2: Reset the tabs and content
			CloseCurrentFile();
			InfoTab.Clear();
			StructureTab.Clear();
			ResetTabs();

			// Step 3: Update the title bar
			var fileName = Path.GetFileName(filePath);
			Title = $"ufex - {fileName}";

			// Step 4: Get the FileInfo using Ufex.API.FileInfo.FromFile
			SetStatus("Reading file information...");
			UfexFileInfo? fileInfo = null;
			try
			{
				fileInfo = UfexFileInfo.FromFile(filePath);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to get FileInfo: {ex.Message}");
			}

			// Update the file info/attributes on the Info tab
			if (fileInfo != null)
			{
				InfoTab.LoadFileInfo(fileInfo);
			}
			else
			{
				// Fallback to basic file info if Ufex.API.FileInfo fails
				InfoTab.LoadFileInfo(filePath);
			}

			// Step 5: Determine the file type using FileTypeManager
			SetStatus("Identifying file type...");
			FILETYPE? detectedFileType = null;
			string fileTypeDescription = "Unknown File Type";
			
			if (_fileTypeManager != null)
			{
				try
				{
					detectedFileType = _fileTypeManager.GetFileType(filePath);
					if (detectedFileType != null)
					{
						Logger.Info($"Detected file type: {detectedFileType.ID} - {detectedFileType.Description}");
						fileTypeDescription = detectedFileType.Description;
					}
					else
					{
						Logger.Info("File type detection returned null");
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"File type detection failed: {ex.Message}");
					Console.WriteLine($"Failed to identify file type: {ex.Message}");
				}
			}

			// Step 6: Display the file type on the Info tab
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

			// Step 8: FUTURE - Initialize the hex viewer
			// TODO: HexTab.Initialize(_openFileStream);

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
							Logger.Info($"Created instance of file type class: {_currentFileType.GetType().FullName}");

							// Set the file stream on the file type instance
							_currentFileType.m_FileStream = _openFileStream;
							_currentFileType.FilePath = filePath;

							// Step 10: Run the ProcessFile function
							SetStatus("Processing file...");
							bool processResult = _currentFileType.ProcessFile();

							if (!processResult)
							{
								Logger.Error("ProcessFile returned false");
								Console.WriteLine("ProcessFile returned false");
							}

							// Step 11: Toggle visible tabs based on file type settings
							SetTabVisibility(
								_currentFileType.ShowGraphic,      // Preview tab
								_currentFileType.ShowTechnical,    // Structure tab
								_currentFileType.ShowFileCheck     // Validation tab
							);

							// Step 12: Set number format on the file type instance
							_currentFileType.NumFormat = _currentNumberFormat;

							// Step 13: Call GetQuickInfo() to populate the data grid on the Info tab
							try
							{
								var quickInfo = _currentFileType.GetQuickInfo();
								if (quickInfo != null)
								{
									InfoTab.LoadQuickInfo(quickInfo);
								}
							}
							catch (Exception ex)
							{
								Logger.Error($"Failed to get QuickInfo: {ex.Message}");
							}

							// Step 14: Populate the structure tab with TreeNodes if ShowTechnical is true
							if (_currentFileType.ShowTechnical)
							{
								try
								{
									StructureTab.SetFileType(_currentFileType);
									StructureTab.LoadTreeNodes(_currentFileType.TreeNodes);
								}
								catch (Exception ex)
								{
									Logger.Error($"Failed to load structure: {ex.Message}");
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Failed to load file type handler: {ex.Message}");
					Console.WriteLine($"Failed to load file type handler: {ex.Message}");
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
	/// Closes the currently open file, releasing resources.
	/// </summary>
	private void CloseCurrentFile()
	{
		// Clean up the file type instance
		_currentFileType = null;

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
	private void OnFileTypeManagerClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Open File Type Manager
	}

	private void OnOptionsClick(object? sender, RoutedEventArgs e)
	{
		// TODO: Open Options dialog
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
}
