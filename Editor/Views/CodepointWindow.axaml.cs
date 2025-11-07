using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class CodepointWindow : Window, ICultureInfoChanged
{
    public CodepointWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<CodepointWindowViewModel>();

    public CodepointWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
        // TODO: Add support for this.
        /*UnicodeValueBox.NumberFormatter = new CodepointNumberFormatter(this);*/
    }

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Number > 0)
        {
            var commandDetails = new CommandDetails
            {
                UnicodeString = Convert.ToChar((uint)ViewModel.Number).ToString(),
                CommandType = CommandType.Text
            };
            ((IMainWindow)Owner!).HandleToolBarCommand(commandDetails);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        ViewModel.OnCultureInfoChanged(newCultureInfo);
    }
}
