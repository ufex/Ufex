using Avalonia.Controls;
using System.IO;
using Ufex.Controls.Avalonia;

namespace Ufex.Desktop.Views;

public partial class HexTabView : UserControl
{
	private HexView? _hexViewer;

	public HexTabView()
	{
		InitializeComponent();
		_hexViewer = this.FindControl<HexView>("HexViewer");
	}

	/// <summary>
	/// Loads a file stream into the hex viewer.
	/// </summary>
	public void LoadStream(Stream? stream)
	{
		if (_hexViewer == null) return;

		if (stream == null)
		{
			_hexViewer.UnloadFile();
		}
		else
		{
			_hexViewer.LoadStream(stream);
		}
	}

	/// <summary>
	/// Loads a file by path into the hex viewer.
	/// </summary>
	public void LoadFile(string filePath)
	{
		_hexViewer?.LoadFile(filePath);
	}

	/// <summary>
	/// Clears the hex viewer content.
	/// </summary>
	public void Clear()
	{
		_hexViewer?.UnloadFile();
	}

	/// <summary>
	/// Scrolls to display the specified file position.
	/// </summary>
	public void GotoPosition(long position)
	{
		_hexViewer?.GotoPosition(position);
	}

	/// <summary>
	/// Highlights a range of bytes in the display.
	/// </summary>
	public void SetHighlight(long startPosition, long endPosition)
	{
		_hexViewer?.SetHighlight(startPosition, endPosition);
	}

	/// <summary>
	/// Clears any highlight.
	/// </summary>
	public void ClearHighlight()
	{
		_hexViewer?.ClearHighlight();
	}

	/// <summary>
	/// Gets or sets whether hex digits are displayed in uppercase.
	/// </summary>
	public bool HexUpperCase
	{
		get => _hexViewer?.HexUpperCase ?? true;
		set
		{
			if (_hexViewer != null)
				_hexViewer.HexUpperCase = value;
		}
	}
}
