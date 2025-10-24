using System;
using System.Globalization;
using System.Windows.Data;

namespace Editor;

[ValueConversion(typeof(bool?), typeof(bool))]
public sealed class NullableBoolToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool? && value is not null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
