using DotElectric.TemplateEditor.Services;
using System.IO;

namespace DotElectric.TemplateEditor.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _testFolder;
    private readonly string _testSettingsFile;

    public SettingsServiceTests()
    {
        // Каждый тест — уникальная папка
        _testFolder = Path.Combine(Path.GetTempPath(), $"settings_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testFolder);
        _testSettingsFile = Path.Combine(_testFolder, "settings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testFolder))
            Directory.Delete(_testFolder, true);
    }

    private SettingsService CreateService()
    {
        return new SettingsService(settingsFilePath: _testSettingsFile);
    }

    [Fact]
    public void Load_FirstTime_ReturnsDefaults()
    {
        var service = CreateService();
        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.True(settings.AutosaveIntervalMinutes > 0);
    }

    [Fact]
    public void SaveAndLoad_PreservesValues()
    {
        var service = CreateService();
        var original = service.Load();
        var settings = new AppSettings
        {
            AutosaveIntervalMinutes = 10,
            Theme = "Dark",
            ShowGrid = false,
            SnapToGrid = false,
            GridStepMm = 10.0,
            DefaultZoom = 2.0,
            DefaultSheetFormat = "A4"
        };

        service.Save(settings);
        var loaded = service.Load();

        Assert.Equal(10, loaded.AutosaveIntervalMinutes);
        Assert.Equal("Dark", loaded.Theme);
        Assert.False(loaded.ShowGrid);

        // Восстанавливаем оригинальные
        service.Save(original);
    }

    [Fact]
    public void Get_KnownKey_ReturnsValue()
    {
        var service = CreateService();
        var result = service.Get("AutosaveIntervalMinutes", 0);
        Assert.True(result > 0);
    }

    [Fact]
    public void Get_UnknownKey_ReturnsDefault()
    {
        var service = CreateService();
        var result = service.Get("NonExistentKey_12345", "fallback");
        Assert.Equal("fallback", result);
    }

    [Fact]
    public void Get_EmptyKey_ReturnsDefault()
    {
        var service = CreateService();
        var result = service.Get("", "fallback");
        Assert.Equal("fallback", result);
    }

    [Fact]
    public void Set_KnownKey_UpdatesAndSaves()
    {
        var service = CreateService();
        var original = service.Load();
        service.Set("Theme", "Dark");
        var result = service.Get("Theme", "Light");
        Assert.Equal("Dark", result);
        // Восстанавливаем
        service.Set("Theme", original.Theme);
    }

    [Fact]
    public void Set_EmptyKey_DoesNothing()
    {
        var service = CreateService();
        service.Set("", "value");
        // Не должно бросать исключение
    }

    [Fact]
    public void Save_NullSettings_ThrowsArgumentNullException()
    {
        var service = CreateService();
        Assert.Throws<ArgumentNullException>(() => service.Save(null!));
    }

    // === LastUsedSheetFormat ===

    [Fact]
    public void Get_LastUsedSheetFormat_ReturnsDefault()
    {
        var service = CreateService();
        service.Set("LastUsedSheetFormat", "A3");
        var result = service.Get("LastUsedSheetFormat", "A3");
        Assert.Equal("A3", result);
    }

    [Fact]
    public void Set_LastUsedSheetFormat_PersistsValue()
    {
        var service = CreateService();
        service.Set("LastUsedSheetFormat", "A0");
        var result = service.Get("LastUsedSheetFormat", "A3");
        Assert.Equal("A0", result);
    }

    [Fact]
    public void Get_DefaultSheetFormat_ReturnsDefault()
    {
        var service = CreateService();
        // Сбрасываем в случай предыдущих тестов
        service.Set("DefaultSheetFormat", "A3");
        var result = service.Get("DefaultSheetFormat", "A3");
        Assert.Equal("A3", result);
    }

    // === Расширенные тесты Get/Set для всех типов настроек ===

    [Fact]
    public void Get_Set_AutosaveIntervalMinutes_WorksCorrectly()
    {
        var service = CreateService();
        var original = service.Load();
        
        service.Set("AutosaveIntervalMinutes", 5);
        Assert.Equal(5, service.Get("AutosaveIntervalMinutes", 0));
        
        service.Set("AutosaveIntervalMinutes", 15);
        Assert.Equal(15, service.Get("AutosaveIntervalMinutes", 0));
        
        service.Set("AutosaveIntervalMinutes", original.AutosaveIntervalMinutes);
    }

    [Fact]
    public void Get_Set_ShowGrid_WorksCorrectly()
    {
        var service = CreateService();
        var original = service.Load();
        
        service.Set("ShowGrid", true);
        Assert.True(service.Get("ShowGrid", false));
        
        service.Set("ShowGrid", false);
        Assert.False(service.Get("ShowGrid", true));
        
        service.Set("ShowGrid", original.ShowGrid);
    }

    [Fact]
    public void Get_Set_SnapToGrid_WorksCorrectly()
    {
        var service = CreateService();
        var original = service.Load();
        
        service.Set("SnapToGrid", false);
        Assert.False(service.Get("SnapToGrid", true));
        
        service.Set("SnapToGrid", true);
        Assert.True(service.Get("SnapToGrid", false));
        
        service.Set("SnapToGrid", original.SnapToGrid);
    }

    [Fact]
    public void Get_Set_GridStepMm_WorksCorrectly()
    {
        var service = CreateService();
        var original = service.Load();
        
        service.Set("GridStepMm", 5.0);
        Assert.Equal(5.0, service.Get("GridStepMm", 0.0));
        
        service.Set("GridStepMm", 10.0);
        Assert.Equal(10.0, service.Get("GridStepMm", 0.0));
        
        service.Set("GridStepMm", original.GridStepMm);
    }

    [Fact]
    public void Get_Set_DefaultZoom_WorksCorrectly()
    {
        var service = CreateService();
        var original = service.Load();
        
        service.Set("DefaultZoom", 1.0);
        Assert.Equal(1.0, service.Get("DefaultZoom", 0.0));
        
        service.Set("DefaultZoom", 2.5);
        Assert.Equal(2.5, service.Get("DefaultZoom", 0.0));
        
        service.Set("DefaultZoom", original.DefaultZoom);
    }

    [Fact]
    public void Get_Set_Theme_WorksCorrectly()
    {
        var service = CreateService();
        var original = service.Load();
        
        service.Set("Theme", "Light");
        Assert.Equal("Light", service.Get("Theme", "Dark"));
        
        service.Set("Theme", "Dark");
        Assert.Equal("Dark", service.Get("Theme", "Light"));
        
        service.Set("Theme", original.Theme);
    }

    [Fact]
    public void Get_Set_LastUsedSheetFormat_WorksCorrectly()
    {
        var service = CreateService();
        var original = service.Load();
        
        service.Set("LastUsedSheetFormat", "A4");
        Assert.Equal("A4", service.Get("LastUsedSheetFormat", "A3"));
        
        service.Set("LastUsedSheetFormat", "A3");
        Assert.Equal("A3", service.Get("LastUsedSheetFormat", "A4"));
        
        service.Set("LastUsedSheetFormat", original.LastUsedSheetFormat);
    }

    [Fact]
    public void Get_Set_LastUsedSheetOrientation_WorksCorrectly()
    {
        var service = CreateService();
        var original = service.Load();
        
        service.Set("LastUsedSheetOrientation", "Portrait");
        Assert.Equal("Portrait", service.Get("LastUsedSheetOrientation", "Landscape"));
        
        service.Set("LastUsedSheetOrientation", "Landscape");
        Assert.Equal("Landscape", service.Get("LastUsedSheetOrientation", "Portrait"));
        
        service.Set("LastUsedSheetOrientation", original.LastUsedSheetOrientation);
    }

    [Fact]
    public void Get_NullKey_ReturnsDefault()
    {
        var service = CreateService();
        var result = service.Get<string?>(null!, "fallback");
        Assert.Equal("fallback", result);
    }

    [Fact]
    public void Get_WhiteSpaceKey_ReturnsDefault()
    {
        var service = CreateService();
        var result = service.Get("   ", "fallback");
        Assert.Equal("fallback", result);
    }

    [Fact]
    public void Set_NullValue_ForReferenceType_Works()
    {
        var service = CreateService();
        // Не должно бросать исключение
        service.Set<string>("Theme", null!);
    }

    [Fact]
    public void Get_CustomSettings_Key_ReturnsDefaultValue()
    {
        var service = CreateService();
        // CustomSettings пустой по умолчанию
        var result = service.Get("CustomKey", "default");
        Assert.Equal("default", result);
    }

    [Fact]
    public void Get_WrongType_ReturnsDefaultValue()
    {
        var service = CreateService();
        service.Set("Theme", "Dark");
        
        // Пытаемся получить как int (неправильный тип)
        var result = service.Get("Theme", 0);
        Assert.Equal(0, result); // Возвращает default
    }

    [Fact]
    public void Set_Setting_UpdatesCachedSettings()
    {
        var service = CreateService();
        service.Set<int>("AutosaveIntervalMinutes", 30);
        
        // Проверяем, что кэш обновился
        var settings = service.Load();
        Assert.Equal(30, settings.AutosaveIntervalMinutes);
    }

    [Fact]
    public void Load_CachedSettings_ReturnsSameInstance()
    {
        var service = CreateService();
        var first = service.Load();
        var second = service.Load();
        
        // Должен вернуться кэшированный экземпляр
        Assert.Same(first, second);
    }

    [Fact]
    public void Save_ThenLoad_ReturnsSameValues()
    {
        var service = CreateService();
        var original = service.Load();
        
        var newSettings = new AppSettings
        {
            AutosaveIntervalMinutes = 20,
            Theme = "Light",
            ShowGrid = true,
            SnapToGrid = true,
            GridStepMm = 1.0,
            DefaultZoom = 1.5,
            DefaultSheetFormat = "A2",
            LastUsedSheetFormat = "A2",
            LastUsedSheetOrientation = "Portrait"
        };
        
        service.Save(newSettings);
        var loaded = service.Load();
        
        Assert.Equal(20, loaded.AutosaveIntervalMinutes);
        Assert.Equal("Light", loaded.Theme);
        Assert.True(loaded.ShowGrid);
        Assert.True(loaded.SnapToGrid);
        Assert.Equal(1.0, loaded.GridStepMm);
        Assert.Equal(1.5, loaded.DefaultZoom);
        Assert.Equal("A2", loaded.DefaultSheetFormat);
        Assert.Equal("A2", loaded.LastUsedSheetFormat);
        Assert.Equal("Portrait", loaded.LastUsedSheetOrientation);
        
        // Восстанавливаем
        service.Save(original);
    }
}
