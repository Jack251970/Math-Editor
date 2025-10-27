using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Editor;

[INotifyPropertyChanged]
public partial class CodepointWindow : Window, ICultureInfoChanged
{
    private static readonly string ClassName = nameof(CodepointWindow);

    public List<UnicodeFormatLocalized> AllUnicodeFormats { get; } = UnicodeFormatLocalized.GetValues();

    [ObservableProperty]
    private UnicodeFormat _unicodeFormat = UnicodeFormat.Decimal;

    [ObservableProperty]
    private string _numberBoxText = string.Empty;

    public CodepointWindow()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !ConvertToNumber(e.Text, out var _);
        base.OnPreviewTextInput(e);
    }

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        if (ConvertToNumber(NumberBoxText, out var number))
        {
            try
            {
                var commandDetails = new CommandDetails
                {
                    UnicodeString = Convert.ToChar(number).ToString(),
                    CommandType = CommandType.Text
                };
                ((MainWindow)Owner).HandleToolBarCommand(commandDetails);
            }
            catch
            {
                MessageBox.Show(this, Localize.CodepointWindow_GivenValueError(), Localize.Error(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show(this, Localize.CodepointWindow_EnteredError(), Localize.Error(),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    partial void OnUnicodeFormatChanged(UnicodeFormat value)
    {
        var numberBase = value switch
        {
            UnicodeFormat.Octal => 8,
            UnicodeFormat.Decimal => 10,
            UnicodeFormat.Hexadecimal => 16,
            _ => throw new NotImplementedException(),
        };
        if (!string.IsNullOrEmpty(NumberBoxText))
        {
            try
            {
                if (ConvertToNumber(NumberBoxText, out var number))
                {
                    NumberBoxText = Convert.ToString(number, numberBase);
                }
                else
                {
                    MessageBox.Show(this, Localize.CodepointWindow_EnteredError(), Localize.Error(),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception e)
            {
                EditorLogger.Fatal(ClassName, "Failed to convert number on format change.", e);
                MessageBox.Show(this, Localize.CodepointWindow_NumberFormatError(), Localize.Error(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NumberBoxText = string.Empty;
            }
        }
    }

    private bool ConvertToNumber(string str, out uint number)
    {
        try
        {
            switch (UnicodeFormat)
            {
                case UnicodeFormat.Octal:
                    number = Convert.ToUInt32(str, 8);
                    return true;
                case UnicodeFormat.Decimal:
                    number = uint.Parse(str);
                    return true;
                case UnicodeFormat.Hexadecimal:
                    number = Convert.ToUInt32(str, 16);
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }
        catch
        {
            number = 0;
            return false;
        }
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        UnicodeFormatLocalized.UpdateLabels(AllUnicodeFormats);
    }
}
