using DotElectric.TemplateEditor.Models;
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
    public void Save_NullSettings_ThrowsArgumentNullException()
    {
        var service = CreateService();
        Assert.Throws<ArgumentNullException>(() => service.Save(null!));
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

    // === Round-trip полей сетки (GridMaxNodes / GridNodeColor / GridNodeSize) ===

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
    public void SaveLoad_GridNodeSettings_RoundTripThroughFile()
    {
        // Arrange
        var writer = CreateService();
        var settings = writer.Load();
        settings.GridMaxNodes = 100000;
        settings.GridNodeColor = "#FF8800";
        settings.GridNodeSize = 3.5;
        writer.Save(settings);

        // Act — новый инстанс на том же файле: чтение идёт из JSON, а не из кэша
        var reader = new SettingsService(settingsFilePath: _testSettingsFile);
        var loaded = reader.Load();

        // Assert — AppSettings десериализован корректно
        Assert.Equal(100000, loaded.GridMaxNodes);
        Assert.Equal("#FF8800", loaded.GridNodeColor);
        Assert.Equal(3.5, loaded.GridNodeSize);

        // Assert — ключи реально сериализованы в JSON-файл
        var json = File.ReadAllText(_testSettingsFile);
        Assert.Contains("GridMaxNodes", json);
        Assert.Contains("GridNodeColor", json);
        Assert.Contains("GridNodeSize", json);
    }

    [Fact]
    public void SaveLoad_GridNodeColor_Null_RoundTripThroughFile()
    {
        // Arrange — сначала явный цвет, затем сброс в null (null = авто по теме)
        var writer = CreateService();
        var settings = writer.Load();
        settings.GridNodeColor = "#FF0000";
        writer.Save(settings);
        settings.GridNodeColor = null;
        writer.Save(settings);

        // Act — новый инстанс: десериализация из JSON
        var reader = new SettingsService(settingsFilePath: _testSettingsFile);
        var loaded = reader.Load();

        // Assert
        Assert.Null(loaded.GridNodeColor);
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
    public void Load_LegacyFileWithCustomSettingsKey_IgnoresUnknownKey()
    {
        // Файл от предыдущей версии приложения может содержать удалённый ключ CustomSettings —
        // десериализация должна игнорировать его, сохраняя остальные значения.
        File.WriteAllText(_testSettingsFile, """
            {
              "Theme": "Dark",
              "GridStepMm": 10.0,
              "CustomSettings": { "legacyKey": "legacyValue" }
            }
            """);

        var service = CreateService();
        var settings = service.Load();

        Assert.Equal("Dark", settings.Theme);
        Assert.Equal(10.0, settings.GridStepMm);
    }
}
