using System;
using System.Windows;

namespace Editor;

public partial class App : Application
{
    [STAThread]
    public static void Main()
    {
        var application = new App();
        application.InitializeComponent();
        application.Run();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Current.MainWindow = new MainWindow();
        Current.MainWindow.Show();
    }
}
