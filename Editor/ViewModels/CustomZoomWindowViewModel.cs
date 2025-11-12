using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class CustomZoomWindowViewModel : ObservableObject
{
    public int MaxPercentage { get; } = 9999;

    [ObservableProperty]
    private int? _number = null;
}
