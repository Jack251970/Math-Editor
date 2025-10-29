using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

[INotifyPropertyChanged]
public partial class MatrixInputWindow : Window
{
    [ObservableProperty]
    private double _rows = 1;

    [ObservableProperty]
    private double _columns = 1;

    public MatrixInputWindow()
    {
        DataContext = this;
        InitializeComponent();
    }

    public MatrixInputWindow(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        DataContext = this;
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (Rows > 0 && Columns > 0)
        {
            var newCommand = new CommandDetails
            {
                CommandType = CommandType.Matrix,
                CommandParam = new int[]
                {
                    (int)Rows, (int)Columns
                }
            };
            ((MainWindow)Owner).Editor.HandleUserCommand(newCommand);
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
