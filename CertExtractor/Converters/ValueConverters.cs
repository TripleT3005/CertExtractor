using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace CertExtractor.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool v && v) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InvertedBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !(value is bool v && v);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return !(value is bool v && v);
    }
}