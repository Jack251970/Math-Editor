using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Editor;

public sealed class CopyTypeToLatexVisibleConverter : IValueConverter
{
    public static readonly CopyTypeToLatexVisibleConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is CopyType copyType && copyType == CopyType.Latex;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
