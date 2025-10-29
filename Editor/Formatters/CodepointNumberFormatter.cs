using System;
using iNKORE.UI.WPF.Modern.Controls;

namespace Editor;

public class CodepointNumberFormatter(CodepointWindow codepointWindow) : INumberBoxNumberFormatter
{
    private readonly CodepointWindow _codepointWindow = codepointWindow;

    public string FormatDouble(double value)
    {
        var rounded = (long)Math.Round(value);
        return rounded.ToString();
    }

    public double? ParseDouble(string text)
    {
        if (!string.IsNullOrWhiteSpace(text) &&
            CodepointWindow.TryConvertToNumber(_codepointWindow.UnicodeFormat, text, out var result, out _))
        {
            return result;
        }
        return null;
    }
}
