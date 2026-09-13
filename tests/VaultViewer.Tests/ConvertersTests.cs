using System.Globalization;
using System.Windows;
using VaultViewer;

namespace VaultViewer.Tests;

public class ConvertersTests
{
    private static readonly Type T = typeof(object);
    private static readonly CultureInfo C = CultureInfo.InvariantCulture;

    // ---- StringEmptyToVisibilityConverter ----

    [Fact]
    public void StringEmpty_null_returns_Visible()
    {
        var result = new StringEmptyToVisibilityConverter().Convert(null, T, null, C);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void StringEmpty_empty_string_returns_Visible()
    {
        var result = new StringEmptyToVisibilityConverter().Convert("", T, null, C);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void StringEmpty_non_empty_string_returns_Collapsed()
    {
        var result = new StringEmptyToVisibilityConverter().Convert("hello", T, null, C);
        Assert.Equal(Visibility.Collapsed, result);
    }

    // ---- InverseBooleanToVisibilityConverter ----

    [Fact]
    public void InverseBoolVisibility_false_returns_Visible()
    {
        var result = new InverseBooleanToVisibilityConverter().Convert(false, T, null, C);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void InverseBoolVisibility_true_returns_Collapsed()
    {
        var result = new InverseBooleanToVisibilityConverter().Convert(true, T, null, C);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void InverseBoolVisibility_null_returns_Collapsed()
    {
        var result = new InverseBooleanToVisibilityConverter().Convert(null, T, null, C);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void InverseBoolVisibility_non_bool_returns_Collapsed()
    {
        var result = new InverseBooleanToVisibilityConverter().Convert("notabool", T, null, C);
        Assert.Equal(Visibility.Collapsed, result);
    }

    // ---- PositiveIntToVisibilityConverter ----

    [Fact]
    public void PositiveInt_positive_returns_Visible()
    {
        var result = new PositiveIntToVisibilityConverter().Convert(1, T, null, C);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void PositiveInt_zero_returns_Collapsed()
    {
        var result = new PositiveIntToVisibilityConverter().Convert(0, T, null, C);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void PositiveInt_negative_returns_Collapsed()
    {
        var result = new PositiveIntToVisibilityConverter().Convert(-5, T, null, C);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void PositiveInt_non_int_returns_Collapsed()
    {
        var result = new PositiveIntToVisibilityConverter().Convert("five", T, null, C);
        Assert.Equal(Visibility.Collapsed, result);
    }

    // ---- InverseBooleanConverter ----

    [Fact]
    public void InverseBool_true_returns_false()
    {
        var result = new InverseBooleanConverter().Convert(true, T, null, C);
        Assert.Equal(false, result);
    }

    [Fact]
    public void InverseBool_false_returns_true()
    {
        var result = new InverseBooleanConverter().Convert(false, T, null, C);
        Assert.Equal(true, result);
    }

    [Fact]
    public void InverseBool_non_bool_passes_through()
    {
        var value = "passthrough";
        var result = new InverseBooleanConverter().Convert(value, T, null, C);
        Assert.Equal(value, result);
    }

    [Fact]
    public void InverseBool_ConvertBack_true_returns_false()
    {
        var result = new InverseBooleanConverter().ConvertBack(true, T, null, C);
        Assert.Equal(false, result);
    }

    [Fact]
    public void InverseBool_ConvertBack_false_returns_true()
    {
        var result = new InverseBooleanConverter().ConvertBack(false, T, null, C);
        Assert.Equal(true, result);
    }
}
