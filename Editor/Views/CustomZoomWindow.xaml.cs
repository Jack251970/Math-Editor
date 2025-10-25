using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Editor;

[INotifyPropertyChanged]
public partial class CustomZoomWindow : Window
{
    private const int MaxPercentage = 9999;

    [ObservableProperty]
    private string _numberBoxText = string.Empty;

    public CustomZoomWindow()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !AreAllValidNumericChars(e.Text) || NumberBoxText.Length > 3;
        base.OnPreviewTextInput(e);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var number = int.Parse(NumberBoxText);
            if (number is <= 0 or > MaxPercentage)
            {
                MessageBox.Show(Localize.CustomZoomWindow_ZoomPercentageRangeError(MaxPercentage), Localize.Error(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((MainWindow)Owner).SetFontSizePercentage(number);
            Close();
        }
        catch
        {
            MessageBox.Show(Localize.CustomZoomWindow_ZoomPercentageFormatError(MaxPercentage), Localize.Error(),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private static bool AreAllValidNumericChars(string str)
    {
        foreach (var c in str)
        {
            if (!char.IsNumber(c)) return false;
        }

        return true;
    }
}
