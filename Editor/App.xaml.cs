using System;
using System.Windows;
using AnyBar.Helpers.Application;

namespace Editor;

public partial class App : Application, ISingleInstanceApp
{
    [STAThread]
    public static void Main()
    {
        // Start the application as a single instance
        if (SingleInstance<App>.InitializeAsFirstInstance())
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
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
