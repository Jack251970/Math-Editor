using System;
using System.Globalization;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class SettingsWindow : Window, ICultureInfoChanged
{
    public SettingsWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<SettingsWindowViewModel>();

    public SettingsWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        App.Settings.Save();
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        ViewModel.OnCultureInfoChanged(newCultureInfo);
    }
}
