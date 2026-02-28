using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ufex.Controls.Avalonia;

/// <summary>
/// Represents the state of a search operation within a HexView.
/// Holds the search pattern, cached match positions, and current index.
/// </summary>
public class HexSearchState
{
	/// <summary>
	/// The byte pattern that was searched for.
	/// </summary>
	public byte[] Pattern { get; }

	/// <summary>
	/// All match positions found in the stream.
	/// </summary>
	public List<long> Matches { get; }

	/// <summary>
	/// The index of the currently selected match, or -1 if none.
	/// </summary>
	public int CurrentIndex { get; set; }

	/// <summary>
	/// The total number of results found.
	/// </summary>
	public int TotalResults => Matches.Count;

	public HexSearchState(byte[] pattern)
	{
		Pattern = pattern;
		Matches = new List<long>();
		CurrentIndex = -1;
	}
}

/// <summary>
/// Cross-platform hex viewer control for Avalonia.
/// Displays file data in three columns: position, hex values, and ASCII text.
/// Uses buffered file loading for large files.
/// </summary>
public partial class HexView : UserControl
{
	// UI Elements
	private Grid? _mainGrid;
	private Border? _positionBorder;
	private Border? _hexBorder;
	private Border? _asciiBorder;
	private Canvas? _positionCanvas;
	private Canvas? _hexCanvas;
	private Canvas? _asciiCanvas;
	private ScrollBar? _scrollBar;

	// Font settings
	private readonly FontFamily _monoFont = new FontFamily("Courier New");
	private readonly double _fontSize = 13;
	private readonly double _cellHeight = 18;
	private readonly double _positionCellWidth = 72;
	private readonly double _hexCellWidth = 26;
	private readonly double _asciiCellWidth = 14;

	// Display dimensions (-1 means auto-calculate)
	private int _numCols = -1;
	private int _numRows = -1;
	private int _calculatedCols = 16;
	private int _calculatedRows = 20;

	// File handling
	private Stream? _fileStream;
	private bool _ownsStream;
	private long _fileSize;
	private bool _fileLoaded;

	// Buffer management
	private byte[]? _buffer;
	private int _bufferSize = 65536; // 64KB buffer
	private long _bufferStartPosition;

	// Current display state
	private long _displayPosition;
	private long _numLines;

	// Highlight support
	private long _highlightStart;
	private long _highlightEnd;

	// Content width tracking
	private double _contentWidth;

	/// <summary>
	/// Raised when the content width changes (e.g. due to column count change).
	/// </summary>
	public event EventHandler<double>? ContentWidthChanged;

	// Format settings
	private bool _hexCaps = true;

	// Colors (used as fallbacks when theme resources are not available)
	private Color _textColor = Colors.Black;
	private Color _cellBorderColor = Color.FromRgb(127, 157, 185);
	private Color _gridBorderColor = Color.FromRgb(100, 100, 100);
	private int _gridHorizontalSpacing = 6;

	// Whether to use theme resources for colors (true by default)
	private bool _useThemeColors = true;

	public HexView()
	{
		InitializeComponent();
		ActualThemeVariantChanged += OnActualThemeVariantChanged;
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);

		_mainGrid = this.FindControl<Grid>("MainGrid");
		_positionBorder = this.FindControl<Border>("PositionBorder");
		_hexBorder = this.FindControl<Border>("HexBorder");
		_asciiBorder = this.FindControl<Border>("AsciiBorder");
		_positionCanvas = this.FindControl<Canvas>("PositionCanvas");
		_hexCanvas = this.FindControl<Canvas>("HexCanvas");
		_asciiCanvas = this.FindControl<Canvas>("AsciiCanvas");
		_scrollBar = this.FindControl<ScrollBar>("VerticalScrollBar");

		if (_scrollBar != null)
		{
			_scrollBar.Scroll += OnScrollBarScroll;
		}

