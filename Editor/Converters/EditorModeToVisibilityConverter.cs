using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Editor;

[ValueConversion(typeof(EditorMode), typeof(Visibility))]
public sealed class EditorModeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is EditorMode mode && mode == EditorMode.Math ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility mode && mode == Visibility.Collapsed ? EditorMode.Math : EditorMode.Text;
    }
}
