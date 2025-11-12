using System;
using iNKORE.UI.WPF.Modern.Controls;

namespace Editor;

public class IntegerNumberFormatter : INumberBoxNumberFormatter
{
    public string FormatDouble(double value)
    {
        var rounded = (long)Math.Round(value);
        return rounded.ToString();
    }

    public double? ParseDouble(string text)
    {
        if (double.TryParse(text, out var result))
        {
            return Math.Round(result);
        }
        return null;
    }
}
