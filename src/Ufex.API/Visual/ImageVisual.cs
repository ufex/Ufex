namespace Ufex.API.Visual;

public abstract class ImageVisual : Visual
{
	public int Width { get; set; }
	public int Height { get; set; }

	protected ImageVisual(string description) : base(description)
	{
	}
}