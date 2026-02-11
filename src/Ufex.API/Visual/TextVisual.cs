using Ufex.API.Tables;

namespace Ufex.API.Visual;

/// <summary>
/// A read-only visual representation of text for display in a text box or similar control.
/// </summary>
public class TextVisual : Visual
{
	public string Text { get; set; }

	public TextVisual(string text, string description) : base(description)
	{
		Text = text;
	}
}