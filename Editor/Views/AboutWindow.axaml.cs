using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class AboutWindow : Window
{
    public AboutWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<AboutWindowViewModel>();

    public AboutWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }
}
