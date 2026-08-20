using System.IO;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.Tests.Models;

public class GridSettingsTests
{
    [Fact]
    public void FromDefaultGrid_CopiesStepAndEnablesAll()
    {
        var settings = GridSettings.FromDefaultGrid();

        Assert.True(settings.Enabled);
        Assert.True(settings.SnapEnabled);
        Assert.True(settings.Visible);
        Assert.Equal(5000, settings.StepMicrons);
    }

    [Fact]
    public void Properties_CanBeModified()
    {
        var settings = GridSettings.FromDefaultGrid();

        settings.Enabled = false;
        settings.SnapEnabled = false;
        settings.Visible = false;
        settings.StepMicrons = 10000;

        Assert.False(settings.Enabled);
        Assert.False(settings.SnapEnabled);
        Assert.False(settings.Visible);
        Assert.Equal(10000, settings.StepMicrons);
    }

    [Fact]
    public void DefaultConstructor_Defaults_AreCorrect()
    {
        var settings = new GridSettings();

        Assert.True(settings.Enabled);
        Assert.True(settings.SnapEnabled);
        Assert.True(settings.Visible);
        Assert.Equal(5000, settings.StepMicrons);
    }

    [Fact]
    public void Constructor_CustomValues_CanBeSetViaProperties()
    {
        var settings = new GridSettings();

        settings.Enabled = false;
        Assert.False(settings.Enabled);

        settings.SnapEnabled = false;
        Assert.False(settings.SnapEnabled);

        settings.Visible = false;
        Assert.False(settings.Visible);

        settings.StepMicrons = 10000;
        Assert.Equal(10000, settings.StepMicrons);
    }

    [Fact]
    public void FromAppSettings_MapsAllFields()
    {
        var settings = GridSettings.FromAppSettings(new AppSettings
        {
            ShowGrid = false,
            SnapToGrid = false,
            GridStepMm = 1.0,
            GridMaxNodes = 50000,
            GridNodeColor = "#FF0000",
            GridNodeSize = 3.5,
        });

        Assert.False(settings.Enabled);
        Assert.False(settings.SnapEnabled);
        Assert.Equal(1000, settings.StepMicrons);
        Assert.True(settings.Visible);
        Assert.Equal(50000, settings.MaxGridNodes);
        Assert.Equal("#FF0000", settings.NodeColor);
        Assert.Equal(3.5, settings.NodeSize);
    }

    [Fact]
    public void FromAppSettings_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => GridSettings.FromAppSettings(null!));
    }

    [Fact]
    public void FromAppSettings_GridStepMmZero_FallsBackToDefaultStep()
    {
        var settings = GridSettings.FromAppSettings(new AppSettings { GridStepMm = 0.0 });

        Assert.Equal(EditorSettings.DefaultGridStepMicrons, settings.StepMicrons);
    }

    [Fact]
    public void FromAppSettings_GridStepMmNegative_FallsBackToDefaultStep()
    {
        var settings = GridSettings.FromAppSettings(new AppSettings { GridStepMm = -5.0 });

        Assert.Equal(EditorSettings.DefaultGridStepMicrons, settings.StepMicrons);
    }

    [Fact]
    public void FromAppSettings_GridStepMmFractional_ConvertsToMicrons()
    {
        var settings = GridSettings.FromAppSettings(new AppSettings { GridStepMm = 0.5 });

        Assert.Equal(500, settings.StepMicrons);
    }

    [Fact]
    public void FromAppSettings_MaxGridNodesZero_FallsBackToDefault()
    {
        var settings = GridSettings.FromAppSettings(new AppSettings { GridMaxNodes = 0 });

        Assert.Equal(EditorSettings.MaxGridNodes, settings.MaxGridNodes);
    }

    [Fact]
    public void FromAppSettings_MaxGridNodesNegative_FallsBackToDefault()
    {
        var settings = GridSettings.FromAppSettings(new AppSettings { GridMaxNodes = -100 });

        Assert.Equal(EditorSettings.MaxGridNodes, settings.MaxGridNodes);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    public void FromAppSettings_NodeSizeInvalid_FallsBackToDefault(double nodeSize)
    {
        var settings = GridSettings.FromAppSettings(new AppSettings { GridNodeSize = nodeSize });

        Assert.Equal(2.0, settings.NodeSize);
    }

    [Fact]
    public void FromAppSettings_NodeColorNull_Preserved()
    {
        var settings = GridSettings.FromAppSettings(new AppSettings { GridNodeColor = null });

        Assert.Null(settings.NodeColor);
    }

    [Fact]
    public void FromAppSettings_MaxGridNodesOne_IsPreserved()
    {
        // Arrange — минимальный допустимый бюджет узлов (1) не должен fallback'иться на дефолт
        var appSettings = new AppSettings { GridMaxNodes = 1 };

        // Act
        var settings = GridSettings.FromAppSettings(appSettings);

        // Assert
        Assert.Equal(1, settings.MaxGridNodes);
    }

    [Fact]
    public void FromAppSettings_NodeSizePositiveInfinity_FallsBackToDefault()
    {
        // Arrange — +∞ не является валидным размером узла
        var appSettings = new AppSettings { GridNodeSize = double.PositiveInfinity };

        // Act
        var settings = GridSettings.FromAppSettings(appSettings);

        // Assert
        Assert.Equal(EditorSettings.DefaultGridNodeSize, settings.NodeSize);
    }
}

public class MockTemplateFileService : IFileService
{
    public string TemplatesFolder { get; }

    public MockTemplateFileService()
    {
        TemplatesFolder = Path.Combine(Path.GetTempPath(), $"TemplateLibTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(TemplatesFolder);
    }

    public string? OpenFileDialog(string filter) => null;
    public string? SaveFileDialog(string filter, string defaultFileName) => null;
    public string GetTemplatesFolder() => TemplatesFolder;
    public string GetBackupFolder() => Path.Combine(Path.GetTempPath(), "BackupTest");
    public void CreateBackup(string sourcePath) { }
}
