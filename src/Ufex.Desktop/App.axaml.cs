using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Ufex.Desktop;

public partial class App : Application
{
    public static App? Instance => Current as App;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Toggles between Light and Dark theme variants.
    /// </summary>
    public void ToggleTheme()
    {
        if (RequestedThemeVariant == ThemeVariant.Light)
        {
            RequestedThemeVariant = ThemeVariant.Dark;
        }
        else
        {
            RequestedThemeVariant = ThemeVariant.Light;
        }
    }

    /// <summary>
    /// Sets the theme to a specific variant.
    /// </summary>
    public void SetTheme(ThemeVariant theme)
    {
        RequestedThemeVariant = theme;
    }

    /// <summary>
    /// Gets whether the current theme is dark.
    /// </summary>
    public bool IsDarkTheme => RequestedThemeVariant == ThemeVariant.Dark;
}
