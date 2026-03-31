using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Ufex.Desktop;

public partial class MatchDetailsWindow : Window
{
	public MatchDetailsWindow(string fileTypeDescription, string matchDetails)
	{
		InitializeComponent();
		TitleText.Text = fileTypeDescription;
		DetailsText.Text = matchDetails;
	}

	private void OnCloseClick(object? sender, RoutedEventArgs e)
	{
		Close();
	}
}
