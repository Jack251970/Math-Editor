using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using iNKORE.UI.WPF.Modern.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Editor;

public partial class App : Application, IDisposable, ISingleInstanceApp
{
    private static readonly string ClassName = nameof(App);

    public static Settings Settings { get; private set; } = new();

    private static bool _disposed;

    // To prevent two disposals running at the same time.
    private static readonly Lock _disposingLock = new();

    [STAThread]
    public static void Main()
    {
        // Initialize settings so that we can get language code
        try
        {
            var storage = new EditorJsonStorage<Settings>();
            Settings = storage.Load();
            Settings.SetStorage(storage);
        }
        catch (Exception e)
        {
            ShowErrorMsgBoxAndFailFast("Cannot load setting storage, please check local data directory", e);
            return;
        }

        // Initialize system language before changing culture info
        Internationalization.InitSystemLanguageCode();

        // Change culture info before application creation to localize WinForm windows
        if (Settings.Language != Constants.SystemLanguageCode)
        {
            Internationalization.ChangeCultureInfo(Settings.Language);
        }

        // Start the application as a single instance
        if (SingleInstance<App>.InitializeAsFirstInstance())
        {
            // Set up Logging
            EditorLogger.Initialize();

            using var application = new App();
            application.InitializeComponent();
            application.Run();

            // Close and flush the logger
            EditorLogger.Close();
        }
    }

    private static void ShowErrorMsgBoxAndFailFast(string message, Exception e)
    {
        // Firstly show users the message
        MessageBox.Show(e.ToString(), message, MessageBoxButton.OK, MessageBoxImage.Error);

        // App cannot construct its App instance, so ensure app crashes w/ the exception info.
        Environment.FailFast(message, e);
    }

    public App()
    {
        // Do not use bitmap cache since it can cause WPF second window freezing issue
        ShadowAssist.UseBitmapCache = false;

        // Configure the dependency injection container
        try
        {
            var host = Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureFBLogger()
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateOnBuild = true;
                })
                .ConfigureServices(services => services
                    .AddSingleton(_ => Settings)
                    .AddSingleton<Internationalization>()
                    .AddSingleton<LatexConverter>()
                    .AddSingleton<TextManager>()
                    .AddTransient<UndoManager>()
                    .AddSingleton<ClipboardHelper>()
                    .AddTransient<MainWindowViewModel>()
                    .AddTransient<SettingsWindowViewModel>()
            ).Build();
            Ioc.Default.ConfigureServices(host.Services);
        }
        catch (Exception e)
        {
            ShowErrorMsgBoxAndFailFast("Cannot configure dependency injection container, please open new issue in Math Editor", e);
            return;
        }
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        await Stopwatch.InfoAsync(ClassName, "Startup cost", async () =>
        {
            // Initialize language before portable clean up since it needs translations
            await Ioc.Default.GetRequiredService<Internationalization>().InitializeLanguageAsync();

            EditorLogger.Info(ClassName, "Begin Editor startup -------------------------------------------------");
            EditorLogger.Info(ClassName, $"Runtime info:{ExceptionFormatter.RuntimeInfo()}");

            RegisterAppDomainExceptions();
            RegisterDispatcherUnhandledException();
            RegisterTaskSchedulerUnhandledException();

            Ioc.Default.GetRequiredService<ClipboardHelper>().StartMonitoring();

            Ioc.Default.GetRequiredService<LatexConverter>().LoadPredefinedLatexUnicodeMapping();

            var strings = Environment.GetCommandLineArgs();
            var fileName = strings.Length > 1 ? strings[1] : string.Empty;
            var mainWindow = new MainWindow(fileName);
            mainWindow.Show();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            RegisterExitEvents();

            EditorLogger.Info(ClassName, "End Editor startup ---------------------------------------------------");

            _ = Task.Run(Ioc.Default.GetRequiredService<LatexConverter>().LoadUserUnicodeMapping);
        });
    }

    private void RegisterExitEvents()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            EditorLogger.Info(ClassName, "Process Exit");
            Dispose();
        };

        Current.Exit += (s, e) =>
        {
            EditorLogger.Info(ClassName, "Application Exit");
            Dispose();
        };

        Current.SessionEnding += (s, e) =>
        {
            EditorLogger.Info(ClassName, "Session Ending");
            Dispose();
        };
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug.
    /// </summary>
    [Conditional("RELEASE")]
    private void RegisterDispatcherUnhandledException()
    {
        DispatcherUnhandledException += ErrorReporting.DispatcherUnhandledException;
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug.
    /// </summary>
    [Conditional("RELEASE")]
    private static void RegisterAppDomainExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += ErrorReporting.UnhandledException;
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug.
    /// </summary>
    private static void RegisterTaskSchedulerUnhandledException()
    {
        TaskScheduler.UnobservedTaskException += ErrorReporting.TaskSchedulerUnobservedTaskException;
    }

    public void OnSecondAppStarted()
    {
        var mainWindow = new MainWindow(string.Empty);
        mainWindow.Show();
    }

    protected virtual void Dispose(bool disposing)
    {
        // Prevent two disposes at the same time.
        lock (_disposingLock)
        {
            if (!disposing)
            {
                return;
            }

            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        Stopwatch.Info(ClassName, "Dispose cost", () =>
        {
            EditorLogger.Info(ClassName, "Begin Editor dispose -------------------------------------------------");

            if (disposing)
            {
                // Dispose needs to be called on the main Windows thread,
                // since some resources owned by the thread need to be disposed.
                Settings.Save();
            }

            EditorLogger.Info(ClassName, "End Editor dispose ---------------------------------------------------");
        });
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
