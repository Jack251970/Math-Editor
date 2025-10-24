using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

[INotifyPropertyChanged]
public partial class MatrixInputWindow : Window
{
    [ObservableProperty]
    private string _rowsText = string.Empty;

    [ObservableProperty]
    private string _columnsText = string.Empty;

    public MatrixInputWindow()
    {
        DataContext = this;
        InitializeComponent();
    }

    public MatrixInputWindow(int rows, int columns)
    {
        DataContext = this;
        InitializeComponent();
        RowsText = rows.ToString(CultureInfo.CurrentUICulture);
        ColumnsText = columns.ToString(CultureInfo.CurrentUICulture);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // WPF does not provide a NumericUpDown control out of the box
        // The button click will be ignored, when the number cannot be parsed.
        // TODO: provide user feedback when the input is invalid or implement a proper NumericUpDownControl
        if (int.TryParse(RowsText, NumberStyles.Integer, CultureInfo.CurrentUICulture, out var rows)
            && int.TryParse(ColumnsText, NumberStyles.Integer, CultureInfo.CurrentUICulture, out var columns))
        {
            var newCommand = new CommandDetails
            {
                CommandType = CommandType.Matrix,
                CommandParam = new int[] { rows, columns }
            };
            ((MainWindow)Owner).editor.HandleUserCommand(newCommand);
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
