using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class CustomZoomWindowViewModel : ObservableObject
{
    public int MaxPercentage { get; } = 9999;

    [ObservableProperty]
    public partial int? Number { get; set; } = null;
}
