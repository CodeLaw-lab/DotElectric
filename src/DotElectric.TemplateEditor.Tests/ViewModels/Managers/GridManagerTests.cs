using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels.Managers;

namespace DotElectric.TemplateEditor.Tests.ViewModels.Managers;

public class GridManagerTests
{
    private static Template CreateTemplate()
    {
        var sheet = Sheet.FromFormat("A4", SheetOrientation.Landscape);
        var metadata = new Metadata();
        return new Template(metadata, sheet);
    }

    private static ZoomPanManager CreateZoomPanManager(Template template)
    {
        return new ZoomPanManager(template, () => { }, () => { });
    }

    private static GridManager CreateSut(GridSettings? gs = null, Template? template = null, ZoomPanManager? zpm = null)
    {
        template ??= CreateTemplate();
        gs ??= new GridSettings { Enabled = true, Visible = true, StepMicrons = 5000 };
        zpm ??= CreateZoomPanManager(template);
        var sut = new GridManager(template, gs, zpm);
        return sut;
    }

    [Fact]
    public void Constructor_ThrowsOnNullTemplate()
    {
        var gs = new GridSettings();
        var zpm = CreateZoomPanManager(CreateTemplate());

        var ex = Assert.Throws<ArgumentNullException>(() => new GridManager(null!, gs, zpm));
        Assert.Equal("template", ex.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsOnNullGridSettings()
    {
        var template = CreateTemplate();
        var zpm = CreateZoomPanManager(template);

        var ex = Assert.Throws<ArgumentNullException>(() => new GridManager(template, null!, zpm));
        Assert.Equal("gridSettings", ex.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsOnNullZoomPanManager()
    {
        var template = CreateTemplate();
        var gs = new GridSettings();

        var ex = Assert.Throws<ArgumentNullException>(() => new GridManager(template, gs, null!));
        Assert.Equal("zoomPanManager", ex.ParamName);
    }

    [Fact]
    public void Constructor_RawNodeCountZero()
    {
        var sut = CreateSut(new GridSettings());
        Assert.Equal(0, sut.RawNodeCount);
    }

    [Fact]
    public void ToggleGrid_TogglesEnabled()
    {
        var gs = new GridSettings { Enabled = false, Visible = true, StepMicrons = 5000 };
        var sut = CreateSut(gs);

        sut.ToggleGrid();

        Assert.True(gs.Enabled);
    }

    [Fact]
    public void ToggleGrid_TogglesDisabled()
    {
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 5000 };
        var sut = CreateSut(gs);

        sut.ToggleGrid();

        Assert.False(gs.Enabled);
    }

    [Fact]
    public void ToggleSnap_TogglesSnapEnabled()
    {
        var gs = new GridSettings { SnapEnabled = false };
        var sut = CreateSut(gs);

        sut.ToggleSnap();

        Assert.True(gs.SnapEnabled);
    }

    [Fact]
    public void IsGridEnabled_Getter_ReturnsEnabledAndVisible()
    {
        var gs = new GridSettings { Enabled = true, Visible = true };
        var sut = CreateSut(gs);

        Assert.True(sut.IsGridEnabled);
    }

    [Fact]
    public void IsGridEnabled_Setter_UpdatesBothEnabledAndVisible()
    {
        var gs = new GridSettings { Enabled = false, Visible = false };
        var sut = CreateSut(gs);

        sut.IsGridEnabled = true;

        Assert.True(gs.Enabled);
        Assert.True(gs.Visible);
    }

    [Fact]
    public void IsGridEnabled_SetterFalse_ClearsNodes()
    {
        var sut = CreateSut();
        sut.RefreshGridNodes();

        sut.IsGridEnabled = false;

        Assert.False(sut.IsGridEnabled);
        Assert.Equal(0, sut.RawNodeCount);
    }

    [Fact]
    public void IsSnapEnabled_Getter_ReturnsGridSettingsValue()
    {
        var gs = new GridSettings { SnapEnabled = true };
        var sut = CreateSut(gs);

        Assert.True(sut.IsSnapEnabled);
    }

    [Fact]
    public void IsSnapEnabled_Setter_UpdatesGridSettings()
    {
        var gs = new GridSettings { SnapEnabled = true };
        var sut = CreateSut(gs);

        sut.IsSnapEnabled = false;

        Assert.False(gs.SnapEnabled);
    }

    [Fact]
    public void GridStepMm_Getter_ReturnsMmValue()
    {
        var gs = new GridSettings { StepMicrons = 5000 };
        var sut = CreateSut(gs);

        Assert.Equal(5.0, sut.GridStepMm);
    }

    [Fact]
    public void GridStepMm_Setter_UpdatesMicrons()
    {
        var sut = CreateSut();

        sut.GridStepMm = 10.0;

        Assert.Equal(10000L, sut.GridStepMicrons);
    }

    [Fact]
    public void GridStepMicrons_ReturnsRawMicrons()
    {
        var gs = new GridSettings { StepMicrons = 2500 };
        var sut = CreateSut(gs);

        Assert.Equal(2500L, sut.GridStepMicrons);
    }

    [Fact]
    public void RefreshGridNodes_GridDisabled_ClearsNodes()
    {
        var gs = new GridSettings { Enabled = false, Visible = true, StepMicrons = 5000 };
        var sut = CreateSut(gs);

        sut.RefreshGridNodes();

        Assert.Equal(0, sut.RawNodeCount);
    }

    [Fact]
    public void RefreshGridNodes_GridNotVisible_ClearsNodes()
    {
        var gs = new GridSettings { Enabled = true, Visible = false, StepMicrons = 5000 };
        var sut = CreateSut(gs);

        sut.RefreshGridNodes();

        Assert.Equal(0, sut.RawNodeCount);
    }

    [Fact]
    public void RefreshGridNodes_AdaptsStepAtLowZoom()
    {
        // At minimum zoom (0.1×), target = 0.5/0.1 = 5mm, nearest nice step = 5mm
        // A4L (297×210mm) at 5mm step: 60×43=2580 nodes
        var template = CreateTemplate();
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 1000 };
        var zpm = CreateZoomPanManager(template);
        zpm.SetZoom(EditorSettings.ZoomMin);
        var sut = new GridManager(template, gs, zpm);
        sut.RefreshGridNodes();

        Assert.True(sut.RawNodeCount > 0, "Grid should generate nodes even at min zoom");
        // 2580 nodes for A4L at 5mm step (full-sheet fallback when viewport has no size)
        Assert.Equal(2580, sut.RawNodeCount);
    }

    [Fact]
    public void RefreshGridNodes_GeneratesNodes()
    {
        var sut = CreateSut();
        sut.RefreshGridNodes();

        Assert.True(sut.RawNodeCount > 0);
    }

    [Fact]
    public void RefreshGridNodes_GridInvalidatedCallback_Invoked()
    {
        var gs = new GridSettings { Enabled = false, Visible = true, StepMicrons = 5000 };
        var sut = CreateSut(gs);
        var invoked = false;
        sut.GridInvalidated = () => invoked = true;

        sut.RefreshGridNodes();

        Assert.True(invoked);
    }

    [Fact]
    public void RefreshGridNodes_GridInvalidatedCallback_InvokedAfterGeneration()
    {
        var sut = CreateSut();
        var invoked = false;
        sut.GridInvalidated = () => invoked = true;

        sut.RefreshGridNodes();

        Assert.True(invoked);
    }

    [Fact]
    public void RefreshGridNodes_GeneratedNodes_HaveNonNegativeCoordinates()
    {
        var sut = CreateSut();
        sut.RefreshGridNodes();

        Assert.True(sut.RawNodeCount > 0);
        for (int i = 0; i < sut.RawNodeCount; i++)
        {
            Assert.True(sut.RawNodeData[i * 2] >= 0, $"Node {i} X is negative");
            Assert.True(sut.RawNodeData[i * 2 + 1] >= 0, $"Node {i} Y is negative");
        }
    }

    [Fact]
    public void RefreshGridNodes_AllocatesNewArrayEachCall()
    {
        var sut = CreateSut();
        sut.RefreshGridNodes();
        var firstArray = sut.RawNodeData;

        sut.RefreshGridNodes();
        var secondArray = sut.RawNodeData;

        // Each refresh allocates a new array — no shared mutable state
        Assert.NotSame(firstArray, secondArray);
    }

    [Fact]
    public void RefreshGridNodes_FullSheet_GeneratesExpectedNodes()
    {
        var template = CreateTemplate();
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 1000 };
        var zpm = CreateZoomPanManager(template);
        zpm.SetZoom(2.0);
        var sut = new GridManager(template, gs, zpm);

        sut.RefreshGridNodes();

        // Full-sheet A4L (297x210mm) at zoom 2.0: target = 0.5/2 = 0.25mm
        // nearest = 0.5mm (500 microns) → 595×421 = 250,495 > 250K → coarsen to 0.7mm
        // display = 700 microns → 425×301 = 127,925 nodes
        Assert.True(sut.RawNodeCount > 0, "Should have some nodes");
        Assert.True(sut.RawNodeCount <= EditorSettings.MaxGridNodes,
            $"Nodes ({sut.RawNodeCount}) exceed budget ({EditorSettings.MaxGridNodes})");
    }

    [Fact]
    public void RefreshGridNodes_GeneratesNodes_AtHighZoom()
    {
        // A4 portrait (210x297mm) at zoom 7.51x with viewport set
        var sheet = Sheet.FromFormat("A4");
        var template = new Template(new Metadata(), sheet);
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 5000 };
        var zpm = new ZoomPanManager(template, () => { }, () => { });
        zpm.SetZoom(7.51);
        zpm.SetViewportSize(1920, 1080);
        var sut = new GridManager(template, gs, zpm);

        sut.RefreshGridNodes();

        // At zoom 7.51, viewport is small → GetViewportMicrons(1.5) returns viewport region
        // target = 0.5/7.51 ≈ 0.067mm → nearest nice step = 0.5mm → culled by node budget
        Assert.True(sut.RawNodeCount > 0, "Should generate nodes at high zoom");
        Assert.True(sut.RawNodeCount <= EditorSettings.MaxGridNodes,
            $"Nodes ({sut.RawNodeCount}) exceed budget ({EditorSettings.MaxGridNodes})");
    }

    [Fact]
    public void GridStepMm_Setter_RaisesPropertyChanged()
    {
        var sut = CreateSut();
        var propertyChanged = false;
        sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GridManager.GridStepMm))
                propertyChanged = true;
        };

        sut.GridStepMm = 7.5;

        Assert.True(propertyChanged);
    }
}
