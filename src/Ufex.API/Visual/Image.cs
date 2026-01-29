namespace Ufex.API.Visual;

public abstract class Image : Visual
{
	public int Width { get; set; }
	public int Height { get; set; }

	protected Image(string description) : base(description)
	{
	}
}