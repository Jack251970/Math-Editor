using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class CodepointWindow : Window, ICultureInfoChanged
{
    public CodepointWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<CodepointWindowViewModel>();

    public CodepointWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Number.HasValue &&
            ViewModel.Number.Value > 0)
        {
            var commandDetails = new CommandDetails
            {
                UnicodeString = Convert.ToChar(ViewModel.Number.Value).ToString(),
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

    private void UnicodeValueBox_TextChanging(object? sender, TextChangingEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        var text = textBox.Text ?? string.Empty;

        var allowedCharsPattern = ViewModel.UnicodeFormat switch
        {
            UnicodeFormat.Decimal => "^[0-9]*$",
            UnicodeFormat.Octal => "^[0-7]*$",
            UnicodeFormat.Hexadecimal => "^[0-9a-fA-Fx]*$", // support 0x prefix typing
            _ => ".*"
        };

        var handled = false;
        var newText = text;

        // Check if text contains only allowed characters
        if (!Regex.IsMatch(text, allowedCharsPattern))
        {
            // Remove invalid characters
            newText = new string([.. text.Where(c =>
                ViewModel.UnicodeFormat switch
                {
                    UnicodeFormat.Decimal => char.IsDigit(c),
                    UnicodeFormat.Octal => c >= '0' && c <= '7',
                    UnicodeFormat.Hexadecimal =>
                        char.IsDigit(c) ||
                        (c >= 'a' && c <= 'f') ||
                        (c >= 'A' && c <= 'F') ||
                        c == 'x' || c == 'X',
                    _ => true
                })]);

            handled = true;
        }

        // Check if the number is within valid range
        if (ViewModel.TryUpdateNumberFromNumberText(newText))
        {
            // Limit the text to valid range
            newText = ViewModel.GetNumberTextFromNumber();

            handled = true;
        }

        e.Handled = handled;

        if (handled)
        {
            // This is a workaround for Avalonia not supporting updating text in TextBox.TextChanging event:
            // https://github.com/AvaloniaUI/Avalonia/issues/20022
            // It can cause little flicker when updating the TextBox.Text property but we have no choice.
            _ = Dispatcher.UIThread.InvokeAsync(() =>
            {
                textBox.Text = newText;
                textBox.CaretIndex = textBox.Text.Length;
            });
        }
    }
}
