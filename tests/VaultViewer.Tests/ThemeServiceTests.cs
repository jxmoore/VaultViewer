using VaultViewer.Services;
using Xunit;

namespace VaultViewer.Tests;

public class ThemeServiceTests
{
    [Fact]
    public void Current_defaults_to_dark()
    {
        var service = new ThemeService();

        Assert.Equal(AppTheme.Dark, service.Current);
    }

    [Fact]
    public void Apply_updates_Current()
    {
        var service = new ThemeService();

        service.Apply(AppTheme.Light);
        Assert.Equal(AppTheme.Light, service.Current);

        service.Apply(AppTheme.Dark);
        Assert.Equal(AppTheme.Dark, service.Current);
    }
}
