using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class MatrixInputWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private double _rows = 1;

    [ObservableProperty]
    private double _columns = 1;
}