		ApplyColors();
		UpdateDimensions();
	}

	protected override void OnSizeChanged(SizeChangedEventArgs e)
	{
		base.OnSizeChanged(e);
		CalculateDimensions();
		if (_fileLoaded)
		{
			UpdateScrollbar();
		}
		RedrawAll();
	}

	private void OnActualThemeVariantChanged(object? sender, EventArgs e)
	{
		ApplyColors();
		RedrawAll();
	}

	/// <summary>
	/// Gets a brush from theme resources, falling back to the specified color.
	/// </summary>
	private IBrush GetThemeBrush(string resourceKey, Color fallback)
	{
		if (_useThemeColors && this.TryFindResource(resourceKey, ActualThemeVariant, out var resource) && resource is IBrush brush)
		{
			return brush;
		}
		return new SolidColorBrush(fallback);
	}

	/// <summary>
	/// Gets the current text brush, using theme foreground or the fallback _textColor.
	/// </summary>
	private IBrush GetTextBrush()
	{
		return GetThemeBrush("SystemControlForegroundBaseHighBrush", _textColor);
	}

	/// <summary>
	/// Gets the current cell border brush, using theme resources or the fallback.
	/// </summary>
	private IBrush GetCellBorderBrush()
	{
		return GetThemeBrush("SystemControlForegroundBaseMediumBrush", _cellBorderColor);
	}

	/// <summary>
	/// Gets the current grid border brush, using theme resources or the fallback.
	/// </summary>
	private IBrush GetGridBorderBrush()
	{
		return GetThemeBrush("SystemControlForegroundBaseMediumLowBrush", _gridBorderColor);
	}

	private void ApplyColors()
	{
		var gridBrush = GetGridBorderBrush();

		if (_positionBorder != null)
		{
			_positionBorder.BorderBrush = gridBrush;
			_positionBorder.Margin = new Thickness(0, 0, _gridHorizontalSpacing, 0);
		}
		if (_hexBorder != null)
		{
			_hexBorder.BorderBrush = gridBrush;
			_hexBorder.Margin = new Thickness(0, 0, _gridHorizontalSpacing, 0);
		}
		if (_asciiBorder != null)
		{
			_asciiBorder.BorderBrush = gridBrush;
			_asciiBorder.Margin = new Thickness(0, 0, _gridHorizontalSpacing, 0);
		}
	}

	private void CalculateDimensions()
	{
		// Calculate rows
		if (_numRows == -1)
		{
			if (Bounds.Height > 0)
			{
				_calculatedRows = Math.Max(1, (int)(Bounds.Height / _cellHeight));
			}
		}
		else
		{
			_calculatedRows = _numRows;
		}

		// Calculate columns
		if (_numCols == -1)
		{
			// Calculate based on available width
			double availableWidth = Bounds.Width;
			if (availableWidth > 0)
			{
				// Subtract position column, scrollbar, and spacing
				double usedWidth = _positionCellWidth + 20 + (_gridHorizontalSpacing * 3);
				double remainingWidth = availableWidth - usedWidth;

				// Each byte needs hex cell + ascii cell width
				double widthPerByte = _hexCellWidth + _asciiCellWidth;
				_calculatedCols = Math.Max(4, (int)(remainingWidth / widthPerByte));

				// Round to nearest 4 for nice alignment
				_calculatedCols = (_calculatedCols / 4) * 4;
				if (_calculatedCols < 4) _calculatedCols = 4;
			}
		}
		else
		{
			_calculatedCols = _numCols;
		}

		UpdateCanvasSizes();
	}

	private void UpdateCanvasSizes()
	{
		double canvasHeight = _calculatedRows * _cellHeight;

		if (_positionCanvas != null)
		{
			_positionCanvas.Width = _positionCellWidth;
			_positionCanvas.Height = canvasHeight;
		}
		if (_hexCanvas != null)
		{
			_hexCanvas.Width = _calculatedCols * _hexCellWidth;
			_hexCanvas.Height = canvasHeight;
		}
		if (_asciiCanvas != null)
		{
			_asciiCanvas.Width = _calculatedCols * _asciiCellWidth;
			_asciiCanvas.Height = canvasHeight;
		}

		// Calculate and notify content width
		// position border + hex border + ascii border + scrollbar + spacing + border thicknesses
		double newContentWidth = _positionCellWidth + 2  // position + border
			+ _gridHorizontalSpacing
			+ (_calculatedCols * _hexCellWidth) + 2  // hex + border
			+ _gridHorizontalSpacing
			+ (_calculatedCols * _asciiCellWidth) + 2  // ascii + border
			+ _gridHorizontalSpacing
			+ 20; // scrollbar

		if (Math.Abs(newContentWidth - _contentWidth) > 0.5)
		{
			_contentWidth = newContentWidth;
			ContentWidthChanged?.Invoke(this, _contentWidth);
		}
	}

	private void UpdateDimensions()
	{
		CalculateDimensions();
	}

	/// <summary>
	/// Loads a file by path into the hex viewer.
	/// </summary>
	public void LoadFile(string filePath)
	{
		UnloadFile();

		try
		{
			_fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			_ownsStream = true;
			InitializeFile();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading file: {ex.Message}");
		}
	}

	/// <summary>
	/// Loads a stream into the hex viewer.
	/// </summary>
	public void LoadStream(Stream stream)
	{
		UnloadFile();

		_fileStream = stream;
		_ownsStream = false;
		InitializeFile();
	}

	/// <summary>
	/// Unloads the current file and frees resources.
	/// </summary>
	public void UnloadFile()
	{
		if (_fileLoaded && _ownsStream && _fileStream != null)
		{
			_fileStream.Close();
			_fileStream.Dispose();
		}

		_fileStream = null;
		_fileLoaded = false;
		_buffer = null;
		_fileSize = 0;
		_displayPosition = 0;
		_bufferStartPosition = 0;

		if (_scrollBar != null)
		{
			_scrollBar.IsEnabled = false;
		}

		ClearCanvases();
	}

	private void InitializeFile()
	{
		if (_fileStream == null) return;

		_fileLoaded = true;
		_fileSize = _fileStream.Length;
		_displayPosition = 0;

		// Calculate number of lines
		_numLines = (_fileSize + _calculatedCols - 1) / _calculatedCols;

		// Initialize buffer
		_buffer = new byte[_bufferSize];
		_bufferStartPosition = 0;
		LoadBuffer(0);

		UpdateScrollbar();
		RedrawAll();
	}

	private void UpdateScrollbar()
	{
		if (_scrollBar == null) return;

		// Recalculate number of lines based on current columns
		_numLines = (_fileSize + _calculatedCols - 1) / _calculatedCols;

		if (_numLines > _calculatedRows)
		{
			_scrollBar.IsEnabled = true;
			_scrollBar.Maximum = _numLines - _calculatedRows;
			_scrollBar.LargeChange = _calculatedRows;
			_scrollBar.Value = Math.Min(_scrollBar.Value, _scrollBar.Maximum);
		}
		else
		{
			_scrollBar.IsEnabled = false;
			_scrollBar.Value = 0;
		}
	}

	private void LoadBuffer(long position)
	{
		if (_fileStream == null || _buffer == null) return;

		// Calculate buffer start position (load some data before current position)
		long bufferStart = Math.Max(0, position - _bufferSize / 4);

		// Align to row boundary
		bufferStart = (bufferStart / _calculatedCols) * _calculatedCols;

		_bufferStartPosition = bufferStart;

		// Read data into buffer
		_fileStream.Seek(_bufferStartPosition, SeekOrigin.Begin);
		int bytesToRead = (int)Math.Min(_bufferSize, _fileSize - _bufferStartPosition);
		int bytesRead = 0;
		while (bytesRead < bytesToRead)
		{
			int read = _fileStream.Read(_buffer, bytesRead, bytesToRead - bytesRead);
			if (read == 0) break;
			bytesRead += read;
		}

		// Clear any remaining buffer space
		if (bytesRead < _bufferSize)
		{
			Array.Clear(_buffer, bytesRead, _bufferSize - bytesRead);
		}
	}

	private void OnScrollBarScroll(object? sender, ScrollEventArgs e)
	{
		if (!_fileLoaded || _scrollBar == null) return;

		long newRow = (long)_scrollBar.Value;
		long newPosition = newRow * _calculatedCols;

		if (newPosition != _displayPosition)
		{
			_displayPosition = newPosition;

			// Check if we need to reload the buffer
			long displayEnd = _displayPosition + (_calculatedRows * _calculatedCols);
			if (_displayPosition < _bufferStartPosition ||
			    displayEnd > _bufferStartPosition + _bufferSize)
			{
				LoadBuffer(_displayPosition);
			}

			RedrawAll();
		}
	}

	private void ClearCanvases()
	{
		_positionCanvas?.Children.Clear();
		_hexCanvas?.Children.Clear();
		_asciiCanvas?.Children.Clear();
	}

	private void RedrawAll()
	{
		if (!_fileLoaded || _buffer == null) return;

		ClearCanvases();
		DrawPositionColumn();
		DrawHexColumn();
		DrawAsciiColumn();
	}

	private void DrawPositionColumn()
	{
		if (_positionCanvas == null) return;

		var textBrush = GetTextBrush();
		var cellBorderBrush = GetCellBorderBrush();

		for (int row = 0; row < _calculatedRows; row++)
		{
			long address = _displayPosition + (row * _calculatedCols);
			if (address >= _fileSize) break;

			// Zero-padded, right-aligned position text
			string positionText = address.ToString(_hexCaps ? "X8" : "x8");

			// Create cell with border
			var cellBorder = new Border
			{
				Width = _positionCellWidth,
				Height = _cellHeight,
				BorderBrush = cellBorderBrush,
				BorderThickness = new Thickness(0, 0, 0, 1)
			};

			var textBlock = new TextBlock
			{
				Text = positionText,
				FontFamily = _monoFont,
				FontSize = _fontSize,
				Foreground = textBrush,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center,
				Padding = new Thickness(0, 0, 4, 0)
			};

			cellBorder.Child = textBlock;

			Canvas.SetLeft(cellBorder, 0);
			Canvas.SetTop(cellBorder, row * _cellHeight);
			_positionCanvas.Children.Add(cellBorder);
		}
	}

	private void DrawHexColumn()
	{
		if (_hexCanvas == null || _buffer == null) return;

		var textBrush = GetTextBrush();
		var cellBorderBrush = GetCellBorderBrush();
		var highlightBrush = new SolidColorBrush(Colors.Yellow);
		var highlightTextBrush = new SolidColorBrush(Colors.Black);

		for (int row = 0; row < _calculatedRows; row++)
		{
			for (int col = 0; col < _calculatedCols; col++)
			{
				long filePos = _displayPosition + (row * _calculatedCols) + col;
				if (filePos >= _fileSize) break;

				long bufferOffset = filePos - _bufferStartPosition;
				if (bufferOffset < 0 || bufferOffset >= _buffer.Length) continue;

				byte value = _buffer[bufferOffset];
				string hexText = value.ToString(_hexCaps ? "X2" : "x2");

				bool isHighlighted = filePos >= _highlightStart && filePos <= _highlightEnd && _highlightStart != _highlightEnd;

				// Create cell with border
				var cellBorder = new Border
				{
					Width = _hexCellWidth,
					Height = _cellHeight,
					BorderBrush = cellBorderBrush,
					BorderThickness = new Thickness(0, 0, 1, 1),
					Background = isHighlighted ? highlightBrush : null
				};

				var textBlock = new TextBlock
				{
					Text = hexText,
					FontFamily = _monoFont,
					FontSize = _fontSize,
					Foreground = isHighlighted ? highlightTextBrush : textBrush,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				};

				cellBorder.Child = textBlock;

				Canvas.SetLeft(cellBorder, col * _hexCellWidth);
				Canvas.SetTop(cellBorder, row * _cellHeight);
				_hexCanvas.Children.Add(cellBorder);
			}
		}
	}

	private void DrawAsciiColumn()
	{
		if (_asciiCanvas == null || _buffer == null) return;

		var textBrush = GetTextBrush();
		var cellBorderBrush = GetCellBorderBrush();
		var highlightBrush = new SolidColorBrush(Colors.Yellow);
		var highlightTextBrush = new SolidColorBrush(Colors.Black);

		for (int row = 0; row < _calculatedRows; row++)
		{
			for (int col = 0; col < _calculatedCols; col++)
			{
				long filePos = _displayPosition + (row * _calculatedCols) + col;
				if (filePos >= _fileSize) break;

				long bufferOffset = filePos - _bufferStartPosition;
				if (bufferOffset < 0 || bufferOffset >= _buffer.Length) continue;

				byte value = _buffer[bufferOffset];
				char displayChar = (value >= 32 && value < 127) ? (char)value : '.';

				bool isHighlighted = filePos >= _highlightStart && filePos <= _highlightEnd && _highlightStart != _highlightEnd;

				// Create cell with border
				var cellBorder = new Border
				{
					Width = _asciiCellWidth,
					Height = _cellHeight,
					BorderBrush = cellBorderBrush,
					BorderThickness = new Thickness(0, 0, 1, 1),
					Background = isHighlighted ? highlightBrush : null
				};

				var textBlock = new TextBlock
				{
					Text = displayChar.ToString(),
					FontFamily = _monoFont,
					FontSize = _fontSize,
					Foreground = isHighlighted ? highlightTextBrush : textBrush,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				};

				cellBorder.Child = textBlock;

				Canvas.SetLeft(cellBorder, col * _asciiCellWidth);
				Canvas.SetTop(cellBorder, row * _cellHeight);
				_asciiCanvas.Children.Add(cellBorder);
			}
		}
	}

	/// <summary>
	/// Scrolls to display the specified file position.
	/// </summary>
	public void GotoPosition(long position)
	{
		if (!_fileLoaded || position < 0 || position >= _fileSize) return;

		long row = position / _calculatedCols;
		_displayPosition = row * _calculatedCols;

		// Ensure we don't scroll past the end
		long maxStartRow = Math.Max(0, _numLines - _calculatedRows);
		if (row > maxStartRow)
		{
			_displayPosition = maxStartRow * _calculatedCols;
		}

		// Update scrollbar
		if (_scrollBar != null)
		{
			_scrollBar.Value = _displayPosition / _calculatedCols;
		}

		// Reload buffer if needed
		long displayEnd = _displayPosition + (_calculatedRows * _calculatedCols);
		if (_displayPosition < _bufferStartPosition ||
		    displayEnd > _bufferStartPosition + _bufferSize)
		{
			LoadBuffer(_displayPosition);
		}

		RedrawAll();
	}

	/// <summary>
	/// Highlights a range of bytes in the display.
	/// </summary>
	public void SetHighlight(long startPosition, long endPosition)
	{
		_highlightStart = Math.Min(startPosition, endPosition);
		_highlightEnd = Math.Max(startPosition, endPosition);
		RedrawAll();
	}

	/// <summary>
	/// Clears any highlight.
	/// </summary>
	public void ClearHighlight()
	{
		_highlightStart = 0;
		_highlightEnd = 0;
		RedrawAll();
	}

	// Search API

	/// <summary>
	/// Searches the entire loaded file for the given byte pattern.
	/// Returns a HexSearchState containing all match positions.
	/// Navigates to and highlights the first match if found.
	/// </summary>
	public HexSearchState Find(byte[] pattern)
	{
		var state = new HexSearchState(pattern);
		if (!_fileLoaded || _fileStream == null || pattern.Length == 0)
			return state;

		// Save current stream position
		long savedPosition = _fileStream.Position;

		try
		{
			_fileStream.Seek(0, SeekOrigin.Begin);

			int searchBufferSize = Math.Max(_bufferSize, pattern.Length * 4);
			byte[] searchBuffer = new byte[searchBufferSize];
			long fileOffset = 0;
			int overlap = pattern.Length - 1;
			int carryOver = 0;

			while (fileOffset < _fileSize)
			{
				// Read chunk (preserving overlap from previous chunk)
				int bytesToRead = searchBufferSize - carryOver;
				int bytesRead = 0;
				while (bytesRead < bytesToRead)
				{
					int read = _fileStream.Read(searchBuffer, carryOver + bytesRead, bytesToRead - bytesRead);
					if (read == 0) break;
					bytesRead += read;
				}

				int totalBytes = carryOver + bytesRead;
				if (totalBytes < pattern.Length) break;

				// Search for pattern in buffer
				long searchStartOffset = fileOffset - carryOver;
				int searchLimit = totalBytes - pattern.Length + 1;

				for (int i = 0; i < searchLimit; i++)
				{
					bool match = true;
					for (int j = 0; j < pattern.Length; j++)
					{
						if (searchBuffer[i + j] != pattern[j])
						{
							match = false;
							break;
						}
					}
					if (match)
					{
						state.Matches.Add(searchStartOffset + i);
					}
				}

				fileOffset += bytesRead;

				// Preserve overlap for next iteration
				if (bytesRead > 0 && fileOffset < _fileSize)
				{
					Array.Copy(searchBuffer, totalBytes - overlap, searchBuffer, 0, overlap);
					carryOver = overlap;
				}
				else
				{
					carryOver = 0;
				}
			}
		}
		finally
		{
			// Restore stream position
			_fileStream.Seek(savedPosition, SeekOrigin.Begin);
		}

		// Navigate to first result
		if (state.TotalResults > 0)
		{
			state.CurrentIndex = 0;
			NavigateToMatch(state);
		}

		return state;
	}

	/// <summary>
	/// Searches the entire loaded file for the given byte pattern (case-insensitive).
	/// Each byte in the pattern is compared ignoring ASCII case.
	/// Returns a HexSearchState containing all match positions.
	/// </summary>
	public HexSearchState FindCaseInsensitive(byte[] pattern)
	{
		var state = new HexSearchState(pattern);
		if (!_fileLoaded || _fileStream == null || pattern.Length == 0)
			return state;

		long savedPosition = _fileStream.Position;

		try
		{
			_fileStream.Seek(0, SeekOrigin.Begin);

			int searchBufferSize = Math.Max(_bufferSize, pattern.Length * 4);
			byte[] searchBuffer = new byte[searchBufferSize];
			long fileOffset = 0;
			int overlap = pattern.Length - 1;
			int carryOver = 0;

			// Pre-compute lowercase pattern
			byte[] lowerPattern = new byte[pattern.Length];
			for (int k = 0; k < pattern.Length; k++)
			{
				byte b = pattern[k];
				if (b >= (byte)'A' && b <= (byte)'Z')
					lowerPattern[k] = (byte)(b + 32);
				else
					lowerPattern[k] = b;
			}

			while (fileOffset < _fileSize)
			{
				int bytesToRead = searchBufferSize - carryOver;
				int bytesRead = 0;
				while (bytesRead < bytesToRead)
				{
					int read = _fileStream.Read(searchBuffer, carryOver + bytesRead, bytesToRead - bytesRead);
					if (read == 0) break;
					bytesRead += read;
				}

				int totalBytes = carryOver + bytesRead;
				if (totalBytes < pattern.Length) break;

				long searchStartOffset = fileOffset - carryOver;
				int searchLimit = totalBytes - pattern.Length + 1;

				for (int i = 0; i < searchLimit; i++)
				{
					bool match = true;
					for (int j = 0; j < pattern.Length; j++)
					{
						byte b = searchBuffer[i + j];
						if (b >= (byte)'A' && b <= (byte)'Z')
							b = (byte)(b + 32);
						if (b != lowerPattern[j])
						{
							match = false;
							break;
						}
					}
					if (match)
					{
						state.Matches.Add(searchStartOffset + i);
					}
				}

				fileOffset += bytesRead;

				if (bytesRead > 0 && fileOffset < _fileSize)
				{
					Array.Copy(searchBuffer, totalBytes - overlap, searchBuffer, 0, overlap);
					carryOver = overlap;
				}
				else
				{
					carryOver = 0;
				}
			}
		}
		finally
		{
			_fileStream.Seek(savedPosition, SeekOrigin.Begin);
		}

		if (state.TotalResults > 0)
		{
			state.CurrentIndex = 0;
			NavigateToMatch(state);
		}

		return state;
	}

	/// <summary>
	/// Navigates to the next search result (wraps around at end).
	/// </summary>
	public void FindNext(HexSearchState state)
	{
		if (state.TotalResults == 0) return;

		state.CurrentIndex = (state.CurrentIndex + 1) % state.TotalResults;
		NavigateToMatch(state);
	}

	/// <summary>
	/// Navigates to the previous search result (wraps around at start).
	/// </summary>
	public void FindPrevious(HexSearchState state)
	{
		if (state.TotalResults == 0) return;

		state.CurrentIndex = (state.CurrentIndex - 1 + state.TotalResults) % state.TotalResults;
		NavigateToMatch(state);
	}

	/// <summary>
	/// Clears the search highlight.
	/// </summary>
	public void ClearSearch()
	{
		ClearHighlight();
	}

	/// <summary>
	/// Navigates to the current match in the search state and highlights it.
	/// </summary>
	private void NavigateToMatch(HexSearchState state)
	{
		if (state.CurrentIndex < 0 || state.CurrentIndex >= state.TotalResults) return;

		long matchPos = state.Matches[state.CurrentIndex];
		GotoPosition(matchPos);
		SetHighlight(matchPos, matchPos + state.Pattern.Length - 1);
	}

	// Properties

	/// <summary>
	/// Gets or sets the buffer size in bytes.
	/// </summary>
	public int BufferSize
	{
		get => _bufferSize;
		set
		{
			if (value >= 512)
			{
				_bufferSize = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets whether hex digits are displayed in uppercase.
	/// </summary>
	public bool HexCaps
	{
		get => _hexCaps;
		set
		{
			_hexCaps = value;
			RedrawAll();
		}
	}

	/// <summary>
	/// Alias for HexCaps for API compatibility.
	/// </summary>
	public bool HexUpperCase
	{
		get => HexCaps;
		set => HexCaps = value;
	}

	/// <summary>
	/// Gets or sets the text color.
	/// </summary>
	public Color TextColor
	{
		get => _textColor;
		set
		{
			_textColor = value;
			RedrawAll();
		}
	}

	/// <summary>
	/// Gets or sets the cell border color.
	/// </summary>
	public Color CellBorderColor
	{
		get => _cellBorderColor;
		set
		{
			_cellBorderColor = value;
			RedrawAll();
		}
	}

	/// <summary>
	/// Gets or sets the grid border color.
	/// </summary>
	public Color GridBorderColor
	{
		get => _gridBorderColor;
		set
		{
			_gridBorderColor = value;
			ApplyColors();
		}
	}

	/// <summary>
	/// Gets or sets the horizontal spacing between grids in pixels.
	/// </summary>
	public int GridHorizontalSpacing
	{
		get => _gridHorizontalSpacing;
		set
		{
			_gridHorizontalSpacing = value;
			ApplyColors();
			CalculateDimensions();
			RedrawAll();
		}
	}

	/// <summary>
	/// Gets or sets the number of rows (-1 for auto-fill height).
	/// </summary>
	public int NumRows
	{
		get => _numRows;
		set
		{
			_numRows = value;
			CalculateDimensions();
			if (_fileLoaded) UpdateScrollbar();
			RedrawAll();
		}
	}

	/// <summary>
	/// Gets or sets the number of columns (bytes per row, -1 for auto-fill width).
	/// </summary>
	public int NumColumns
	{
		get => _numCols;
		set
		{
			_numCols = value;
			CalculateDimensions();
			if (_fileLoaded)
			{
				_numLines = (_fileSize + _calculatedCols - 1) / _calculatedCols;
				UpdateScrollbar();
			}
			RedrawAll();
		}
	}

	/// <summary>
	/// Gets the current file size, or 0 if no file is loaded.
	/// </summary>
	public long FileSize => _fileSize;

	/// <summary>
	/// Gets whether a file is currently loaded.
	/// </summary>
	public bool IsFileLoaded => _fileLoaded;
}
