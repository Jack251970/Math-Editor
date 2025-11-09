using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class MatrixInputWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private int _rows = 1;

    [ObservableProperty]
    private int _columns = 1;
}
