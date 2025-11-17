using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Velopack;

namespace Editor;

public class AboutWindowViewModel : ObservableObject
{
    private UpdateManager? _updateManager { get; }

    public string RepositoryUrl { get; } = Constants.RepositoryUrl;
    public string IssuesUrl { get; } = Constants.IssuesUrl;
    public string SponsorUrl { get; } = Constants.SponsorUrl;
    public string Version => _updateManager?.CurrentVersion?.ToString() ?? "1.0.0";

    public AboutWindowViewModel()
    {
        if (!Design.IsDesignMode)
        {
            _updateManager = Ioc.Default.GetRequiredService<UpdateManager>();
        }
    }
}
