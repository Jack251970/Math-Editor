using System.Windows;

namespace Editor;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        DataContext = this;
        InitializeComponent();
#if DEBUG
        versionLabel.Text = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev})";
#else
        versionLabel.Text = $"{Constants.MathEditorFullName} v{Constants.Version}";
#endif
    }
}
