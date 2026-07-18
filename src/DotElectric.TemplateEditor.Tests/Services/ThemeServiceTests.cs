using DotElectric.TemplateEditor.Services;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// Тесты для ThemeService с моковыми IThemeDictionaryManager и ISettingsService.
/// </summary>
public class ThemeServiceTests : IDisposable
{
    private readonly Mock<IThemeDictionaryManager> _mockDictionaryManager;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private ThemeService _service;

    private const string LightThemePath = "Resources/Styles/LightTheme.xaml";
    private const string DarkThemePath = "Resources/Styles/DarkTheme.xaml";

    public ThemeServiceTests()
    {
        _mockDictionaryManager = new Mock<IThemeDictionaryManager>();
        _mockSettingsService = new Mock<ISettingsService>();
        _mockSettingsService.Setup(s => s.Get("Theme", "Light")).Returns("Light");

        _service = new ThemeService(_mockSettingsService.Object, _mockDictionaryManager.Object);
    }

    public void Dispose()
    {
        _service = null!;
    }

    // ==================== Constructor Tests ====================

    [Fact]
    public void Constructor_LoadsThemeFromSettings()
    {
        _mockSettingsService.Setup(s => s.Get("Theme", "Light")).Returns("Dark");

        var service = new ThemeService(_mockSettingsService.Object, _mockDictionaryManager.Object);

        Assert.Equal("Dark", service.CurrentTheme);
    }

    [Fact]
    public void Constructor_UsesLightDefault_WhenSettingsHasNoTheme()
    {
        _mockSettingsService.Setup(s => s.Get("Theme", "Light")).Returns("Light");

        var service = new ThemeService(_mockSettingsService.Object, _mockDictionaryManager.Object);

        Assert.Equal("Light", service.CurrentTheme);
    }

    // ==================== CurrentTheme Property Tests ====================

    [Fact]
    public void CurrentTheme_InitialState_ReturnsValueFromSettings()
    {
        Assert.Equal("Light", _service.CurrentTheme);
    }

    [Fact]
    public void CurrentTheme_AfterSetTheme_UpdatesCorrectly()
    {
        _service.SetTheme("Dark");
        Assert.Equal("Dark", _service.CurrentTheme);
    }

    // ==================== SetTheme Tests ====================

    [Fact]
    public void SetTheme_Light_CallsDictionaryManagerWithLightPath()
    {
        _service.SetTheme("Light");

        _mockDictionaryManager.Verify(m => m.SetThemeDictionary(LightThemePath), Times.Once);
        Assert.Equal("Light", _service.CurrentTheme);
    }

    [Fact]
    public void SetTheme_Dark_CallsDictionaryManagerWithDarkPath()
    {
        _service.SetTheme("Dark");

        _mockDictionaryManager.Verify(m => m.SetThemeDictionary(DarkThemePath), Times.Once);
        Assert.Equal("Dark", _service.CurrentTheme);
    }

    [Fact]
    public void SetTheme_Light_SavesToSettings()
    {
        _service.SetTheme("Light");

        _mockSettingsService.Verify(s => s.Set("Theme", "Light"), Times.Once);
    }

    [Fact]
    public void SetTheme_Dark_SavesToSettings()
    {
        _service.SetTheme("Dark");

        _mockSettingsService.Verify(s => s.Set("Theme", "Dark"), Times.Once);
    }

    [Fact]
    public void SetTheme_Null_FallsBackToLight()
    {
        _service.SetTheme(null!);

        _mockDictionaryManager.Verify(m => m.SetThemeDictionary(LightThemePath), Times.Once);
        Assert.Equal("Light", _service.CurrentTheme);
        _mockSettingsService.Verify(s => s.Set("Theme", "Light"), Times.Once);
    }

    [Fact]
    public void SetTheme_EmptyString_FallsBackToLight()
    {
        _service.SetTheme(string.Empty);

        _mockDictionaryManager.Verify(m => m.SetThemeDictionary(LightThemePath), Times.Once);
        Assert.Equal("Light", _service.CurrentTheme);
    }

    [Fact]
    public void SetTheme_UnknownThemeValue_TreatsAsLight()
    {
        // Любое значение, кроме "Dark", трактуется как Light
        _service.SetTheme("SomeUnknownTheme");

        _mockDictionaryManager.Verify(m => m.SetThemeDictionary(LightThemePath), Times.Once);
        Assert.Equal("SomeUnknownTheme", _service.CurrentTheme);
    }

    [Fact]
    public void SetTheme_MultipleCalls_UpdatesCurrentThemeEachTime()
    {
        _service.SetTheme("Dark");
        Assert.Equal("Dark", _service.CurrentTheme);

        _service.SetTheme("Light");
        Assert.Equal("Light", _service.CurrentTheme);

        _service.SetTheme("Dark");
        Assert.Equal("Dark", _service.CurrentTheme);
    }

