using System.IO;
using DotElectric.TemplateEditor.Abstractions;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

[Collection("AutosaveSharedState")]
public class AutosaveServiceTests : IDisposable
{
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<ILogger<AutosaveService>> _mockLogger;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly AutosaveService _service;
    private readonly string _testAutosaveFolder;
    private static readonly DateTime FixedDate = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public AutosaveServiceTests()
    {
        _testAutosaveFolder = Path.Combine(Path.GetTempPath(), $"AutosaveTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testAutosaveFolder);

        _mockTemplateService = new Mock<ITemplateService>();
        _mockTemplateService.Setup(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()));

        _mockSettingsService = new Mock<ISettingsService>();
        _mockSettingsService.Setup(s => s.Get("AutosaveIntervalMinutes", 5)).Returns(5);

        _mockLogger = new Mock<ILogger<AutosaveService>>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        _mockDateTimeProvider.Setup(p => p.UtcNow).Returns(FixedDate);

        _service = new AutosaveService(
            _mockTemplateService.Object,
            _mockSettingsService.Object,
            _mockLogger.Object,
            dateTimeProvider: _mockDateTimeProvider.Object,
            autosaveFolder: _testAutosaveFolder);
    }

    public void Dispose()
    {
        _service.Dispose();
        if (Directory.Exists(_testAutosaveFolder))
            Directory.Delete(_testAutosaveFolder, true);
    }

    [Fact]
    public void Constructor_CreatesService()
    {
        Assert.NotNull(_service);
    }

    [Fact]
    public void Start_DoesNotThrow()
    {
        var ex = Record.Exception(() => _service.Start());
        Assert.Null(ex);
    }

    [Fact]
    public void Start_ThenStop_Works()
    {
        var ex = Record.Exception(() =>
        {
            _service.Start();
            _service.Stop();
        });
        Assert.Null(ex);
    }

    [Fact]
    public void Stop_MultipleCalls_DoesNotThrow()
    {
        var ex = Record.Exception(() =>
        {
            _service.Stop();
            _service.Stop();
        });
        Assert.Null(ex);
    }

    [Fact]
    public void Start_CallsSettingsGetForInterval()
    {
        _service.Start();
        _mockSettingsService.Verify(s => s.Get("AutosaveIntervalMinutes", 5), Times.Once);
    }

    [Fact]
    public void GetAutosaveFilePath_ReturnsCorrectPath()
    {
        var path = _service.GetAutosaveFilePath("test.tdel");
        Assert.Contains("test.tdel", path);
    }

    [Fact]
    public void LoadSession_NoSessionFile_ReturnsNull()
    {
        // Ensure no stale session.json from previous tests
        var sessionFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DotElectric", "autosave", "session.json");
        if (File.Exists(sessionFile))
            File.Delete(sessionFile);

        var session = _service.LoadSession();
        Assert.Null(session);
    }

    [Fact]
    public void LoadSession_InvalidJson_ReturnsNull()
    {
        var sessionFile = Path.Combine(_testAutosaveFolder, "session.json");
        Directory.CreateDirectory(_testAutosaveFolder);
        File.WriteAllText(sessionFile, "invalid json");

        try
        {
            var session = _service.LoadSession();
            Assert.Null(session);
        }
        finally
        {
            if (File.Exists(sessionFile)) File.Delete(sessionFile);
        }
    }

    [Fact]
    public void LoadSession_EmptyJson_ReturnsNull()
    {
        var sessionFile = Path.Combine(_testAutosaveFolder, "session.json");
        Directory.CreateDirectory(_testAutosaveFolder);
        File.WriteAllText(sessionFile, string.Empty);

        try
        {
            var session = _service.LoadSession();
            Assert.Null(session);
        }
        finally
        {
            if (File.Exists(sessionFile)) File.Delete(sessionFile);
        }
    }

    [Fact]
    public void LoadSession_ValidJson_ReturnsCorrectState()
    {
        var sessionFile = Path.Combine(_testAutosaveFolder, "session.json");
        Directory.CreateDirectory(_testAutosaveFolder);
        var json = @"{
                ""SessionStart"": ""2025-01-15T12:00:00Z"",
                ""LastAutosave"": ""2025-01-15T13:00:00Z"",
                ""Tabs"": [
                    {
                        ""TabId"": ""tab1"",
                        ""DisplayName"": ""My Template"",
                        ""OriginalFilePath"": ""C:\\templates\\test.tdel"",
                        ""AutosaveFile"": ""autosave_tab1_20250115_120000.tdel"",
                        ""WasDirty"": true
                    }
                ]
            }";
        File.WriteAllText(sessionFile, json);

