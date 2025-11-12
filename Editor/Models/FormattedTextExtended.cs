using System.Globalization;
using Avalonia.Media;

namespace Editor;

public sealed class FormattedTextExtended(string textToFormat, CultureInfo culture,
    FlowDirection flowDirection, Typeface typeface, double emSize, IBrush? foreground) :
    FormattedText(textToFormat, culture, flowDirection, typeface, emSize, foreground)
{
    private readonly string _text = textToFormat;

    /// <summary>
    /// Returns the string of text to be displayed
    /// </summary>
    public string Text => _text;
}
