using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using iNKORE.UI.WPF.Modern.Controls;

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

public class IntegerNumberFormatter : INumberBoxNumberFormatter
{
    public string FormatDouble(double value)
    {
        var rounded = (long)Math.Round(value);
        return rounded.ToString();
    }

    public double? ParseDouble(string text)
    {
        if (double.TryParse(text, out double result))
        {
            return Math.Round(result);
        }
        return null;
    }
}