        try
        {
            var session = _service.LoadSession();
            Assert.NotNull(session);
            Assert.Equal(new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc), session.SessionStart);
            Assert.Equal(new DateTime(2025, 1, 15, 13, 0, 0, DateTimeKind.Utc), session.LastAutosave);
            Assert.Single(session.Tabs);
            Assert.Equal("tab1", session.Tabs[0].TabId);
            Assert.Equal("My Template", session.Tabs[0].DisplayName);
            Assert.Equal(@"C:\templates\test.tdel", session.Tabs[0].OriginalFilePath);
            Assert.Equal("autosave_tab1_20250115_120000.tdel", session.Tabs[0].AutosaveFile);
            Assert.True(session.Tabs[0].WasDirty);
        }
        finally
        {
            if (File.Exists(sessionFile)) File.Delete(sessionFile);
        }
    }

    [Fact]
    public void ClearAutosaveFolder_DoesNotThrow()
    {
        // Create some files to clear
        Directory.CreateDirectory(_testAutosaveFolder);

        var testFile = Path.Combine(_testAutosaveFolder, "test_clear.txt");
        File.WriteAllText(testFile, "test");

        try
        {
            _service.ClearAutosaveFolder();
            Assert.False(File.Exists(testFile));
        }
        finally
        {
            if (File.Exists(testFile)) File.Delete(testFile);
        }
    }

    [Fact]
    public void ClearAutosaveFolder_NonExistentFolder_DoesNotThrow()
    {
        var ex = Record.Exception(() => _service.ClearAutosaveFolder());
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var ex = Record.Exception(() =>
        {
            _service.Dispose();
            _service.Dispose();
        });
        Assert.Null(ex);
    }

    [Fact]
    public void Start_AfterDispose_ThrowsObjectDisposedException()
    {
        _service.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _service.Start());
    }

    [Fact]
    public async Task AutosaveAllTabsAsync_NoDirtyTabs_DoesNotSave()
    {
        var tabs = new List<MockAutosaveTab>
        {
            new MockAutosaveTab { TabId = "tab1", IsDirty = false }
        };

        await _service.AutosaveAllTabsAsync(tabs, TestContext.Current.CancellationToken);

        _mockTemplateService.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AutosaveAllTabsAsync_DirtyTab_Saves()
    {
        var template = new Template(
            new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            Sheet.FromFormat("A3"));
        template.Objects.Add(new Line(0, 0, 1000, 1000));

        var tabs = new List<MockAutosaveTab>
        {
            new MockAutosaveTab
            {
                TabId = "tab1",
                DisplayName = "Test Tab",
                IsDirty = true,
                Template = template,
                FilePath = null
            }
        };

        await _service.AutosaveAllTabsAsync(tabs, TestContext.Current.CancellationToken);

        _mockTemplateService.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task AutosaveAllTabsAsync_EmptyList_DoesNotThrow()
    {
        var tabs = new List<MockAutosaveTab>();
        var ex = await Record.ExceptionAsync(() => _service.AutosaveAllTabsAsync(tabs, TestContext.Current.CancellationToken));
        Assert.Null(ex);
    }

    [Fact]
    public async Task AutosaveAllTabsAsync_MixedDirty_Clean_OnlySavesDirty()
    {
        var template = new Template(
            new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            Sheet.FromFormat("A3"));

        var tabs = new List<MockAutosaveTab>
        {
            new MockAutosaveTab { TabId = "tab1", IsDirty = true, Template = template },
            new MockAutosaveTab { TabId = "tab2", IsDirty = false, Template = template },
            new MockAutosaveTab { TabId = "tab3", IsDirty = true, Template = template },
        };

        await _service.AutosaveAllTabsAsync(tabs, TestContext.Current.CancellationToken);

        _mockTemplateService.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AutosaveAllTabsAsync_ExceptionInSave_LogsAndContinues()
    {
        _mockTemplateService.Setup(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()))
            .Throws(new IOException("Disk full"));

        var template = new Template(
            new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            Sheet.FromFormat("A3"));

        var tabs = new List<MockAutosaveTab>
        {
            new MockAutosaveTab { TabId = "tab1", IsDirty = true, Template = template }
        };

        await _service.AutosaveAllTabsAsync(tabs, TestContext.Current.CancellationToken);
        // Should not throw, exception is logged
    }
}

/// <summary>
/// Mock implementation of IAutosaveTab for testing.
/// </summary>
public class MockAutosaveTab : IAutosaveTab
{
    public string? TabId { get; set; }
    public string? FilePath { get; set; }
    public string DisplayName { get; set; } = "Mock Tab";
    public bool IsDirty { get; set; }
    public object Template { get; set; } = new Template();
}
