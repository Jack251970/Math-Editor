using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class MatrixInputWindow : Window
{
    public MatrixInputWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<MatrixInputWindowViewModel>();

    public MatrixInputWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }

    public MatrixInputWindow(int rows, int columns)
    {
        ViewModel.Rows = rows;
        ViewModel.Columns = columns;
        DataContext = this;
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Rows > 0 && ViewModel.Columns > 0)
        {
            var newCommand = new CommandDetails
            {
                CommandType = CommandType.Matrix,
                CommandParam = new int[]
                {
                    (int)ViewModel.Rows, (int)ViewModel.Columns
                }
            };
            ((IMainWindow)Owner!).HandleUserCommand(newCommand);
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
