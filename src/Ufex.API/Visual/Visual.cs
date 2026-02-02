
namespace Ufex.API.Visual;

/// <summary>
/// Represents data for a visual element, with a description.
/// </summary>
public abstract class Visual
{
	public string Description { get; protected set; } 

	public Visual(string description)
	{
		Description = description;
	}
}