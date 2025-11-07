using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class CustomZoomWindow : Window
{
    public CustomZoomWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<CustomZoomWindowViewModel>();

    public CustomZoomWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Number is null)
        {
            return;
        }
        var percentage = (int)ViewModel.Number;
        if (percentage > 0 && percentage < ViewModel.MaxPercentage)
        {
            ((IMainWindow)Owner!).CustomZoomPercentage = percentage;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
