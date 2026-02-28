using Avalonia.Controls;
using Avalonia.Layout;
using Ufex.API.Visual;
using Ufex.Controls.Avalonia;

namespace Ufex.Desktop.Views;

public partial class VisualTabView : UserControl
{
	private TabControl? _visualsTabControl;
	private TextBlock? _emptyMessage;

	public VisualTabView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		global::Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
		_visualsTabControl = this.FindControl<TabControl>("VisualsTabControl");
		_emptyMessage = this.FindControl<TextBlock>("EmptyMessage");
	}

	/// <summary>
	/// Loads an array of Visual objects and displays them as tabs.
	/// </summary>
	public void LoadVisuals(Visual[] visuals, long fileSize = 0)
	{
		Clear();

		if (visuals == null || visuals.Length == 0)
		{
			ShowEmptyMessage(true);
			return;
		}

		ShowEmptyMessage(false);

		foreach (var visual in visuals)
		{
			var control = CreateControlForVisual(visual, fileSize);
			if (control != null)
			{
				var tabItem = new TabItem
				{
					Header = visual.Description ?? visual.GetType().Name,
					Content = control
				};
				_visualsTabControl?.Items.Add(tabItem);
			}
		}

		// Select the first tab
		if (_visualsTabControl != null && _visualsTabControl.Items.Count > 0)
		{
			_visualsTabControl.SelectedIndex = 0;
		}
	}

	private Control? CreateControlForVisual(Visual visual, long fileSize)
	{
		switch (visual)
		{
			case FileMap fileMap:
				var fileMapControl = new FileMapControl
				{
					FileMap = fileMap,
					FileSize = fileSize,
					MinHeight = 300,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Stretch
				};
				return fileMapControl;

			case RasterImage rasterImage:
			case VectorImage vectorImage:
				var imageViewer = new ImageViewerControl
				{
					SourceImage = visual as ImageVisual,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Stretch
				};
				return imageViewer;

			default:
				// Unknown visual type - show a placeholder
				return new TextBlock
				{
					Text = $"Unsupported visual type: {visual.GetType().Name}",
					Foreground = Avalonia.Media.Brushes.Gray,
					FontStyle = Avalonia.Media.FontStyle.Italic,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				};
		}
	}

	private void ShowEmptyMessage(bool show)
	{
		if (_emptyMessage != null)
		{
			_emptyMessage.IsVisible = show;
		}
		if (_visualsTabControl != null)
		{
			_visualsTabControl.IsVisible = !show;
		}
	}

	/// <summary>
	/// Clears all visuals from the view.
	/// </summary>
	public void Clear()
	{
		_visualsTabControl?.Items.Clear();
		ShowEmptyMessage(true);
	}
}
