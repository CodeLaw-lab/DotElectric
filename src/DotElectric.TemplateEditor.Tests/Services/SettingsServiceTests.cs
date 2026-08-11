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

    // === Round-trip новых полей сетки (GridMaxNodes / GridNodeColor / GridNodeSize) ===

    [Fact]
    public void Load_FirstTime_GridNodeDefaults()
    {
        // Arrange
        var service = CreateService();

        // Act
        var settings = service.Load();

        // Assert
        Assert.Equal(250000, settings.GridMaxNodes);
        Assert.Null(settings.GridNodeColor);
        Assert.Equal(2.0, settings.GridNodeSize);
    }

    [Fact]
    public void SetGet_GridNodeSettings_RoundTripThroughFile()
    {
        // Arrange
        var writer = CreateService();
        writer.Set("GridMaxNodes", 100000);
        writer.Set("GridNodeColor", "#FF8800");
        writer.Set("GridNodeSize", 3.5);

        // Act — новый инстанс на том же файле: чтение идёт из JSON, а не из кэша
        var reader = new SettingsService(settingsFilePath: _testSettingsFile);
        var loaded = reader.Load();

        // Assert — AppSettings десериализован корректно
        Assert.Equal(100000, loaded.GridMaxNodes);
        Assert.Equal("#FF8800", loaded.GridNodeColor);
        Assert.Equal(3.5, loaded.GridNodeSize);

        // Assert — Get<T> по ключам возвращает те же значения
        Assert.Equal(100000, reader.Get("GridMaxNodes", 0));
        Assert.Equal("#FF8800", reader.Get("GridNodeColor", "fallback"));
        Assert.Equal(3.5, reader.Get("GridNodeSize", 0.0));

        // Assert — ключи реально сериализованы в JSON-файл
        var json = File.ReadAllText(_testSettingsFile);
        Assert.Contains("GridMaxNodes", json);
        Assert.Contains("GridNodeColor", json);
        Assert.Contains("GridNodeSize", json);
    }

    [Fact]
    public void SetGet_GridNodeColor_Null_RoundTripThroughFile()
    {
        // Arrange — сначала явный цвет, затем сброс в null (null = авто по теме)
        var writer = CreateService();
        writer.Set("GridNodeColor", "#FF0000");
        writer.Set<string?>("GridNodeColor", null);

        // Act — новый инстанс: десериализация из JSON
        var reader = new SettingsService(settingsFilePath: _testSettingsFile);
        var loaded = reader.Load();

        // Assert
        Assert.Null(loaded.GridNodeColor);
        Assert.Null(reader.Get<string?>("GridNodeColor", null));
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

    [Fact]
    public void Load_CorruptJson_ReturnsDefaults()
    {
        File.WriteAllText(_testSettingsFile, "this is not json {{");

        var service = CreateService();
        var settings = service.Load();

        // Файл повреждён → возвращаются настройки по умолчанию, без исключения
        Assert.NotNull(settings);
        Assert.Equal(250000, settings.GridMaxNodes);
    }

    [Fact]
    public void Get_CustomSettings_ConvertibleValue_ReturnsConverted()
    {
        var service = CreateService();
        service.Set("CustomIntKey", "42");

        var result = service.Get("CustomIntKey", 0);

        Assert.Equal(42, result);
    }

    [Fact]
    public void Get_CustomSettings_NonConvertibleValue_ReturnsDefault()
    {
        var service = CreateService();
        service.Set("CustomBadKey", "not-a-number");

        var result = service.Get("CustomBadKey", 0);

        // Convert.ChangeType("not-a-number" → int) не удаётся → default
        Assert.Equal(0, result);
    }

    [Fact]
    public void Set_CustomKey_StoresInCustomSettings()
    {
        var service = CreateService();
        service.Set("MyCustomKey", "myValue");

        var settings = service.Load();
        Assert.True(settings.CustomSettings.ContainsKey("MyCustomKey"));
        Assert.Equal("myValue", settings.CustomSettings["MyCustomKey"]);

        var result = service.Get("MyCustomKey", "fallback");
        Assert.Equal("myValue", result);
    }

    [Fact]
    public void Set_NullKey_DoesNothing()
    {
        var service = CreateService();
        var before = service.Load();

        service.Set<string?>(null!, "value");

        var after = service.Load();
        Assert.Same(before, after); // кэш не пересоздан, Save не вызван
    }

    [Fact]
    public void Get_LastUsedOrientation_NonStringDefault_ReturnsDefault()
    {
        var service = CreateService();
        service.Set("LastUsedSheetOrientation", "Portrait");

        // default не string → ветка `defaultValue is string` не выполняется → возвращается default
        var result = service.Get("LastUsedSheetOrientation", 42);

        Assert.Equal(42, result);
    }
}
