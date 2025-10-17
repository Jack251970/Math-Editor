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
    //protected override void OnStartup(StartupEventArgs e)
    //{
    //    try
    //    {
    //        if (e.Args != null && e.Args.Count() > 0)
    //        {
    //            //MessageBox.Show(e.Args[0]);
    //        }
    //        var fileName = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];
    //        MessageBox.Show("After call");
    //        this.Properties["AFunnyNameForKKj$$"] = (new Uri(fileName)).LocalPath;
    //        MessageBox.Show(this.Properties["AFunnyNameForKKj$$"] as string);
    //    }
    //    catch { }
    //    base.OnStartup(e);
    //}
}
