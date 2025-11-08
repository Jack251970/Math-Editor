using System;
using Avalonia;
using Avalonia.Controls;

namespace Editor.Desktop;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Start the application as a single instance
            if (SingleInstance<App>.InitializeAsFirstInstance())
            {
                // Set up Logging
                EditorLogger.Initialize();

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
                if (Application.Current is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                // Close and flush the logger
                EditorLogger.Close();
            }
        }
        catch (Exception e)
        {
            ErrorReporting.DispatcherUnhandledException(e);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
