using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Globalization;
using System.Text;

namespace Ufex.Controls.Avalonia;

/// <summary>
/// Search and seek bar control for use with HexView.
/// Provides hex/text search with result navigation and hex/dec offset seeking.
/// </summary>
public partial class HexSearchBar : UserControl
{
	private HexView? _hexView;
	private HexSearchState? _searchState;

	private ComboBox? _searchModeCombo;
	private TextBox? _searchTextBox;
	private TextBlock? _resultsLabel;
	private Button? _prevButton;
	private Button? _nextButton;
	private ComboBox? _seekModeCombo;
	private TextBox? _seekTextBox;
	private Button? _goButton;

	public HexSearchBar()
	{
		InitializeComponent();
	}

	/// <summary>
	/// Gets or sets the associated HexView control.
	/// </summary>
	public HexView? HexView
	{
		get => _hexView;
		set => _hexView = value;
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);

		_searchModeCombo = this.FindControl<ComboBox>("SearchModeCombo");
		_searchTextBox = this.FindControl<TextBox>("SearchTextBox");
		_resultsLabel = this.FindControl<TextBlock>("ResultsLabel");
		_prevButton = this.FindControl<Button>("PrevButton");
		_nextButton = this.FindControl<Button>("NextButton");
		_seekModeCombo = this.FindControl<ComboBox>("SeekModeCombo");
		_seekTextBox = this.FindControl<TextBox>("SeekTextBox");
		_goButton = this.FindControl<Button>("GoButton");