    // ==================== SetTheme Exception Handling Tests ====================

    [Fact]
    public void SetTheme_DictionaryManagerThrows_CurrentThemeDoesNotChange()
    {
        _mockDictionaryManager
            .Setup(m => m.SetThemeDictionary(DarkThemePath))
            .Throws(new InvalidOperationException("Dictionary error"));

        _service.SetTheme("Dark");

        // CurrentTheme должен остаться "Light" (из конструктора)
        Assert.Equal("Light", _service.CurrentTheme);
    }

    [Fact]
    public void SetTheme_DictionaryManagerThrows_SettingsNotUpdated()
    {
        _mockDictionaryManager
            .Setup(m => m.SetThemeDictionary(DarkThemePath))
            .Throws(new InvalidOperationException("Dictionary error"));

        _service.SetTheme("Dark");

        _mockSettingsService.Verify(s => s.Set("Theme", It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void SetTheme_AfterException_CanStillSetValidTheme()
    {
        // Сначала вызываем исключение
        _mockDictionaryManager
            .Setup(m => m.SetThemeDictionary(DarkThemePath))
            .Throws(new InvalidOperationException("Dictionary error"));

        _service.SetTheme("Dark");
        Assert.Equal("Light", _service.CurrentTheme);

        // Теперь восстанавливаем нормальную работу
        _mockDictionaryManager.Setup(m => m.SetThemeDictionary(It.IsAny<string>()));
        _service.SetTheme("Dark");

        Assert.Equal("Dark", _service.CurrentTheme);
    }

    // ==================== ToggleTheme Tests ====================

    [Fact]
    public void ToggleTheme_FromLight_SwitchesToDark()
    {
        // CurrentTheme = "Light" (из конструктора)
        var result = _service.ToggleTheme();

        Assert.Equal("Dark", result);
        Assert.Equal("Dark", _service.CurrentTheme);
        _mockDictionaryManager.Verify(m => m.SetThemeDictionary(DarkThemePath), Times.Once);
    }

    [Fact]
    public void ToggleTheme_FromDark_SwitchesToLight()
    {
        // Сначала установим Dark
        _service.SetTheme("Dark");

        var result = _service.ToggleTheme();

        Assert.Equal("Light", result);
        Assert.Equal("Light", _service.CurrentTheme);
    }

    [Fact]
    public void ToggleTheme_MultipleTimes_AlternatesCorrectly()
    {
        Assert.Equal("Light", _service.CurrentTheme);

        _service.ToggleTheme();
        Assert.Equal("Dark", _service.CurrentTheme);

        _service.ToggleTheme();
        Assert.Equal("Light", _service.CurrentTheme);

        _service.ToggleTheme();
        Assert.Equal("Dark", _service.CurrentTheme);
    }

    [Fact]
    public void ToggleTheme_SavesToSettings()
    {
        _service.ToggleTheme();

        _mockSettingsService.Verify(s => s.Set("Theme", "Dark"), Times.Once);
    }

    [Fact]
    public void ToggleTheme_FromUnknownTheme_SwitchesToLight()
    {
        // Устанавливаем неизвестную тему напрямую (через SetTheme)
        _service.SetTheme("CustomTheme");
        Assert.Equal("CustomTheme", _service.CurrentTheme);

        var result = _service.ToggleTheme();

        // "CustomTheme" != "Light", значит переключится на Light
        Assert.Equal("Light", result);
        Assert.Equal("Light", _service.CurrentTheme);
    }

    // ==================== Integration-style Tests ====================

    [Fact]
    public void SetTheme_LightThenDark_BothCallsSaved()
    {
        _service.SetTheme("Light");
        _service.SetTheme("Dark");

        _mockSettingsService.Verify(s => s.Set("Theme", "Light"), Times.Once);
        _mockSettingsService.Verify(s => s.Set("Theme", "Dark"), Times.Once);
    }

    [Fact]
    public void SetTheme_SameThemeTwice_UpdatesEachTime()
    {
        _service.SetTheme("Dark");
        _service.SetTheme("Dark");

        _mockDictionaryManager.Verify(m => m.SetThemeDictionary(DarkThemePath), Times.Exactly(2));
        _mockSettingsService.Verify(s => s.Set("Theme", "Dark"), Times.Exactly(2));
    }

    [Fact]
    public void SetTheme_Null_ThenDark_SetsDark()
    {
        _service.SetTheme(null!);
        Assert.Equal("Light", _service.CurrentTheme);

        _service.SetTheme("Dark");
        Assert.Equal("Dark", _service.CurrentTheme);
    }
}
