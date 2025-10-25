using System;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class SettingsWindow : Window, ICultureInfoChanged
{
    private readonly SettingsWindowViewModel _viewModel;

    public SettingsWindow()
    {
        _viewModel = Ioc.Default.GetRequiredService<SettingsWindowViewModel>();
        DataContext = _viewModel;
        InitializeComponent();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        App.Settings.Save();
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        _viewModel.OnCultureInfoChanged(newCultureInfo);
    }
}
