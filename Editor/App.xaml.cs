using System;
using System.Windows;
using AnyBar.Helpers.Application;
using Editor.Models;

namespace Editor;

public partial class App : Application, ISingleInstanceApp
{
    public static Settings Settings { get; private set; } = new();

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
        Current.MainWindow = new MainWindow();
        Current.MainWindow.Show();
    }

    public void OnSecondAppStarted()
    {
        if (Current.MainWindow is MainWindow window)
        {
            window.Show();
            window.Focus();
        }
    }
}
