using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ufex.Hex;

namespace Ufex.Controls.Avalonia;

/// <summary>
/// Cross-platform hex viewer control for Avalonia.
/// Displays file data in three columns: position, hex values, and ASCII text.
/// Uses DrawingContext rendering for maximum scroll performance.
/// </summary>
public partial class HexView : UserControl
{
	// UI Elements
	private Grid? _mainGrid;
	private Border? _positionBorder;
	private Border? _hexBorder;
	private Border? _asciiBorder;
	private HexViewPanel? _positionPanel;
	private HexViewPanel? _hexPanel;
	private HexViewPanel? _asciiPanel;
	private ScrollBar? _scrollBar;

	// Font settings
	private readonly FontFamily _monoFont = new FontFamily("Courier New");
	private readonly double _fontSize = 13;
	private readonly double _cellHeight = 18;
	private readonly double _positionCellWidth = 72;
	private readonly double _hexCellWidth = 26;
	private readonly double _asciiCellWidth = 14;

	// Cached typeface for DrawingContext rendering
	private Typeface _typeface;

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
	private HexBuffer? _hexBuffer;
	private int _bufferSize = 262144; // 256KB buffer

	// Current display state
	private long _displayPosition;
	private long _numLines;

	// Highlight support
	private long _highlightStart;
	private long _highlightEnd;

	// Content width tracking
	private double _contentWidth;

	// Scroll debounce timer
	private DispatcherTimer? _scrollDebounceTimer;

	// Track whether data columns have content to draw
	private bool _dataAvailable;

	// Selection state
	private long _selectionStart = -1;
	private long _selectionEnd = -1;
	private long _selectionAnchor = -1;
	private bool _isSelecting;

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

	// Color profile support
	private ColorProfile? _colorProfile;
	private ColorProfileEvaluator? _colorEvaluator;
	private UInt32[]? _colorCache;
	private long _colorCacheStart = -1;
	private int _colorCacheCount;

