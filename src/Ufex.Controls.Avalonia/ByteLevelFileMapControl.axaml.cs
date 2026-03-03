using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Ufex.API.Visual;

namespace Ufex.Controls.Avalonia;

/// <summary>
/// Displays a byte-level file map where each pixel represents one byte in the file,
/// colored according to the label classification from a <see cref="ByteLevelFileMapVisual"/>.
/// Rendering is deferred until the user clicks the control to avoid unnecessary load.
/// </summary>
public partial class ByteLevelFileMapControl : UserControl
{
	private static readonly Color[] DefaultPalette =
	[
		Color.Parse("#4E79A7"), // Blue
		Color.Parse("#F28E2C"), // Orange
		Color.Parse("#E15759"), // Red
		Color.Parse("#76B7B2"), // Teal
		Color.Parse("#59A14F"), // Green
		Color.Parse("#EDC949"), // Yellow
		Color.Parse("#AF7AA1"), // Purple
		Color.Parse("#FF9DA7"), // Pink
		Color.Parse("#9C755F"), // Brown
		Color.Parse("#BAB0AB")  // Gray
	];

	/// <summary>
	/// Maximum number of bitmap rows to render. Files larger than this (in rows)
	/// will be truncated in the visual. Each row represents <c>BytesPerRow</c> bytes.
	/// </summary>
	private const int MaxBitmapHeight = 65536;

	/// <summary>Minimum display scale (1 pixel per byte).</summary>
	private const int MinDisplayScale = 1;

	/// <summary>Maximum display scale to avoid extreme zoom on tiny files.</summary>
	private const int MaxDisplayScale = 32;

	/// <summary>Width reserved for the offset axis column in layout-independent pixels.</summary>
	private const double OffsetAxisReservedWidth = 100;

	private static readonly int[] RowWidthOptions = [64, 128, 256, 512, 1024, 2048];
	private static readonly double[] ZoomOptions = [0.50, 0.75, 1.00, 1.50, 2.00];

	private WriteableBitmap? _bitmap;
	private OffsetAxisControl? _offsetAxis;
	private bool _isRendered;

	// UI elements resolved from AXAML
	private WrapPanel? _legendPanel;
	private ScrollViewer? _mapScrollViewer;
	private Image? _bitmapImage;
	private Grid? _mapGrid;
	private ComboBox? _rowWidthCombo;
	private ComboBox? _zoomCombo;

	public static readonly StyledProperty<ByteLevelFileMapVisual?> VisualProperty =
		AvaloniaProperty.Register<ByteLevelFileMapControl, ByteLevelFileMapVisual?>(nameof(Visual));

	public ByteLevelFileMapVisual? Visual
	{
		get => GetValue(VisualProperty);
		set => SetValue(VisualProperty, value);
	}

