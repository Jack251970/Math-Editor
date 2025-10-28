using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

[INotifyPropertyChanged]
public partial class CustomZoomWindow : Window
{
    public int MaxPercentage { get; } = 9999;

    [ObservableProperty]
    private double? _number = null;

    public CustomZoomWindow()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (Number is null)
        {
            return;
        }
        var percentage = (int)Number;
        if (percentage > 0 && percentage < MaxPercentage)
        {
            ((MainWindow)Owner).ViewModel.CustomZoomPercentage = percentage;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
