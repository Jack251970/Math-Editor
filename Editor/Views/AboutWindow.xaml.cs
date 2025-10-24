using System.Windows;

namespace Editor;

public partial class AboutWindow : Window
{
    public string RepositoryUrl { get; } = Constants.RepositoryUrl;
    public string IssuesUrl { get; } = Constants.IssuesUrl;
    public string SponsorUrl { get; } = Constants.SponsorUrl;

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
