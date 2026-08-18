using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels.Managers;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels.Managers;

public class GridManagerTests
{
    private static Template CreateTemplate()
    {
        var sheet = Sheet.FromFormat("A4", SheetOrientation.Landscape); // 297000x210000
        var metadata = new Metadata();
        return new Template(metadata, sheet);
    }

    private static ZoomPanManager CreateZoomPanManager(Template template)
    {
        return new ZoomPanManager(template);
    }

    private static GridManager CreateSut(
        GridSettings? gs = null,
        Template? template = null,
        ZoomPanManager? zpm = null,
        IGridNodeGenerator? generator = null)
    {
        template ??= CreateTemplate();
        gs ??= new GridSettings { Enabled = true, Visible = true, StepMicrons = 5000 };
        zpm ??= CreateZoomPanManager(template);
        generator ??= new GridNodeGenerator();
        return new GridManager(template, gs, zpm, generator);
    }

    // ===== Constructor =====

    [Fact]
    public void Constructor_ThrowsOnNullTemplate()
    {
        var gs = new GridSettings();
        var zpm = CreateZoomPanManager(CreateTemplate());

        var ex = Assert.Throws<ArgumentNullException>(() => new GridManager(null!, gs, zpm, new GridNodeGenerator()));
        Assert.Equal("template", ex.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsOnNullGridSettings()
    {
        var template = CreateTemplate();
        var zpm = CreateZoomPanManager(template);

        var ex = Assert.Throws<ArgumentNullException>(() => new GridManager(template, null!, zpm, new GridNodeGenerator()));
        Assert.Equal("gridSettings", ex.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsOnNullZoomPanManager()
    {
        var template = CreateTemplate();
        var gs = new GridSettings();

        var ex = Assert.Throws<ArgumentNullException>(() => new GridManager(template, gs, null!, new GridNodeGenerator()));
        Assert.Equal("zoomPanManager", ex.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsOnNullGridNodeGenerator()
    {
        var template = CreateTemplate();
        var gs = new GridSettings();
        var zpm = CreateZoomPanManager(template);

        var ex = Assert.Throws<ArgumentNullException>(() => new GridManager(template, gs, zpm, null!));
        Assert.Equal("gridNodeGenerator", ex.ParamName);
    }

    [Fact]
    public void Constructor_NodesEmpty()
    {
        var sut = CreateSut(new GridSettings());
        Assert.Empty(sut.Nodes);
    }

    // ===== Toggle / Enabled =====

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
        Assert.NotEmpty(sut.Nodes);

        sut.IsGridEnabled = false;

        Assert.False(sut.IsGridEnabled);
        Assert.Empty(sut.Nodes);
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

    // ===== GridStep =====

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

    [Fact]
    public void GridStepMicrons_ReturnsRawMicrons()
    {
        var gs = new GridSettings { StepMicrons = 2500 };
        var sut = CreateSut(gs);

        Assert.Equal(2500L, sut.GridStepMicrons);
    }

    // ===== RefreshGridNodes — guards =====

    [Fact]
    public void RefreshGridNodes_Disabled_EmptyNodes()
    {
        var gs = new GridSettings { Enabled = false, Visible = true, StepMicrons = 5000 };
        var sut = CreateSut(gs);

        sut.RefreshGridNodes();

        Assert.Empty(sut.Nodes);
    }

    [Fact]
    public void RefreshGridNodes_NotVisible_EmptyNodes()
    {
        var gs = new GridSettings { Enabled = true, Visible = false, StepMicrons = 5000 };
        var sut = CreateSut(gs);

        sut.RefreshGridNodes();

        Assert.Empty(sut.Nodes);
    }

    // ===== RefreshGridNodes — generation =====

    [Fact]
    public void RefreshGridNodes_Enabled_GeneratesNodes()
    {
        var sut = CreateSut();

        sut.RefreshGridNodes();

        Assert.NotEmpty(sut.Nodes);
    }

    [Fact]
    public void RefreshGridNodes_NodesInAbsoluteCoordinates()
    {
        // Узлы — абсолютные микроны листа, начинаются с (0,0), кратны шагу
        var sut = CreateSut(); // A4L 297000x210000, step 5000, zoom 1.0
        sut.RefreshGridNodes();

        Assert.NotEmpty(sut.Nodes);
        foreach (var node in sut.Nodes)
        {
            Assert.Equal(0, node.XMicrons % 5000);
            Assert.Equal(0, node.YMicrons % 5000);
            Assert.InRange(node.XMicrons, 0, 297000);
            Assert.InRange(node.YMicrons, 0, 210000);
        }
        Assert.Contains(sut.Nodes, n => n.XMicrons == 0 && n.YMicrons == 0);
    }

    [Fact]
    public void RefreshGridNodes_AdaptsStepAtLowZoom()
    {
        // At minimum zoom (0.1x): user step 1000 даёт pixelSpacing 0.1px < 5.0 → fallback.
        // target = 5.0/0.1 = 50мм → nearest nice = 50000.
        // A4L (297x210мм) at 50мм step: 6x5 = 30 узлов
        var template = CreateTemplate();
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 1000 };
        var zpm = CreateZoomPanManager(template);
        zpm.SetZoom(EditorSettings.ZoomMin);
        var sut = new GridManager(template, gs, zpm, new GridNodeGenerator());

        sut.RefreshGridNodes();

        Assert.NotEmpty(sut.Nodes);
        Assert.Equal(30, sut.Nodes.Count);
    }

    [Fact]
    public void RefreshGridNodes_FullSheet_GeneratesExpectedNodes()
    {
        // A4L (297x210мм) at zoom 2.0, user step 1000: pixelSpacing = 2px < 5.0 → user step отклонён,
        // fallback target = 5.0/2.0 = 2.5мм → nearest nice = 3000: 100*71 = 7100 <= 250000
        var template = CreateTemplate();
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 1000 };
        var zpm = CreateZoomPanManager(template);
        zpm.SetZoom(2.0);
        var sut = new GridManager(template, gs, zpm, new GridNodeGenerator());

        sut.RefreshGridNodes();

        Assert.NotEmpty(sut.Nodes);
        Assert.True(sut.Nodes.Count <= EditorSettings.MaxGridNodes,
            $"Nodes ({sut.Nodes.Count}) exceed budget ({EditorSettings.MaxGridNodes})");
    }

    [Fact]
    public void RefreshGridNodes_GeneratesNodes_AtHighZoom()
    {
        // A4 portrait (210x297мм) at zoom 7.51, user step 5000: pixelSpacing = 37.55px >= 5.0,
        // 43*60 = 2580 <= 250000 → user step принят
        var sheet = Sheet.FromFormat("A4");
        var template = new Template(new Metadata(), sheet);
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 5000 };
        var zpm = new ZoomPanManager(template);
        zpm.SetZoom(7.51);
        var sut = new GridManager(template, gs, zpm, new GridNodeGenerator());

        sut.RefreshGridNodes();

        Assert.NotEmpty(sut.Nodes);
        Assert.True(sut.Nodes.Count <= EditorSettings.MaxGridNodes,
            $"Nodes ({sut.Nodes.Count}) exceed budget ({EditorSettings.MaxGridNodes})");
    }

    [Fact]
    public void RefreshGridNodes_GeneratedNodes_HaveNonNegativeCoordinates()
    {
        var sut = CreateSut();
        sut.RefreshGridNodes();

        Assert.NotEmpty(sut.Nodes);
        foreach (var node in sut.Nodes)
        {
            Assert.True(node.XMicrons >= 0, $"Node X is negative: {node.XMicrons}");
            Assert.True(node.YMicrons >= 0, $"Node Y is negative: {node.YMicrons}");
        }
    }

    [Fact]
    public void RefreshGridNodes_NewListInstanceEachCall()
    {
        var sut = CreateSut();
        sut.RefreshGridNodes();
        var firstList = sut.Nodes;

        sut.RefreshGridNodes();
        var secondList = sut.Nodes;

        // Каждый refresh — новый список от генератора, нет shared mutable state
        Assert.NotSame(firstList, secondList);
    }

    // ===== GridInvalidated callback =====

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

    // ===== Generator isolation (Mock) =====

    [Fact]
    public void RefreshGridNodes_PassesSheetAndSettingsToGenerator()
    {
        var genMock = new Mock<IGridNodeGenerator>(MockBehavior.Loose);
        genMock.Setup(g => g.ComputeDisplayStep(
                It.IsAny<double>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
            .Returns(5000);
        genMock.Setup(g => g.GenerateGridNodes(
                It.IsAny<long>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
            .Returns(new List<GridNode> { new(0, 0), new(5000, 5000) });

        var template = CreateTemplate(); // A4L 297000x210000
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 5000, MaxGridNodes = 12345 };
        var zpm = CreateZoomPanManager(template);
        var sut = new GridManager(template, gs, zpm, genMock.Object);

        sut.RefreshGridNodes();

        genMock.Verify(g => g.ComputeDisplayStep(zpm.Zoom, 12345, 297000, 210000, 5000), Times.Once);
        genMock.Verify(g => g.GenerateGridNodes(5000, zpm.Zoom, 297000, 210000, 12345), Times.Once);
        Assert.Equal(2, sut.Nodes.Count);
        Assert.Equal(5000, sut.Nodes[1].XMicrons);
    }

    [Fact]
    public void RefreshGridNodes_Disabled_DoesNotCallGenerator()
    {
        var genMock = new Mock<IGridNodeGenerator>(MockBehavior.Loose);
        var gs = new GridSettings { Enabled = false, Visible = true, StepMicrons = 5000 };
        var sut = CreateSut(gs, generator: genMock.Object);

        sut.RefreshGridNodes();

        genMock.Verify(g => g.ComputeDisplayStep(
            It.IsAny<double>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        genMock.Verify(g => g.GenerateGridNodes(
            It.IsAny<long>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()), Times.Never);
        Assert.Empty(sut.Nodes);
    }

    // ===== Template.Sheet INPC subscription =====

    [Fact]
    public void TemplateSheetChanged_RefreshesNodes()
    {
        var template = CreateTemplate(); // A4L: 297000x210000
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 5000 };
        var zpm = CreateZoomPanManager(template);
        var sut = new GridManager(template, gs, zpm, new GridNodeGenerator());
        sut.RefreshGridNodes();
        var invalidatedCount = 0;
        sut.GridInvalidated = () => invalidatedCount++;

        Assert.Equal(2580, sut.Nodes.Count); // 60x43

        // Act — Template теперь ObservableObject с INPC на Sheet
        template.Sheet = Sheet.FromFormat("A3", SheetOrientation.Landscape); // 420000x297000

        Assert.True(invalidatedCount > 0, "GridInvalidated must fire on sheet change");
        Assert.Equal(5100, sut.Nodes.Count); // 85x60
    }

    [Fact]
    public void Dispose_UnsubscribesFromTemplate()
    {
        var template = CreateTemplate();
        var gs = new GridSettings { Enabled = true, Visible = true, StepMicrons = 5000 };
        var zpm = CreateZoomPanManager(template);
        var sut = new GridManager(template, gs, zpm, new GridNodeGenerator());
        sut.RefreshGridNodes();
        var invalidatedCount = 0;
        sut.GridInvalidated = () => invalidatedCount++;
        var countBefore = sut.Nodes.Count;
        Assert.True(countBefore > 0);

        sut.Dispose();
        template.Sheet = Sheet.FromFormat("A3", SheetOrientation.Landscape);

        // После Dispose смена листа НЕ вызывает регенерацию
        Assert.Equal(0, invalidatedCount);
        Assert.Equal(countBefore, sut.Nodes.Count);
    }
}