	public HexView()
	{
		_typeface = new Typeface(_monoFont);
		Focusable = true;
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
		_positionPanel = this.FindControl<HexViewPanel>("PositionPanel");
		_hexPanel = this.FindControl<HexViewPanel>("HexPanel");
		_asciiPanel = this.FindControl<HexViewPanel>("AsciiPanel");
		_scrollBar = this.FindControl<ScrollBar>("VerticalScrollBar");

		// Assign render callbacks
		if (_positionPanel != null)
			_positionPanel.RenderCallback = RenderPositionColumn;
		if (_hexPanel != null)
		{
			_hexPanel.RenderCallback = RenderHexColumn;
			_hexPanel.PointerPressed += OnHexPanelPointerPressed;
			_hexPanel.PointerMoved += OnHexPanelPointerMoved;
			_hexPanel.PointerReleased += OnHexPanelPointerReleased;
		}
		if (_asciiPanel != null)
		{
			_asciiPanel.RenderCallback = RenderAsciiColumn;
			_asciiPanel.PointerPressed += OnAsciiPanelPointerPressed;
			_asciiPanel.PointerMoved += OnAsciiPanelPointerMoved;
			_asciiPanel.PointerReleased += OnAsciiPanelPointerReleased;
		}

		if (_scrollBar != null)
		{
			_scrollBar.PropertyChanged += OnScrollBarPropertyChanged;
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
		RepaintAll();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (!_fileLoaded || _scrollBar == null)
		{
			base.OnKeyDown(e);
			return;
		}

		switch (e.Key)
		{
			case Key.PageDown:
				_scrollBar.Value = Math.Min(_scrollBar.Value + _calculatedRows, _scrollBar.Maximum);
				e.Handled = true;
				break;
			case Key.PageUp:
				_scrollBar.Value = Math.Max(_scrollBar.Value - _calculatedRows, _scrollBar.Minimum);
				e.Handled = true;
				break;
			case Key.Home:
				_scrollBar.Value = _scrollBar.Minimum;
				e.Handled = true;
				break;
			case Key.End:
				_scrollBar.Value = _scrollBar.Maximum;
				e.Handled = true;
				break;
			case Key.Down:
				_scrollBar.Value = Math.Min(_scrollBar.Value + 1, _scrollBar.Maximum);
				e.Handled = true;
				break;
			case Key.Up:
				_scrollBar.Value = Math.Max(_scrollBar.Value - 1, _scrollBar.Minimum);
				e.Handled = true;
				break;
			case Key.C when e.KeyModifiers.HasFlag(KeyModifiers.Control):
				CopySelectionToClipboard();
				e.Handled = true;
				break;
			case Key.A when e.KeyModifiers.HasFlag(KeyModifiers.Control):
				SelectAll();
				e.Handled = true;
				break;
			case Key.Escape:
				ClearSelection();
				e.Handled = true;
				break;
			default:
				base.OnKeyDown(e);
				break;
		}
	}

	private void OnActualThemeVariantChanged(object? sender, EventArgs e)
	{
		// Update color profile evaluator with new theme
		if (_colorProfile != null)
		{
			string? theme = GetCurrentThemeName();
			_colorEvaluator = new ColorProfileEvaluator(_colorProfile, theme);
			InvalidateColorCache();
		}

		ApplyColors();
		RepaintAll();
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

		UpdatePanelSizes();
	}

	private void UpdatePanelSizes()
	{
		double canvasHeight = _calculatedRows * _cellHeight;

		if (_positionPanel != null)
		{
			_positionPanel.Width = _positionCellWidth;
			_positionPanel.Height = canvasHeight;
		}
		if (_hexPanel != null)
		{
			_hexPanel.Width = _calculatedCols * _hexCellWidth;
			_hexPanel.Height = canvasHeight;
		}
		if (_asciiPanel != null)
		{
			_asciiPanel.Width = _calculatedCols * _asciiCellWidth;
			_asciiPanel.Height = canvasHeight;
		}

		// Calculate and notify content width
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

		_scrollDebounceTimer?.Stop();
		_fileStream = null;
		_fileLoaded = false;
		_hexBuffer = null;
		_fileSize = 0;
		_displayPosition = 0;
		_dataAvailable = false;
		_highlightStart = 0;
		_highlightEnd = 0;
		_selectionStart = -1;
		_selectionEnd = -1;
		_selectionAnchor = -1;
		_isSelecting = false;

		if (_scrollBar != null)
		{
			_scrollBar.IsEnabled = false;
		}

		InvalidateColorCache();
		RepaintAll();
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
		_hexBuffer = new HexBuffer(_fileStream, _bufferSize);
		_hexBuffer.Load(0, _calculatedCols);
		_dataAvailable = true;

		UpdateScrollbar();
		RepaintAll();
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
		if (_fileStream == null || _hexBuffer == null) return;
		_hexBuffer.Load(position, _calculatedCols);
	}

	private void OnScrollBarPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (e.Property != RangeBase.ValueProperty) return;
		if (!_fileLoaded) return;

		long newRow = (long)(double)e.NewValue!;
		long newPosition = newRow * _calculatedCols;

		if (newPosition == _displayPosition) return;

		_displayPosition = newPosition;

		// Check if display data is within the current buffer
		long displayEnd = _displayPosition + (_calculatedRows * _calculatedCols);
		bool inBuffer = _hexBuffer != null &&
		                _hexBuffer.Contains(_displayPosition, (int)(displayEnd - _displayPosition));

		if (inBuffer)
		{
			_scrollDebounceTimer?.Stop();
			_dataAvailable = true;
			InvalidateColorCache();
			RepaintAll();
		}
		else
		{
			// Outside buffer - repaint with positions only (data blanked)
			_dataAvailable = false;
			RepaintAll();
			StartScrollDebounce();
		}
	}

	/// <summary>
	/// Triggers InvalidateVisual on all three panels.
	/// This is extremely cheap â€” it just marks them dirty for the next render pass.
	/// </summary>
	private void RepaintAll()
	{
		_positionPanel?.Repaint();
		_hexPanel?.Repaint();
		_asciiPanel?.Repaint();
	}

	// ========================================================================
	// Selection / mouse handling
	// ========================================================================

	private IBrush GetSelectionBrush()
	{
		if (_useThemeColors && this.TryFindResource("SystemAccentColor", ActualThemeVariant, out var resource) && resource is Color accentColor)
		{
			return new SolidColorBrush(Color.FromArgb(100, accentColor.R, accentColor.G, accentColor.B));
		}
		return new SolidColorBrush(Color.FromArgb(100, 0, 120, 215));
	}

