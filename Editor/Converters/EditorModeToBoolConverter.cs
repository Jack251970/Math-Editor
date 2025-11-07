using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Editor;

public sealed class EditorModeToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        return value is EditorMode mode && mode == EditorMode.Text;
    }

    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        return value is bool mode && mode ? EditorMode.Text : EditorMode.Math;
    }
}
