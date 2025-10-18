using System.Windows;

namespace Editor;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        DataContext = this;
        InitializeComponent();
#if DEBUG
        VersionLabel.Text = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev})";
#else
        VersionLabel.Text = $"{Constants.MathEditorFullName} v{Constants.Version}";
#endif
    }
}
