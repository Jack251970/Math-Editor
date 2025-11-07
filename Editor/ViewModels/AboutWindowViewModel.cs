using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public class AboutWindowViewModel : ObservableObject
{
    public string RepositoryUrl { get; } = Constants.RepositoryUrl;
    public string IssuesUrl { get; } = Constants.IssuesUrl;
    public string SponsorUrl { get; } = Constants.SponsorUrl;
}
