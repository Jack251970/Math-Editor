using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Velopack;

namespace Editor.Desktop;

internal class Program
{
    private static readonly string ClassName = nameof(Program);

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // It's important to Run() the VelopackApp as early as possible in app startup.
            VelopackApp.Build().Run();

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
            // Do not catch critical exceptions that should not be handled
            if (e is StackOverflowException or OutOfMemoryException or ThreadAbortException)
            {
                throw;
            }
            EditorLogger.Fatal(ClassName, "Fatal error in Main method", e);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                // We have used Serilog for logging already
                /*.LogToTrace()*/;
    }
}
