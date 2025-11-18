using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class CodepointWindowViewModel : ObservableObject, ICultureInfoChanged
{
    private static readonly string ClassName = nameof(CodepointWindowViewModel);

    // Unicode Value Maximum: 0x10FFFF, 1114111, 04177777
    private const uint MaxUnicodeValue = 0x10FFFF;

    public List<UnicodeFormatLocalized> AllUnicodeFormats { get; } = UnicodeFormatLocalized.GetValues();

    [ObservableProperty]
    public partial UnicodeFormat UnicodeFormat { get; set; } = UnicodeFormat.Decimal;

    [ObservableProperty]
    public partial string? NumberText { get; set; } = null;

    public uint? Number { get; private set; } = null;

    partial void OnUnicodeFormatChanged(UnicodeFormat value)
    {
        NumberText = GetNumberTextFromNumber();
    }

    public bool TryUpdateNumberFromNumberText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Number = null;
            return false;
        }

        if (TryConvertToNumber(text, out var parsed, out var _))
        {
            if (parsed > MaxUnicodeValue)
            {
                Number = MaxUnicodeValue;
                return true;
            }
            else
            {
                Number = parsed;
                return false;
            }
        }
        else
        {
            Number = null;
            return true;
        }
    }

    private bool TryConvertToNumber(string str, out uint number, out int numberBase)
    {
        try
        {
            switch (UnicodeFormat)
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
                    throw new InvalidOperationException($"Unsupported Unicode format: {UnicodeFormat}");
            }
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to convert string to number.", e);
            number = 0;
            numberBase = 0;
            return false;
        }
    }

    public string GetNumberTextFromNumber()
    {
        try
        {
            if (Number.HasValue)
            {
                return UnicodeFormat switch
                {
                    UnicodeFormat.Decimal => Number.Value.ToString(CultureInfo.InvariantCulture),
                    UnicodeFormat.Octal => Convert.ToString(Number.Value, 8),
                    UnicodeFormat.Hexadecimal => "0x" + Number.Value.ToString("X", CultureInfo.InvariantCulture),
                    _ => throw new InvalidOperationException($"Unsupported Unicode format: {UnicodeFormat}"),
                };
            }
            else
            {
                return string.Empty;
            }
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to update number text from number.", e);
            return string.Empty;
        }
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        UnicodeFormatLocalized.UpdateLabels(AllUnicodeFormats);
    }
}
