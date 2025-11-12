using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Editor;

public partial class App : Application, ISingleInstanceApp, IDisposable
{
    private static readonly string ClassName = nameof(App);

    public static Settings Settings { get; private set; } = new();

    private static bool _disposed;

    // To prevent two disposals running at the same time.
    private static readonly Lock _disposingLock = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Default logic doesn't auto detect windows theme anymore in designer
        // to stop light mode, force here
        if (Design.IsDesignMode)
        {
            RequestedThemeVariant = ThemeVariant.Dark;
        }
    }

    public override async void OnFrameworkInitializationCompleted()
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
            await ShowErrorMsgBoxAndExitAsync("Cannot load setting storage and please check local data directory", e);
            return;
        }

        // Initialize system language before changing culture info
        // Change culture info before application creation to localize some windows
        try
        {
            Internationalization.InitSystemLanguageCode();
            if (Settings.Language != Constants.SystemLanguageCode)
            {
                Internationalization.ChangeCultureInfo(Settings.Language);
            }
        }
        catch (Exception e)
        {
            await ShowErrorMsgBoxAndExitAsync("Cannot initialize system language or change culture info", e);
            return;
        }

        // Configure the dependency injection container
        try
        {
            var host = Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureLogger()
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
                    .AddTransient<AboutWindowViewModel>()
                    .AddTransient<CodepointWindowViewModel>()
                    .AddTransient<CustomZoomWindowViewModel>()
                    .AddTransient<MainWindowViewModel>()
                    .AddTransient<MatrixInputWindowViewModel>()
                    .AddTransient<SettingsWindowViewModel>()
                    .AddTransient<UnicodeSelectorWindowViewModel>()
            ).Build();
            Ioc.Default.ConfigureServices(host.Services);
        }
        catch (Exception e)
        {
            await ShowErrorMsgBoxAndExitAsync("Cannot configure dependency injection container", e);
            return;
        }

        // Startup the application
        await Stopwatch.InfoAsync(ClassName, "Startup cost", async () =>
        {
            // Initialize language before portable clean up since it needs translations
            await Ioc.Default.GetRequiredService<Internationalization>().InitializeLanguageAsync();

            EditorLogger.Info(ClassName, "Begin Editor startup -------------------------------------------------");
            EditorLogger.Info(ClassName, $"Runtime info:{ExceptionFormatter.RuntimeInfo()}");

            RegisterDispatcherUnhandledException();
            RegisterAppDomainExceptions();
            RegisterTaskSchedulerUnhandledException();

            Ioc.Default.GetRequiredService<LatexConverter>().LoadPredefinedLatexUnicodeMapping();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var strings = desktop.Args;
                var fileName = strings?.Length > 1 ? strings[1] : string.Empty;
                var mainWindow = new MainWindow(fileName);
                mainWindow.Show();

                RegisterExitEvents(desktop);
            }
            /*else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                singleView.MainView = new MainView();
            }*/
            else if (Design.IsDesignMode)
            {
                // Ignored so that Avalonia previewer can work
            }
            else
            {
                throw new NotSupportedException("Unsupported application lifetime");
            }

            Ioc.Default.GetRequiredService<ClipboardHelper>().StartMonitoring();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            EditorLogger.Info(ClassName, "End Editor startup ---------------------------------------------------");

            _ = Task.Run(Ioc.Default.GetRequiredService<LatexConverter>().LoadUserUnicodeMapping);
        });

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task ShowErrorMsgBoxAndExitAsync(string message, Exception e)
    {
        // Log to file
        EditorLogger.Fatal(ClassName, message, e);

        // Firstly show users the message
        await MessageBox.ShowAsync("See more information in your log files", message, MessageBoxButton.OK, MessageBoxImage.Error);

        // Try shutdown the application
        Environment.Exit(-1);
    }

    private static void RegisterDispatcherUnhandledException()
    {
        Dispatcher.UIThread.UnhandledException += ErrorReporting.DispatcherUnhandledException;
        Dispatcher.UIThread.UnhandledExceptionFilter += ErrorReporting.DispatcherUnhandledExceptionFilter;
    }

    private static void RegisterAppDomainExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += ErrorReporting.UnhandledException;
    }

    private static void RegisterTaskSchedulerUnhandledException()
    {
        TaskScheduler.UnobservedTaskException += ErrorReporting.TaskSchedulerUnobservedTaskException;
    }

    private void RegisterExitEvents(IClassicDesktopStyleApplicationLifetime desktop)
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            EditorLogger.Info(ClassName, "Process Exit");
            Dispose();
        };

        desktop.Exit += (s, e) =>
        {
            EditorLogger.Info(ClassName, "Application Exit");
            Dispose();
        };

        desktop.ShutdownRequested += (s, e) =>
        {
            EditorLogger.Info(ClassName, "Session Ending");
            Dispose();
        };
    }

    public void OnSecondAppStarted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow(string.Empty);
            mainWindow.Show();
        }
        else
        {
            throw new NotSupportedException("Unsupported application lifetime");
        }
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
                Ioc.Default.GetRequiredService<ClipboardHelper>().Dispose();
                Ioc.Default.GetRequiredService<Internationalization>().Dispose();
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
