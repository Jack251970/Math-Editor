using System;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        DataContext = Ioc.Default.GetRequiredService<SettingsWindowViewModel>();
        InitializeComponent();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        App.Settings.Save();
    }
}
