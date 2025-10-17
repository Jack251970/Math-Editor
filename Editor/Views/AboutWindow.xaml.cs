using System.Reflection;
using System.Windows;

namespace Editor;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        versionLabel.Content = "Math Editor v." + Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}
