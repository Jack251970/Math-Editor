using System.Windows;
using System.Windows.Input;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Editor;

public partial class CustomZoomWindow : Window
{
    private readonly int maxPercentage = 9999;

    public CustomZoomWindow()
    {
        InitializeComponent();
        numberBox.Focus();
    }

    private static bool AreAllValidNumericChars(string str)
    {
        foreach (var c in str)
        {
            if (!char.IsNumber(c)) return false;
        }

        return true;
    }

    private void numberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !AreAllValidNumericChars(e.Text) || numberBox.Text.Length > 3;
        base.OnPreviewTextInput(e);
    }

    private void okButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var number = int.Parse(numberBox.Text);
            if (number <= 0 || number > maxPercentage)
            {
                MessageBox.Show("Zoom percentage must be between 1 and " + maxPercentage + ".");
                return;
            }
            ((MainWindow)Owner).SetFontSizePercentage(number);
            Close();
        }
        catch
        {
            MessageBox.Show("Zoom percentage must be a number between 1 and " + maxPercentage + ".");
        }
    }

    private void cancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
