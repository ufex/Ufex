using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Ufex.API.Visual;

namespace Ufex.Controls.Avalonia;

public partial class ImageViewerControl : UserControl
{
	private double _zoomLevel = 1.0;
	private const double ZoomStep = 0.25;
	private const double MinZoom = 0.1;
	private const double MaxZoom = 10.0;

	private Image? _rasterImageDisplay;
	private global::Avalonia.Svg.Skia.Svg? _svgImageDisplay;
	private TextBlock? _zoomLevelText;
	private Panel? _imageContainer;

	private global::Avalonia.Media.Imaging.Bitmap? _currentBitmap;
	private Size _originalImageSize;
	private bool _isRasterImage;

	public static readonly StyledProperty<Ufex.API.Visual.ImageVisual?> SourceImageProperty =
		AvaloniaProperty.Register<ImageViewerControl, Ufex.API.Visual.ImageVisual?>(nameof(SourceImage));

	public Ufex.API.Visual.ImageVisual? SourceImage
	{
		get => GetValue(SourceImageProperty);
		set => SetValue(SourceImageProperty, value);
	}

	public ImageViewerControl()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		global::Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);

		_rasterImageDisplay = this.FindControl<Image>("RasterImageDisplay");
		_svgImageDisplay = this.FindControl<global::Avalonia.Svg.Skia.Svg>("SvgImageDisplay");
		_zoomLevelText = this.FindControl<TextBlock>("ZoomLevelText");
		_imageContainer = this.FindControl<Panel>("ImageContainer");
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == SourceImageProperty)
		{
			LoadImage(change.NewValue as Ufex.API.Visual.ImageVisual);
		}
	}

	private void LoadImage(Ufex.API.Visual.ImageVisual? image)
	{
		// Clear previous
		if (_rasterImageDisplay != null)
		{
			_rasterImageDisplay.Source = null;
			_rasterImageDisplay.IsVisible = false;
		}
		if (_svgImageDisplay != null)
		{
			_svgImageDisplay.Path = null;
			_svgImageDisplay.IsVisible = false;
		}
		_currentBitmap?.Dispose();
		_currentBitmap = null;

		if (image == null)
			return;

		_zoomLevel = 1.0;

		if (image is RasterImage rasterImage)
		{
			LoadRasterImage(rasterImage);
		}
		else if (image is VectorImage vectorImage)
		{
			LoadVectorImage(vectorImage);
		}

		UpdateZoomDisplay();
	}

	private void LoadRasterImage(RasterImage rasterImage)
	{
		if (_rasterImageDisplay == null)
			return;

		try
		{
			var stream = rasterImage.ImageStream;
			if (stream.CanSeek)
				stream.Position = 0;

			_currentBitmap = new Bitmap(stream);
			_originalImageSize = new Size(_currentBitmap.PixelSize.Width, _currentBitmap.PixelSize.Height);
			_rasterImageDisplay.Source = _currentBitmap;
			_rasterImageDisplay.IsVisible = true;
			_isRasterImage = true;

			ApplyZoom();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error loading raster image: {ex.Message}");
		}
	}

	private void LoadVectorImage(VectorImage vectorImage)
	{
		if (_svgImageDisplay == null)
			return;

		try
		{
			var stream = vectorImage.SvgStream;
			if (stream.CanSeek)
				stream.Position = 0;

			// Read SVG content to a temporary file or use a data URI
			using var reader = new StreamReader(stream, leaveOpen: true);
			string svgContent = reader.ReadToEnd();

			// Reset stream position for potential re-use
			if (stream.CanSeek)
				stream.Position = 0;

			// Create a temporary file for the SVG
			string tempPath = Path.Combine(Path.GetTempPath(), $"ufex_svg_{Guid.NewGuid()}.svg");
			File.WriteAllText(tempPath, svgContent);

			_svgImageDisplay.Path = tempPath;
			_svgImageDisplay.IsVisible = true;
			_isRasterImage = false;

			// Get SVG size if available
			_originalImageSize = new Size(
				vectorImage.Width > 0 ? vectorImage.Width : 400,
				vectorImage.Height > 0 ? vectorImage.Height : 400);

			ApplyZoom();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error loading vector image: {ex.Message}");
		}
	}

	private void ApplyZoom()
	{
		double newWidth = _originalImageSize.Width * _zoomLevel;
		double newHeight = _originalImageSize.Height * _zoomLevel;

		if (_isRasterImage && _rasterImageDisplay != null)
		{
			_rasterImageDisplay.Width = newWidth;
			_rasterImageDisplay.Height = newHeight;
		}
		else if (_svgImageDisplay != null)
		{
			_svgImageDisplay.Width = newWidth;
			_svgImageDisplay.Height = newHeight;
		}

		UpdateZoomDisplay();
	}

	private void UpdateZoomDisplay()
	{
		if (_zoomLevelText != null)
		{
			_zoomLevelText.Text = $"{(int)(_zoomLevel * 100)}%";
		}
	}

	private void OnZoomInClick(object? sender, RoutedEventArgs e)
	{
		_zoomLevel = Math.Min(_zoomLevel + ZoomStep, MaxZoom);
		ApplyZoom();
	}

	private void OnZoomOutClick(object? sender, RoutedEventArgs e)
	{
		_zoomLevel = Math.Max(_zoomLevel - ZoomStep, MinZoom);
		ApplyZoom();
	}

	private void OnZoomFitClick(object? sender, RoutedEventArgs e)
	{
		var scrollViewer = this.FindControl<ScrollViewer>("ImageScrollViewer");
		if (scrollViewer == null || _originalImageSize.Width == 0 || _originalImageSize.Height == 0)
			return;

		double availableWidth = scrollViewer.Bounds.Width - 20; // Account for scrollbar
		double availableHeight = scrollViewer.Bounds.Height - 20;

		double scaleX = availableWidth / _originalImageSize.Width;
		double scaleY = availableHeight / _originalImageSize.Height;

		_zoomLevel = Math.Min(scaleX, scaleY);
		_zoomLevel = Math.Max(MinZoom, Math.Min(MaxZoom, _zoomLevel));

		ApplyZoom();
	}

	private void OnZoomResetClick(object? sender, RoutedEventArgs e)
	{
		_zoomLevel = 1.0;
		ApplyZoom();
	}

	public void Clear()
	{
		SourceImage = null;
	}
}
