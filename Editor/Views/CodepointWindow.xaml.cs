using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

[INotifyPropertyChanged]
public partial class CodepointWindow : Window, ICultureInfoChanged
{
    private static readonly string ClassName = nameof(CodepointWindow);

    public List<UnicodeFormatLocalized> AllUnicodeFormats { get; } = UnicodeFormatLocalized.GetValues();

    [ObservableProperty]
    private UnicodeFormat _unicodeFormat = UnicodeFormat.Decimal;

    [ObservableProperty]
    private double? _number = null;

    public CodepointWindow()
    {
        DataContext = this;
        InitializeComponent();
        UnicodeValueBox.NumberFormatter = new CodepointNumberFormatter(this);
    }

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        if (Number > 0)
        {
            var commandDetails = new CommandDetails
            {
                UnicodeString = Convert.ToChar((uint)Number).ToString(),
                CommandType = CommandType.Text
            };
            ((MainWindow)Owner).HandleToolBarCommand(commandDetails);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    partial void OnUnicodeFormatChanged(UnicodeFormat value)
    {
        if (!string.IsNullOrEmpty(UnicodeValueBox.Text))
        {
            if (TryConvertToNumber(UnicodeFormat, UnicodeValueBox.Text, out var number, out var numberBase))
            {
                UnicodeValueBox.Text = Convert.ToString(number, numberBase);
            }
            else
            {
                UnicodeValueBox.Text = string.Empty;
            }
        }
    }

    public static bool TryConvertToNumber(UnicodeFormat unicodeFormat, string str, out uint number, out int numberBase)
    {
        try
        {
            switch (unicodeFormat)
            {
                case UnicodeFormat.Octal:
                    number = Convert.ToUInt32(str, 8);
                    numberBase = 8;
                    return true;
                case UnicodeFormat.Decimal:
                    number = uint.Parse(str);
                    numberBase = 10;
                    return true;
                case UnicodeFormat.Hexadecimal:
                    number = Convert.ToUInt32(str, 16);
                    numberBase = 16;
                    return true;
                default:
                    throw new InvalidOperationException($"Unsupported Unicode format: {unicodeFormat}");
            }
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to convert string to number.", e);
            number = 0;
            numberBase = 0;
            return false;
        }
        ;
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        UnicodeFormatLocalized.UpdateLabels(AllUnicodeFormats);
    }
}
