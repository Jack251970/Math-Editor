using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Editor;

public partial class App : Application, IDisposable, ISingleInstanceApp
{
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

        // Start the application as a single instance
        if (SingleInstance<App>.InitializeAsFirstInstance())
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }

    private static void ShowErrorMsgBoxAndFailFast(string message, Exception e)
    {
        // Firstly show users the message
        MessageBox.Show(e.ToString(), message, MessageBoxButton.OK, MessageBoxImage.Error);

        // App cannot construct its App instance, so ensure app crashes w/ the exception info.
        Environment.FailFast(message, e);
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        LatexConverter.LoadPredefinedLatexUnicodeMapping();

        var strings = Environment.GetCommandLineArgs();
        var fileName = strings.Length > 1 ? strings[1] : string.Empty;
        Current.MainWindow = new MainWindow(fileName);
        Current.MainWindow.Show();

        _ = Task.Run(LatexConverter.LoadUserUnicodeMapping);
    }

    public void OnSecondAppStarted()
    {
        // TODO: Open a new window
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

        if (disposing)
        {
            // Dispose needs to be called on the main Windows thread,
            // since some resources owned by the thread need to be disposed.
            Settings.Save();
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
