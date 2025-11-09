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
    private UnicodeFormat _unicodeFormat = UnicodeFormat.Decimal;

    [ObservableProperty]
    private string? _numberText = null;

    [ObservableProperty]
    private uint? _number = null;

    partial void OnUnicodeFormatChanged(UnicodeFormat value)
    {
        UpdateNumberTextFromNumber();
    }

    partial void OnNumberTextChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Number = null;
            return;
        }

        if (TryConvertToNumber(value.Trim(), out var parsed, out var _))
        {
            if (parsed < 0)
            {
                Number = 0;
            }
            else if (parsed > MaxUnicodeValue)
            {
                Number = MaxUnicodeValue;
            }
            else
            {
                Number = parsed;
            }
            UpdateNumberTextFromNumber();
        }
        else
        {
            Number = null;
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

    private void UpdateNumberTextFromNumber()
    {
        try
        {
            if (Number.HasValue)
            {
                NumberText = UnicodeFormat switch
                {
                    UnicodeFormat.Decimal => Number.Value.ToString(CultureInfo.InvariantCulture),
                    UnicodeFormat.Octal => Convert.ToString(Number.Value, 8),
                    UnicodeFormat.Hexadecimal => "0x" + Number.Value.ToString("X", CultureInfo.InvariantCulture),
                    _ => throw new InvalidOperationException($"Unsupported Unicode format: {UnicodeFormat}"),
                };
            }
            else
            {
                NumberText = string.Empty;
            }
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to update number text from number.", e);
        }
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        UnicodeFormatLocalized.UpdateLabels(AllUnicodeFormats);
    }
}
