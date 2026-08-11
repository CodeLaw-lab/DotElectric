using System.Windows;
using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// Тесты enum-маппингов WpfMessageBoxProvider (чистые, без WPF-элементов).
/// </summary>
public class WpfMessageBoxProviderTests
{
    [Theory]
    [InlineData(MsgrButtons.OK, MessageBoxButton.OK)]
    [InlineData(MsgrButtons.OKCancel, MessageBoxButton.OKCancel)]
    [InlineData(MsgrButtons.YesNoCancel, MessageBoxButton.YesNoCancel)]
    [InlineData(MsgrButtons.YesNo, MessageBoxButton.YesNo)]
    public void ToWpfButtons_MapsKnownValue(MsgrButtons buttons, MessageBoxButton expected)
    {
        Assert.Equal(expected, WpfMessageBoxProvider.ToWpfButtons(buttons));
    }

    [Fact]
    public void ToWpfButtons_UnknownValue_FallsBackToOk()
    {
        Assert.Equal(MessageBoxButton.OK, WpfMessageBoxProvider.ToWpfButtons((MsgrButtons)999));
    }

    [Theory]
    [InlineData(MsgrIcon.None, MessageBoxImage.None)]
    [InlineData(MsgrIcon.Information, MessageBoxImage.Information)]
    [InlineData(MsgrIcon.Warning, MessageBoxImage.Warning)]
    [InlineData(MsgrIcon.Error, MessageBoxImage.Error)]
    [InlineData(MsgrIcon.Question, MessageBoxImage.Question)]
    public void ToWpfIcon_MapsKnownValue(MsgrIcon icon, MessageBoxImage expected)
    {
        Assert.Equal(expected, WpfMessageBoxProvider.ToWpfIcon(icon));
    }

    [Fact]
    public void ToWpfIcon_UnknownValue_FallsBackToNone()
    {
        Assert.Equal(MessageBoxImage.None, WpfMessageBoxProvider.ToWpfIcon((MsgrIcon)999));
    }

    [Theory]
    [InlineData(MessageBoxResult.OK, MsgrResult.OK)]
    [InlineData(MessageBoxResult.Cancel, MsgrResult.Cancel)]
    [InlineData(MessageBoxResult.Yes, MsgrResult.Yes)]
    [InlineData(MessageBoxResult.No, MsgrResult.No)]
    [InlineData(MessageBoxResult.None, MsgrResult.None)]
    public void ToMsgrResult_MapsKnownValue(MessageBoxResult result, MsgrResult expected)
    {
        Assert.Equal(expected, WpfMessageBoxProvider.ToMsgrResult(result));
    }

    [Fact]
    public void ToMsgrResult_UnknownValue_FallsBackToNone()
    {
        Assert.Equal(MsgrResult.None, WpfMessageBoxProvider.ToMsgrResult((MessageBoxResult)999));
    }
}
