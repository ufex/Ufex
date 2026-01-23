using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace Ufex.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.WriteLine("ufex starting...");
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex}");
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