		if (_searchModeCombo != null)
		{
			_searchModeCombo.SelectionChanged += OnSearchModeChanged;
		}
		if (_searchTextBox != null)
		{
			_searchTextBox.KeyDown += OnSearchKeyDown;
		}
		if (_prevButton != null)
		{
			_prevButton.Click += OnPrevClick;
		}
		if (_nextButton != null)
		{
			_nextButton.Click += OnNextClick;
		}
		if (_goButton != null)
		{
			_goButton.Click += OnGoClick;
		}
		if (_seekTextBox != null)
		{
			_seekTextBox.KeyDown += OnSeekKeyDown;
		}
	}

	/// <summary>
	/// When the search mode changes, clear the search text and results.
	/// </summary>
	private void OnSearchModeChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (_searchTextBox != null)
		{
			_searchTextBox.Text = string.Empty;
		}
		ClearSearchResults();
	}

	/// <summary>
	/// Execute search when Enter is pressed in the search text box.
	/// </summary>
	private void OnSearchKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			ExecuteSearch();
			e.Handled = true;
		}
	}

	/// <summary>
	/// Navigate to the previous search result.
	/// </summary>
	private void OnPrevClick(object? sender, RoutedEventArgs e)
	{
		if (_hexView != null && _searchState != null)
		{
			_hexView.FindPrevious(_searchState);
			UpdateResultsLabel();
		}
	}

	/// <summary>
	/// Navigate to the next search result.
	/// </summary>
	private void OnNextClick(object? sender, RoutedEventArgs e)
	{
		if (_hexView != null && _searchState != null)
		{
			_hexView.FindNext(_searchState);
			UpdateResultsLabel();
		}
	}

	/// <summary>
	/// Execute seek when the Go button is clicked.
	/// </summary>
	private void OnGoClick(object? sender, RoutedEventArgs e)
	{
		ExecuteSeek();
	}

	/// <summary>
	/// Execute seek when Enter is pressed in the seek text box.
	/// </summary>
	private void OnSeekKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			ExecuteSeek();
			e.Handled = true;
		}
	}

	/// <summary>
	/// Executes a search based on the current mode and input text.
	/// </summary>
	private void ExecuteSearch()
	{
		if (_hexView == null || _searchTextBox == null || _searchModeCombo == null)
			return;

		string searchText = _searchTextBox.Text?.Trim() ?? string.Empty;
		if (string.IsNullOrEmpty(searchText))
		{
			ClearSearchResults();
			return;
		}

		bool isHexMode = _searchModeCombo.SelectedIndex == 0;

		byte[]? pattern = isHexMode
			? ParseHexString(searchText)
			: Encoding.UTF8.GetBytes(searchText);

		if (pattern == null || pattern.Length == 0)
		{
			ClearSearchResults();
			return;
		}

		if (isHexMode)
		{
			_searchState = _hexView.Find(pattern);
		}
		else
		{
			// Text mode: case-insensitive search
			_searchState = _hexView.FindCaseInsensitive(pattern);
		}

		UpdateResultsLabel();
		UpdateNavigationButtons();
	}

	/// <summary>
	/// Executes a seek (goto) based on the current mode and offset input.
	/// </summary>
	private void ExecuteSeek()
	{
		if (_hexView == null || _seekTextBox == null || _seekModeCombo == null)
			return;

		string offsetText = _seekTextBox.Text?.Trim() ?? string.Empty;
		if (string.IsNullOrEmpty(offsetText))
			return;

		bool isHexMode = _seekModeCombo.SelectedIndex == 0;
		long offset;

		if (isHexMode)
		{
			// Remove optional "0x" prefix
			if (offsetText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				offsetText = offsetText.Substring(2);
			}
			if (!long.TryParse(offsetText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset))
				return;
		}
		else
		{
			if (!long.TryParse(offsetText, NumberStyles.Integer, CultureInfo.InvariantCulture, out offset))
				return;
		}

		_hexView.GotoPosition(offset);
	}

	/// <summary>
	/// Clears search state, highlights, and UI.
	/// </summary>
	private void ClearSearchResults()
	{
		_searchState = null;
		_hexView?.ClearSearch();
		UpdateResultsLabel();
		UpdateNavigationButtons();
	}

	/// <summary>
	/// Updates the results label text.
	/// </summary>
	private void UpdateResultsLabel()
	{
		if (_resultsLabel == null) return;

		if (_searchState == null || _searchState.TotalResults == 0)
		{
			_resultsLabel.Text = "No results";
		}
		else
		{
			_resultsLabel.Text = $"{_searchState.CurrentIndex + 1} of {_searchState.TotalResults}";
		}
	}

	/// <summary>
	/// Enables/disables navigation buttons based on search state.
	/// </summary>
	private void UpdateNavigationButtons()
	{
		bool hasResults = _searchState != null && _searchState.TotalResults > 0;

		if (_prevButton != null) _prevButton.IsEnabled = hasResults;
		if (_nextButton != null) _nextButton.IsEnabled = hasResults;
	}

	/// <summary>
	/// Parses a hex string (e.g. "504B0304" or "50 4B 03 04") into a byte array.
	/// Returns null if the input is invalid.
	/// </summary>
	private static byte[]? ParseHexString(string hex)
	{
		// Remove spaces, dashes, and other separators
		var cleaned = new StringBuilder();
		foreach (char c in hex)
		{
			if (IsHexChar(c))
			{
				cleaned.Append(c);
			}
			else if (c == ' ' || c == '-' || c == ':')
			{
				// Skip separators
			}
			else
			{
				// Invalid character
				return null;
			}
		}

		string hexStr = cleaned.ToString();

		// Must have even number of hex characters
		if (hexStr.Length == 0 || hexStr.Length % 2 != 0)
			return null;

		byte[] bytes = new byte[hexStr.Length / 2];
		for (int i = 0; i < bytes.Length; i++)
		{
			if (!byte.TryParse(hexStr.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
			{
				return null;
			}
		}

		return bytes;
	}

	/// <summary>
	/// Focuses the search text box.
	/// </summary>
	public void FocusSearchBox()
	{
		_searchTextBox?.Focus();
	}

	/// <summary>
	/// Programmatically triggers a "Find Next" action.
	/// If there is no active search state, executes the current search first.
	/// </summary>
	public void TriggerFindNext()
	{
		if (_searchState == null || _searchState.TotalResults == 0)
		{
			ExecuteSearch();
		}
		else
		{
			OnNextClick(null, null!);
		}
	}

	/// <summary>
	/// Resets the search state and UI. Call when a new file is loaded.
	/// </summary>
	public void ResetSearch()
	{
		_searchState = null;
		UpdateResultsLabel();
		UpdateNavigationButtons();
	}

	private static bool IsHexChar(char c)
	{
		return (c >= '0' && c <= '9') ||
		       (c >= 'a' && c <= 'f') ||
		       (c >= 'A' && c <= 'F');
	}
}
