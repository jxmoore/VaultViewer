using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VaultViewer;

/// <summary>Shows an element (Visible) when the bound string is null/empty. Used for watermarks.</summary>
public sealed class StringEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Visible when the bound bool is false (e.g. show a "disabled" badge when Enabled == false).</summary>
public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Visible when the bound int is greater than zero; collapsed otherwise.</summary>
public sealed class PositiveIntToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int i && i > 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Inverts a boolean.</summary>
public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b ? !b : value!;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b ? !b : value!;
}

/// <summary>Converts a hex colour string (e.g. "#FF5A2C" or "FF5A2C") to a SolidColorBrush.</summary>
public sealed class HexToColorBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex) return Brushes.Transparent;
        var s = hex.TrimStart('#');
        if (s.Length == 6) s = "FF" + s;
        if (s.Length != 8 || !uint.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out var argb))
            return Brushes.Transparent;
        return new SolidColorBrush(Color.FromArgb(
            (byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
