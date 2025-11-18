using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class MatrixInputWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int? Rows { get; set; } = 1;

    [ObservableProperty]
    public partial int? Columns { get; set; } = 1;
}
