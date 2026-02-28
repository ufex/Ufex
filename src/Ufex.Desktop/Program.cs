using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ufex.Desktop;

class Program
{
	private static readonly string CrashLogPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"ufex", "Logs", "crash.log");

	[STAThread]
	public static void Main(string[] args)
	{
		Console.WriteLine("ufex starting...");

		// Global handlers to catch unhandled exceptions and log them before the process dies
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
		{
			WriteCrashLog("AppDomain.UnhandledException", e.ExceptionObject as Exception);
		};

		TaskScheduler.UnobservedTaskException += (_, e) =>
		{
			WriteCrashLog("TaskScheduler.UnobservedTaskException", e.Exception);
			e.SetObserved(); // Prevent process termination
		};

		try
		{
			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
		}
		catch (Exception ex)
		{
			WriteCrashLog("Main", ex);
			throw;
		}
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();

	/// <summary>
	/// Writes an unhandled exception to the crash log file so release build
	/// crashes leave a diagnostic trail even when there is no console window.
	/// </summary>
	private static void WriteCrashLog(string source, Exception? ex)
	{
		try
		{
			var dir = Path.GetDirectoryName(CrashLogPath);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CRASH in {source}:{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}";
			File.AppendAllText(CrashLogPath, message);
		}
		catch
		{
			// Last resort — nothing we can do if crash logging itself fails
		}
	}
}