	private long HitTestHexPanel(Point pos)
	{
		int col = (int)(pos.X / _hexCellWidth);
		int row = (int)(pos.Y / _cellHeight);
		if (col < 0) col = 0;
		if (col >= _calculatedCols) col = _calculatedCols - 1;
		if (row < 0) row = 0;
		if (row >= _calculatedRows) row = _calculatedRows - 1;
		long filePos = _displayPosition + (row * _calculatedCols) + col;
		return Math.Min(filePos, _fileSize - 1);
	}

	private long HitTestAsciiPanel(Point pos)
	{
		int col = (int)(pos.X / _asciiCellWidth);
		int row = (int)(pos.Y / _cellHeight);
		if (col < 0) col = 0;
		if (col >= _calculatedCols) col = _calculatedCols - 1;
		if (row < 0) row = 0;
		if (row >= _calculatedRows) row = _calculatedRows - 1;
		long filePos = _displayPosition + (row * _calculatedCols) + col;
		return Math.Min(filePos, _fileSize - 1);
	}

	private void BeginSelection(long filePos)
	{
		_selectionAnchor = filePos;
		_selectionStart = filePos;
		_selectionEnd = filePos;
		_isSelecting = true;
		// Clear search highlight when user makes a selection
		_highlightStart = 0;
		_highlightEnd = 0;
		Focus();
		RepaintAll();
	}

	private void UpdateSelection(long filePos)
	{
		if (!_isSelecting) return;
		_selectionStart = Math.Min(_selectionAnchor, filePos);
		_selectionEnd = Math.Max(_selectionAnchor, filePos);
		RepaintAll();
	}

	private void EndSelection()
	{
		_isSelecting = false;
	}

