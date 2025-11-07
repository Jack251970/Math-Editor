using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class CodepointWindowViewModel : ObservableObject, ICultureInfoChanged
{
    private static readonly string ClassName = nameof(CodepointWindowViewModel);

    public List<UnicodeFormatLocalized> AllUnicodeFormats { get; } = UnicodeFormatLocalized.GetValues();

    [ObservableProperty]
    private UnicodeFormat _unicodeFormat = UnicodeFormat.Decimal;

    [ObservableProperty]
    private double? _number = null;

    // TODO: Support this.
    /*public NumericUpDown UnicodeValueBox { get; set; } = null!;

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
    }*/

    private static bool TryConvertToNumber(UnicodeFormat unicodeFormat, string str, out uint number, out int numberBase)
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