	public ByteLevelFileMapControl()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		global::Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);

		_legendPanel = this.FindControl<WrapPanel>("LegendPanel");
		_mapScrollViewer = this.FindControl<ScrollViewer>("MapScrollViewer");
		_bitmapImage = this.FindControl<Image>("BitmapImage");
		_mapGrid = this.FindControl<Grid>("MapGrid");
		_rowWidthCombo = this.FindControl<ComboBox>("RowWidthCombo");
		_zoomCombo = this.FindControl<ComboBox>("ZoomCombo");

		// Set NearestNeighbor interpolation so individual bytes appear as crisp pixels
		if (_bitmapImage != null)
		{
			RenderOptions.SetBitmapInterpolationMode(_bitmapImage, BitmapInterpolationMode.None);
		}

		// Wire up toolbar combos — re-render when changed
		if (_rowWidthCombo != null)
		{
			_rowWidthCombo.SelectionChanged += OnToolbarChanged;
		}
		if (_zoomCombo != null)
		{
			_zoomCombo.SelectionChanged += OnToolbarChanged;
		}
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == VisualProperty)
		{
			OnVisualChanged(change.NewValue as ByteLevelFileMapVisual);
		}
	}

	private void OnVisualChanged(ByteLevelFileMapVisual? visual)
	{
		// Reset state
		_isRendered = false;
		_bitmap?.Dispose();
		_bitmap = null;

		if (_bitmapImage != null)
			_bitmapImage.Source = null;

		// Remove old offset axis
		if (_offsetAxis != null && _mapGrid != null)
		{
			_mapGrid.Children.Remove(_offsetAxis);
			_offsetAxis = null;
		}

		if (visual == null)
		{
			return;
		}

		// Populate legend immediately (cheap)
		PopulateLegend(visual);

		// Sync toolbar to the visual's current BytesPerRow
		if (_rowWidthCombo != null)
		{
			int idx = Array.IndexOf(RowWidthOptions, visual.BytesPerRow);
			if (idx >= 0)
				_rowWidthCombo.SelectedIndex = idx;
		}

		// Auto-render immediately
		RenderBitmap(visual);
	}

	private void OnToolbarChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (!_isRendered || Visual == null)
			return;

		// Apply the new row width to the visual before re-rendering
		Visual.BytesPerRow = (ushort)GetSelectedRowWidth();

		// Force a full re-render with new settings
		_isRendered = false;
		RenderBitmap(Visual);
	}

	private int GetSelectedRowWidth()
	{
		int index = _rowWidthCombo?.SelectedIndex ?? 1;
		if (index >= 0 && index < RowWidthOptions.Length)
			return RowWidthOptions[index];
		return 128;
	}

	private double GetSelectedZoom()
	{
		int index = _zoomCombo?.SelectedIndex ?? 2;
		if (index >= 0 && index < ZoomOptions.Length)
			return ZoomOptions[index];
		return 1.0;
	}

	/// <summary>
	/// Populates the legend panel with colored rectangles and descriptions for each label.
	/// </summary>
	private void PopulateLegend(ByteLevelFileMapVisual visual)
	{
		if (_legendPanel == null)
			return;

		_legendPanel.Children.Clear();

		for (int i = 0; i < visual.Labels.Length; i++)
		{
			var label = visual.Labels[i];
			var color = ResolveColor(label.Color, i);

			var item = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Margin = new Thickness(0, 0, 16, 4)
			};

			item.Children.Add(new Border
			{
				Width = 14,
				Height = 14,
				Background = new SolidColorBrush(color),
				BorderBrush = Brushes.Gray,
				BorderThickness = new Thickness(1),
				CornerRadius = new CornerRadius(2),
				Margin = new Thickness(0, 0, 6, 0),
				VerticalAlignment = VerticalAlignment.Center
			});

			item.Children.Add(new TextBlock
			{
				Text = label.Description ?? $"Label {i}",
				VerticalAlignment = VerticalAlignment.Center,
				FontSize = 12
			});

			_legendPanel.Children.Add(item);
		}
	}

	/// <summary>
	/// Reads the label index stream and creates a WriteableBitmap representing the file map.
	/// The bitmap is always 1 pixel per byte at native resolution, then scaled up via the
	/// Image control's Stretch mode to fill the available viewport.
	/// </summary>
	private void RenderBitmap(ByteLevelFileMapVisual visual)
	{
		// Clean up previous render
		_bitmap?.Dispose();
		_bitmap = null;
		if (_bitmapImage != null)
			_bitmapImage.Source = null;
		if (_offsetAxis != null && _mapGrid != null)
		{
			_mapGrid.Children.Remove(_offsetAxis);
			_offsetAxis = null;
		}

		int bytesPerRow = visual.BytesPerRow;
		if (bytesPerRow <= 0)
			return;

		int totalRows = (int)Math.Min(
			(long)Math.Ceiling((double)visual.Size / bytesPerRow),
			MaxBitmapHeight);

		if (totalRows <= 0)
			return;

		// Resolve colors for each label index
		var labelColors = new Color[visual.Labels.Length];
		for (int i = 0; i < visual.Labels.Length; i++)
		{
			labelColors[i] = ResolveColor(visual.Labels[i].Color, i);
		}
		var defaultColor = Color.Parse("#333333");

		// Create bitmap at native resolution (1 pixel = 1 byte)
		var bitmap = new WriteableBitmap(
			new PixelSize(bytesPerRow, totalRows),
			new Vector(96, 96),
			global::Avalonia.Platform.PixelFormat.Bgra8888,
			global::Avalonia.Platform.AlphaFormat.Premul);

		using (var fb = bitmap.Lock())
		{
			int stride = fb.RowBytes;
			var rowPixels = new byte[stride];
			var labelBuffer = new byte[bytesPerRow];

			var stream = visual.LabelIndexStream;
			if (stream.CanSeek)
				stream.Position = 0;

			for (int row = 0; row < totalRows; row++)
			{
				int bytesRead = ReadFully(stream, labelBuffer, 0, bytesPerRow);

				// Clear row buffer
				Array.Clear(rowPixels, 0, rowPixels.Length);

				for (int col = 0; col < bytesPerRow; col++)
				{
					Color color;
					if (col < bytesRead)
					{
						int labelIndex = labelBuffer[col];
						color = labelIndex < labelColors.Length
							? labelColors[labelIndex]
							: defaultColor;
					}
					else
					{
						color = defaultColor;
					}

					int offset = col * 4;
					rowPixels[offset + 0] = color.B;
					rowPixels[offset + 1] = color.G;
					rowPixels[offset + 2] = color.R;
					rowPixels[offset + 3] = color.A;
				}

				Marshal.Copy(rowPixels, 0, IntPtr.Add(fb.Address, row * stride), bytesPerRow * 4);
			}
		}

		_bitmap = bitmap;

		// Compute base display scale, then apply zoom multiplier
		int baseScale = ComputeDisplayScale(bytesPerRow, totalRows);
		double zoom = GetSelectedZoom();
		double displayWidth = bytesPerRow * baseScale * zoom;
		double displayHeight = totalRows * baseScale * zoom;

		// Display the bitmap scaled up using Stretch.Fill so the native 1-px-per-byte
		// bitmap is stretched to the explicit Width/Height we set.
		if (_bitmapImage != null)
		{
			_bitmapImage.Stretch = Stretch.Fill;
			_bitmapImage.Source = bitmap;
			_bitmapImage.Width = displayWidth;
			_bitmapImage.Height = displayHeight;
		}

		// Create and add offset axis matching the bitmap display height
		double scaledRowHeight = baseScale * zoom;
		_offsetAxis = new OffsetAxisControl();
		_offsetAxis.Configure(totalRows, scaledRowHeight, (ushort)bytesPerRow);
		_offsetAxis.VerticalAlignment = VerticalAlignment.Top;
		Grid.SetColumn(_offsetAxis, 0);

		if (_mapGrid != null)
		{
			_mapGrid.Children.Insert(0, _offsetAxis);
		}

		_isRendered = true;
	}

	/// <summary>
	/// Computes an integer display scale so the bitmap fits the available viewport width
	/// (minus the offset axis) and doesn't exceed it vertically unless scrolling is needed.
	/// The bitmap is always 1 pixel per byte; this scale expands each pixel to NxN on screen.
	/// </summary>
	private int ComputeDisplayScale(int bitmapWidth, int bitmapHeight)
	{
		double availableWidth = _mapScrollViewer?.Bounds.Width ?? 800;
		double availableHeight = _mapScrollViewer?.Bounds.Height ?? 600;

		// Reserve space for the offset axis column
		availableWidth = Math.Max(availableWidth - OffsetAxisReservedWidth, 64);

		// Fit to width primarily (user scrolls vertically for long files)
		double scaleFromWidth = availableWidth / bitmapWidth;

		// Also consider height so small files don't become excessively tall
		double scaleFromHeight = availableHeight / bitmapHeight;

		// Use the smaller so the bitmap fits in both dimensions when possible,
		// but always at least 1:1
		double idealScale = Math.Min(scaleFromWidth, scaleFromHeight);
		int scale = Math.Max(MinDisplayScale, (int)Math.Floor(idealScale));
		scale = Math.Min(scale, MaxDisplayScale);

		return scale;
	}



	/// <summary>
	/// Resolves a color for a label, using the label's explicit color or falling back to the palette.
	/// </summary>
	private static Color ResolveColor(uint? labelColor, int index)
	{
		if (labelColor.HasValue)
			return Color.FromUInt32(labelColor.Value);

		return DefaultPalette[index % DefaultPalette.Length];
	}

	/// <summary>
	/// Reads exactly <paramref name="count"/> bytes from the stream, handling partial reads.
	/// </summary>
	private static int ReadFully(Stream stream, byte[] buffer, int offset, int count)
	{
		int totalRead = 0;
		while (totalRead < count)
		{
			int read = stream.Read(buffer, offset + totalRead, count - totalRead);
			if (read == 0)
				break;
			totalRead += read;
		}
		return totalRead;
	}

	private static string FormatSize(ulong bytes)
	{
		if (bytes < 1024)
			return $"{bytes} B";
		if (bytes < 1024 * 1024)
			return $"{bytes / 1024.0:F1} KB";
		if (bytes < 1024 * 1024 * 1024)
			return $"{bytes / (1024.0 * 1024.0):F1} MB";
		return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
	}

	/// <summary>
	/// Inner control that renders the file offset axis alongside the bitmap.
	/// Draws hexadecimal file offsets at regular vertical intervals.
	/// </summary>
	private class OffsetAxisControl : Control
	{
		private int _totalRows;
		private double _rowHeight;
		private ushort _bytesPerRow;

		private static readonly Typeface MonoTypeface =
			new(FontFamily.Parse("Consolas, Courier New, monospace"));

		private const double FontSize = 11;
		private const double AxisWidth = 84;
		private const double MinPixelsBetweenLabels = 18;
		private const double RightMargin = 8;

		public void Configure(int totalRows, double rowHeight, ushort bytesPerRow)
		{
			_totalRows = totalRows;
			_rowHeight = rowHeight;
			_bytesPerRow = bytesPerRow;

			Width = AxisWidth + RightMargin;
			Height = totalRows * rowHeight;
			InvalidateVisual();
		}

		public override void Render(DrawingContext context)
		{
			base.Render(context);

			if (_totalRows <= 0 || _rowHeight <= 0 || _bytesPerRow <= 0)
				return;

			var foreground = new SolidColorBrush(Color.Parse("#999999"));

			// Compute a row interval that produces readable, non-overlapping labels
			int minRowInterval = Math.Max(1, (int)Math.Ceiling(MinPixelsBetweenLabels / _rowHeight));

			// Round up to the nearest power of 2 for clean hex offsets
			int rowInterval = 1;
			while (rowInterval < minRowInterval)
				rowInterval *= 2;

			for (int row = 0; row < _totalRows; row += rowInterval)
			{
				ulong fileOffset = (ulong)row * _bytesPerRow;
				string text = $"0x{fileOffset:X8}";

				var formattedText = new FormattedText(
					text,
					CultureInfo.InvariantCulture,
					FlowDirection.LeftToRight,
					MonoTypeface,
					FontSize,
					foreground);

				double y = row * _rowHeight;
				context.DrawText(formattedText, new Point(0, y));
			}
		}
	}
}