	private void OnHexPanelPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (!_fileLoaded || !_dataAvailable) return;
		var props = e.GetCurrentPoint(_hexPanel).Properties;
		if (!props.IsLeftButtonPressed) return;
		long filePos = HitTestHexPanel(e.GetPosition(_hexPanel));
		BeginSelection(filePos);
		e.Handled = true;
	}

	private void OnHexPanelPointerMoved(object? sender, PointerEventArgs e)
	{
		if (!_isSelecting || _hexPanel == null) return;
		long filePos = HitTestHexPanel(e.GetPosition(_hexPanel));
		UpdateSelection(filePos);
		e.Handled = true;
	}

	private void OnHexPanelPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (!_isSelecting) return;
		EndSelection();
		e.Handled = true;
	}

	private void OnAsciiPanelPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (!_fileLoaded || !_dataAvailable) return;
		var props = e.GetCurrentPoint(_asciiPanel).Properties;
		if (!props.IsLeftButtonPressed) return;
		long filePos = HitTestAsciiPanel(e.GetPosition(_asciiPanel));
		BeginSelection(filePos);
		e.Handled = true;
	}

	private void OnAsciiPanelPointerMoved(object? sender, PointerEventArgs e)
	{
		if (!_isSelecting || _asciiPanel == null) return;
		long filePos = HitTestAsciiPanel(e.GetPosition(_asciiPanel));
		UpdateSelection(filePos);
		e.Handled = true;
	}

	private void OnAsciiPanelPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (!_isSelecting) return;
		EndSelection();
		e.Handled = true;
	}

	private bool IsSelected(long filePos)
	{
		return _selectionStart >= 0 && filePos >= _selectionStart && filePos <= _selectionEnd;
	}

	/// <summary>
	/// Clears the current selection.
	/// </summary>
	public void ClearSelection()
	{
		_selectionStart = -1;
		_selectionEnd = -1;
		_selectionAnchor = -1;
		_isSelecting = false;
		RepaintAll();
	}

	/// <summary>
	/// Selects all bytes in the file.
	/// </summary>
	public void SelectAll()
	{
		if (!_fileLoaded) return;
		_selectionStart = 0;
		_selectionEnd = _fileSize - 1;
		_selectionAnchor = 0;
		RepaintAll();
	}

	/// <summary>
	/// Copies the selected hex bytes to the clipboard as space-separated uppercase hex.
	/// </summary>
	public async void CopySelectionToClipboard()
	{
		if (_selectionStart < 0 || _fileStream == null) return;

		long count = _selectionEnd - _selectionStart + 1;
		// Limit copy size to prevent huge clipboard operations
		if (count > 1_000_000) count = 1_000_000;

		var sb = new StringBuilder((int)(count * 3));
		long pos = _selectionStart;

		// Try to read from buffer first, fall back to file
		for (long i = 0; i < count; i++)
		{
			long filePos = pos + i;
			if (filePos >= _fileSize) break;

			int value;
			if (_hexBuffer != null)
				value = _hexBuffer.ReadByte(filePos);
			else
				value = -1;

			if (value < 0)
			{
				// Need to read from file
				lock (_fileStream)
				{
					_fileStream.Seek(filePos, SeekOrigin.Begin);
					int b = _fileStream.ReadByte();
					if (b < 0) break;
					value = b;
				}
			}

			if (i > 0) sb.Append(' ');
			sb.Append(HexFormat.HexUpper[value]);
		}

		var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
		if (clipboard != null)
		{
			await clipboard.SetTextAsync(sb.ToString());
		}
	}

	/// <summary>
	/// Gets the current selection range, or (-1, -1) if nothing is selected.
	/// </summary>
	public (long Start, long End) GetSelection()
	{
		return (_selectionStart, _selectionEnd);
	}

	// ========================================================================
	// DrawingContext rendering methods
	// ========================================================================

	/// <summary>
	/// Computes a highlight rect that is inset by 1px on sides where the adjacent cell
	/// is not part of the same highlight range, so the highlight doesn't paint over borders
	/// at the edges of a match.
	/// </summary>
	private Rect GetInsetRect(long filePos, long rangeStart, long rangeEnd, double x, double y, double cellWidth, double cellHeight, int col)
	{
		double insetLeft = 0, insetRight = 0, insetBottom = 0;

		// Left edge: no highlighted neighbor to the left on the same row
		if (col == 0 || filePos - 1 < rangeStart)
			insetLeft = 1;

		// Right edge: no highlighted neighbor to the right on the same row
		if (col == _calculatedCols - 1 || filePos + 1 > rangeEnd)
			insetRight = 1;

		// Bottom edge: no highlighted neighbor below
		if (filePos + _calculatedCols > rangeEnd)
			insetBottom = 1;

		return new Rect(
			x + insetLeft,
			y,
			cellWidth - insetLeft - insetRight,
			cellHeight - insetBottom);
	}

	private void RenderPositionColumn(DrawingContext context)
	{
		if (!_fileLoaded) return;

		var textBrush = GetTextBrush();
		var cellBorderPen = new Pen(GetCellBorderBrush(), 1);

		for (int row = 0; row < _calculatedRows; row++)
		{
			long address = _displayPosition + (row * _calculatedCols);
			if (address >= _fileSize) break;

			double y = row * _cellHeight;

			// Draw row bottom border
			context.DrawLine(cellBorderPen, new Point(0, y + _cellHeight - 0.5), new Point(_positionCellWidth, y + _cellHeight - 0.5));

			// Draw position text (right-aligned)
			string posText = address.ToString(_hexCaps ? "X8" : "x8");
			var formattedText = new FormattedText(
				posText,
				CultureInfo.InvariantCulture,
				FlowDirection.LeftToRight,
				_typeface,
				_fontSize,
				textBrush);

			double textX = _positionCellWidth - formattedText.Width - 4;
			double textY = y + (_cellHeight - formattedText.Height) / 2;
			context.DrawText(formattedText, new Point(textX, textY));
		}
	}

	private void RenderHexColumn(DrawingContext context)
	{
		if (!_fileLoaded) return;

		var cellBorderPen = new Pen(GetCellBorderBrush(), 1);
		double totalWidth = _calculatedCols * _hexCellWidth;

		// Draw grid lines first (always visible)
		for (int row = 0; row < _calculatedRows; row++)
		{
			long address = _displayPosition + (row * _calculatedCols);
			if (address >= _fileSize) break;

			double y = row * _cellHeight;

			// Row bottom border
			context.DrawLine(cellBorderPen, new Point(0, y + _cellHeight - 0.5), new Point(totalWidth, y + _cellHeight - 0.5));
		}

		// Column right borders (draw full height once)
		int visibleRows = Math.Min(_calculatedRows, (int)((_fileSize - _displayPosition + _calculatedCols - 1) / _calculatedCols));
		double gridHeight = visibleRows * _cellHeight;
		for (int col = 0; col < _calculatedCols; col++)
		{
			double x = (col + 1) * _hexCellWidth - 0.5;
			context.DrawLine(cellBorderPen, new Point(x, 0), new Point(x, gridHeight));
		}

		// Draw data text only if buffer data is available
		if (!_dataAvailable || _hexBuffer == null) return;

		var textBrush = GetTextBrush();
		var highlightBrush = new SolidColorBrush(Colors.Yellow);
		var highlightTextBrush = new SolidColorBrush(Colors.Black);
		var selectionBrush = GetSelectionBrush();
		bool hasHighlight = _highlightStart != _highlightEnd;
		bool hasSelection = _selectionStart >= 0;
		string[] hexTable = _hexCaps ? HexFormat.HexUpper : HexFormat.HexLower;

		// Evaluate color profile for the displayed range
		EnsureColorCache();

		for (int row = 0; row < _calculatedRows; row++)
		{
			for (int col = 0; col < _calculatedCols; col++)
			{
				long filePos = _displayPosition + (row * _calculatedCols) + col;
				if (filePos >= _fileSize) return;

				long bufferOffset = _hexBuffer.GetBufferOffset(filePos);
				if (bufferOffset < 0) continue;

				byte value = _hexBuffer.Data[bufferOffset];
				bool isHighlighted = hasHighlight && filePos >= _highlightStart && filePos <= _highlightEnd;
				bool isSelected = hasSelection && filePos >= _selectionStart && filePos <= _selectionEnd;

				double x = col * _hexCellWidth;
				double y = row * _cellHeight;

				// Draw highlight background (search takes visual priority)
				if (isHighlighted)
				{
					var highlightRect = GetInsetRect(filePos, _highlightStart, _highlightEnd, x, y, _hexCellWidth, _cellHeight, col);
					context.FillRectangle(highlightBrush, highlightRect);
				}
				else if (isSelected)
				{
					context.FillRectangle(selectionBrush, new Rect(x, y, _hexCellWidth, _cellHeight));
				}

				// Determine text brush: highlight > color profile > default
				IBrush cellBrush;
				if (isHighlighted)
					cellBrush = highlightTextBrush;
				else
					cellBrush = GetColorProfileBrush(filePos) ?? textBrush;

				// Draw hex text (centered)
				var formattedText = new FormattedText(
					hexTable[value],
					CultureInfo.InvariantCulture,
					FlowDirection.LeftToRight,
					_typeface,
					_fontSize,
					cellBrush);

				double textX = x + (_hexCellWidth - formattedText.Width) / 2;
				double textY = y + (_cellHeight - formattedText.Height) / 2;
				context.DrawText(formattedText, new Point(textX, textY));
			}
		}
	}

	private void RenderAsciiColumn(DrawingContext context)
	{
		if (!_fileLoaded) return;

		var cellBorderPen = new Pen(GetCellBorderBrush(), 1);
		double totalWidth = _calculatedCols * _asciiCellWidth;

		// Draw grid lines first (always visible)
		for (int row = 0; row < _calculatedRows; row++)
		{
			long address = _displayPosition + (row * _calculatedCols);
			if (address >= _fileSize) break;

			double y = row * _cellHeight;

			// Row bottom border
			context.DrawLine(cellBorderPen, new Point(0, y + _cellHeight - 0.5), new Point(totalWidth, y + _cellHeight - 0.5));
		}

		// Column right borders (draw full height once)
		int visibleRows = Math.Min(_calculatedRows, (int)((_fileSize - _displayPosition + _calculatedCols - 1) / _calculatedCols));
		double gridHeight = visibleRows * _cellHeight;
		for (int col = 0; col < _calculatedCols; col++)
		{
			double x = (col + 1) * _asciiCellWidth - 0.5;
			context.DrawLine(cellBorderPen, new Point(x, 0), new Point(x, gridHeight));
		}

		// Draw data text only if buffer data is available
		if (!_dataAvailable || _hexBuffer == null) return;

		var textBrush = GetTextBrush();
		var highlightBrush = new SolidColorBrush(Colors.Yellow);
		var highlightTextBrush = new SolidColorBrush(Colors.Black);
		var selectionBrush = GetSelectionBrush();
		bool hasHighlight = _highlightStart != _highlightEnd;
		bool hasSelection = _selectionStart >= 0;

		// Color cache is already populated by hex column render; reuse it

		for (int row = 0; row < _calculatedRows; row++)
		{
			for (int col = 0; col < _calculatedCols; col++)
			{
				long filePos = _displayPosition + (row * _calculatedCols) + col;
				if (filePos >= _fileSize) return;

				long bufferOffset = _hexBuffer.GetBufferOffset(filePos);
				if (bufferOffset < 0) continue;

				byte value = _hexBuffer.Data[bufferOffset];
				bool isHighlighted = hasHighlight && filePos >= _highlightStart && filePos <= _highlightEnd;
				bool isSelected = hasSelection && filePos >= _selectionStart && filePos <= _selectionEnd;

				double x = col * _asciiCellWidth;
				double y = row * _cellHeight;

				// Draw highlight background (search takes visual priority)
				if (isHighlighted)
				{
					var highlightRect = GetInsetRect(filePos, _highlightStart, _highlightEnd, x, y, _asciiCellWidth, _cellHeight, col);
					context.FillRectangle(highlightBrush, highlightRect);
				}
				else if (isSelected)
				{
					context.FillRectangle(selectionBrush, new Rect(x, y, _asciiCellWidth, _cellHeight));
				}

				// Determine text brush: highlight > color profile > default
				IBrush cellBrush;
				if (isHighlighted)
					cellBrush = highlightTextBrush;
				else
					cellBrush = GetColorProfileBrush(filePos) ?? textBrush;

				// Draw ASCII text (centered)
				var formattedText = new FormattedText(
					HexFormat.AsciiChars[value],
					CultureInfo.InvariantCulture,
					FlowDirection.LeftToRight,
					_typeface,
					_fontSize,
					cellBrush);

				double textX = x + (_asciiCellWidth - formattedText.Width) / 2;
				double textY = y + (_cellHeight - formattedText.Height) / 2;
				context.DrawText(formattedText, new Point(textX, textY));
			}
		}
	}

	// ========================================================================
	// Scroll debounce for async buffer loading
	// ========================================================================

	private void StartScrollDebounce()
	{
		if (_scrollDebounceTimer == null)
		{
			_scrollDebounceTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(50)
			};
			_scrollDebounceTimer.Tick += OnScrollDebounceTimer;
		}
		_scrollDebounceTimer.Stop();
		_scrollDebounceTimer.Start();
	}

	private void OnScrollDebounceTimer(object? sender, EventArgs e)
	{
		_scrollDebounceTimer?.Stop();

		if (!_fileLoaded) return;

		long position = _displayPosition;
		LoadBufferAsync(position);
	}

	private async void LoadBufferAsync(long position)
	{
		if (_fileStream == null || _hexBuffer == null) return;

		await _hexBuffer.LoadAsync(position, _calculatedCols);

		if (!_fileLoaded) return;

		_dataAvailable = true;
		InvalidateColorCache();
		RepaintAll();
	}

	// ========================================================================
	// Public API
	// ========================================================================

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
		if (_hexBuffer == null || !_hexBuffer.Contains(_displayPosition, (int)(displayEnd - _displayPosition)))
		{
			LoadBuffer(_displayPosition);
		}

		_dataAvailable = true;
		RepaintAll();
	}

	/// <summary>
	/// Highlights a range of bytes in the display.
	/// </summary>
	public void SetHighlight(long startPosition, long endPosition)
	{
		_highlightStart = Math.Min(startPosition, endPosition);
		_highlightEnd = Math.Max(startPosition, endPosition);
		RepaintAll();
	}

	/// <summary>
	/// Clears any highlight.
	/// </summary>
	public void ClearHighlight()
	{
		_highlightStart = 0;
		_highlightEnd = 0;
		RepaintAll();
	}

	// Search API

	/// <summary>
	/// Searches the entire loaded file for the given byte pattern.
	/// Returns a HexSearchState containing all match positions.
	/// Navigates to and highlights the first match if found.
	/// </summary>
	public HexSearchState Find(byte[] pattern)
	{
		if (!_fileLoaded || _fileStream == null || pattern.Length == 0)
			return new HexSearchState(pattern);

		var state = HexSearch.Find(_fileStream, pattern, _bufferSize);

		if (state.TotalResults > 0)
			NavigateToMatch(state);

		return state;
	}

	/// <summary>
	/// Searches the entire loaded file for the given byte pattern (case-insensitive).
	/// Each byte in the pattern is compared ignoring ASCII case.
	/// Returns a HexSearchState containing all match positions.
	/// </summary>
	public HexSearchState FindCaseInsensitive(byte[] pattern)
	{
		if (!_fileLoaded || _fileStream == null || pattern.Length == 0)
			return new HexSearchState(pattern);

		var state = HexSearch.FindCaseInsensitive(_fileStream, pattern, _bufferSize);

		if (state.TotalResults > 0)
			NavigateToMatch(state);

		return state;
	}

	/// <summary>
	/// Navigates to the next search result (wraps around at end).
	/// </summary>
	public void FindNext(HexSearchState state)
	{
		if (state.TotalResults == 0) return;

		HexSearch.MoveNext(state);
		NavigateToMatch(state);
	}

	/// <summary>
	/// Navigates to the previous search result (wraps around at start).
	/// </summary>
	public void FindPrevious(HexSearchState state)
	{
		if (state.TotalResults == 0) return;

		HexSearch.MovePrevious(state);
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
			RepaintAll();
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
			RepaintAll();
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
			RepaintAll();
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
			RepaintAll();
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
			RepaintAll();
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
			RepaintAll();
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

	/// <summary>
	/// Gets or sets the active color profile. Set to null to disable coloring.
	/// </summary>
	public ColorProfile? ActiveColorProfile
	{
		get => _colorProfile;
		set
		{
			_colorProfile = value;
			if (value != null)
			{
				string? theme = GetCurrentThemeName();
				_colorEvaluator = new ColorProfileEvaluator(value, theme);
			}
			else
			{
				_colorEvaluator = null;
			}
			InvalidateColorCache();
			RepaintAll();
		}
	}

	private string? GetCurrentThemeName()
	{
		var variant = ActualThemeVariant;
		if (variant == ThemeVariant.Light) return "light";
		if (variant == ThemeVariant.Dark) return "dark";
		// For Default/auto, try to detect actual theme
		if (variant == ThemeVariant.Default)
		{
			// Avalonia Default follows system; check background luminance heuristic
			// Fall back to "light" if we can't determine
			return null;
		}
		return null;
	}

	private void InvalidateColorCache()
	{
		_colorCache = null;
		_colorCacheStart = -1;
		_colorCacheCount = 0;
	}

	private void EnsureColorCache()
	{
		if (_colorEvaluator == null || _hexBuffer == null || !_dataAvailable)
		{
			_colorCache = null;
			return;
		}

		int displayCount = _calculatedRows * _calculatedCols;
		long displayEnd = Math.Min(_displayPosition + displayCount, _fileSize);
		int actualCount = (int)(displayEnd - _displayPosition);

		// Check if cache is still valid
		if (_colorCache != null && _colorCacheStart == _displayPosition && _colorCacheCount == actualCount)
			return;

		// Build the context buffer: 128 bytes before + display bytes + 128 bytes after
		const int ContextPadding = 128;
		long contextStart = Math.Max(0, _displayPosition - ContextPadding);
		long contextEnd = Math.Min(_fileSize, displayEnd + ContextPadding);
		int contextSize = (int)(contextEnd - contextStart);
		int displayOffset = (int)(_displayPosition - contextStart);

		// Read context bytes from the hex buffer
		byte[] contextBuffer = new byte[contextSize];
		int filled = 0;
		for (int i = 0; i < contextSize; i++)
		{
			long filePos = contextStart + i;
			long bufOffset = _hexBuffer.GetBufferOffset(filePos);
			if (bufOffset >= 0 && bufOffset < _hexBuffer.ValidBytes)
			{
				contextBuffer[i] = _hexBuffer.Data[bufOffset];
				filled++;
			}
		}

		if (filled == 0)
		{
			_colorCache = null;
			return;
		}

		_colorCache = _colorEvaluator.Evaluate(contextBuffer, displayOffset, actualCount);
		_colorCacheStart = _displayPosition;
		_colorCacheCount = actualCount;
	}

	private IBrush? GetColorProfileBrush(long filePos)
	{
		if (_colorCache == null) return null;

		int index = (int)(filePos - _colorCacheStart);
		if (index < 0 || index >= _colorCacheCount) return null;

		UInt32 color = _colorCache[index];
		if (color == 0) return null;

		// Color is RRGGBBAA
		byte r = (byte)((color >> 24) & 0xFF);
		byte g = (byte)((color >> 16) & 0xFF);
		byte b = (byte)((color >> 8) & 0xFF);
		byte a = (byte)(color & 0xFF);
		return new SolidColorBrush(Color.FromArgb(a, r, g, b));
	}
}
