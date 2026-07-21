using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class GridHelperTests
{
    // ===== ComputeDisplayStep =====

    [Fact]
    public void ComputeDisplayStep_A0Zoom05_Returns3mm()
    {
        // A0: 841x1189mm, zoom 0.5
        // target = 0.5/0.5 = 1.0mm, nearest nice step = 1mm (1000 microns)
        // 842*1190 = 1,001,980 > 250K → coarsen to 3mm
        var step = GridHelper.ComputeDisplayStep(0.5, 250000, 841000, 1189000, 841000, 1189000);
        Assert.Equal(3000, step); // 3mm
    }

    [Fact]
    public void ComputeDisplayStep_A4Zoom1_Returns700()
    {
        // A4 portrait: 210x297mm, zoom 1.0
        // target = 0.5/1.0 = 0.5mm, nearest = 500 microns
        // 421*595 = 250,495 > 250K → coarsen to 700 microns (0.7mm)
        var step = GridHelper.ComputeDisplayStep(1.0, 250000, 210000, 297000, 210000, 297000);
        Assert.Equal(700, step); // 0.7mm
    }

    [Fact]
    public void ComputeDisplayStep_A0Zoom1_Returns3mm()
    {
        // A0: 841x1189mm, zoom 1.0
        // target = 0.5/1.0 = 0.5mm, nearest = 500 microns
        // 4,003,857 > 250K → coarsen through 0.7mm, 1mm, 1.5mm, 2mm all exceed budget
        // 3mm (3000): 281*397 = 111,557 ≤ 250K ✓
        var step = GridHelper.ComputeDisplayStep(1.0, 250000, 841000, 1189000, 841000, 1189000);
        Assert.Equal(3000, step); // 3mm
    }

    [Fact]
    public void ComputeDisplayStep_NullViewport_ReturnsCoarsest()
    {
        var step = GridHelper.ComputeDisplayStep(1.0, 250000, 210000, 297000, 0, 0);
        Assert.Equal(50000, step); // coarsest fallback
    }

    [Fact]
    public void ComputeDisplayStep_LargeViewport_ReturnsCoarseStep()
    {
        // A0 at zoom 0.1: target = 0.5/0.1 = 5mm, nearest = 5mm (5000 microns)
        // 841/5+1=170, 1189/5+1=239, 170*239=40,630 ≤ 250K ✓
        var step = GridHelper.ComputeDisplayStep(0.1, 250000, 841000, 1189000, 841000, 1189000);
        Assert.Equal(5000, step); // 5mm
    }

    // ===== GenerateGridNodes =====

    [Fact]
    public void GenerateGridNodes_A4Sheet_5mmStep_CreatesNodes()
    {
        var sheet = Sheet.FromFormat("A4"); // 210x297 мм (portrait)
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 5000, 1.0,
            0, 0, 210000, 297000);

        // A4 portrait: 210мм / 5мм = 42+1 по X, 297мм / 5мм = 59+1 по Y
        // Всего узлов: 43 * 60 = 2580
        Assert.Equal(43 * 60, nodes.Count);
    }

    [Fact]
    public void GenerateGridNodes_TooSmallZoom_ReturnsEmpty()
    {
        var sheet = Sheet.FromFormat("A4");
        // 5мм * 0.001 зум = 0.005мм < 0.5 (MinPixelSpacing) → узлы слишком плотные, не рисуем
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 5000, 0.001,
            0, 0, 210000, 297000);
        Assert.Empty(nodes);
    }

    [Fact]
    public void GenerateGridNodes_1mmStep_AtZoom1_GeneratesNodes()
    {
        var sheet = Sheet.FromFormat("A4");
        // 1мм * 1.0 зум = 1.0 >= 0.5 (MinPixelSpacing) → точки видны
        // cols*rows = 211*298 = 62,878 ≤ 250,000 → генерируем
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 1000, 1.0,
            0, 0, 210000, 297000);
        Assert.Equal(211 * 298, nodes.Count);
    }

    [Fact]
    public void GenerateGridNodes_1mmStep_AtZoom5_GeneratesNodes()
    {
        var sheet = Sheet.FromFormat("A4");
        // 1мм * 5.0 зум = 5px >= MinPixelSpacing → точки различимы, рисуем
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 1000, 5.0,
            0, 0, 210000, 297000);
        // A4 portrait: 211 по X, 298 по Y
        Assert.Equal(211 * 298, nodes.Count);
    }

    [Fact]
    public void GenerateGridNodes_ZeroStep_ReturnsEmpty()
    {
        var sheet = Sheet.FromFormat("A4");
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 0, 1.0,
            0, 0, 210000, 297000);
        Assert.Empty(nodes);
    }

    [Fact]
    public void GenerateGridNodes_ZeroZoom_ReturnsEmpty()
    {
        var sheet = Sheet.FromFormat("A4");
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 5000, 0,
            0, 0, 210000, 297000);
        Assert.Empty(nodes);
    }

    [Fact]
    public void GenerateGridNodes_ViewportSubset_ReturnsOnlyVisibleNodes()
    {
        var sheet = Sheet.FromFormat("A4"); // 210x297 мм (portrait)
        // Видимая область: 100-200мм по X, 50-150мм по Y
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 5000, 1.0,
            100000, 50000,   // viewport left, bottom
            100000, 100000   // viewport width, height
        );

        // X: 100-200мм → 100,105,...,200 = 21 узел
        // Y: 50-150мм → 50,55,...,150 = 21 узел
        // Всего: 21 * 21 = 441
        Assert.Equal(21 * 21, nodes.Count);
    }

    [Fact]
    public void GenerateGridNodes_ViewportExceedsSheet_ClampsToSheet()
    {
        var sheet = Sheet.FromFormat("A4"); // 210x297 мм (portrait)
        // Viewport больше листа
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 50000, 1.0,
            0, 0,               // viewport left, bottom
            500000, 500000      // viewport width, height (больше листа)
        );

        // Должно вернуть столько же узлов, сколько для всего листа
        // 210мм / 50мм = 0,50,100,150,200 = 5 по X
        // 297мм / 50мм = 0,50,100,150,200,250 = 6 по Y
        Assert.Equal(5 * 6, nodes.Count);
    }

    [Fact]
    public void GenerateGridNodes_ViewportPartial_CoversOnlyVisibleArea()
    {
        var sheet = Sheet.FromFormat("A4"); // 210x297 мм (portrait)
        // Viewport covers first quadrant: 0-100mm X, 0-100mm Y
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 10000, 1.0,
            0, 0,
            100000, 100000
        );

        // X: 0,10,...,100 = 11 nodes, Y: 0,10,...,100 = 11 nodes
        Assert.Equal(11 * 11, nodes.Count);

        // All nodes must be within viewport bounds
        foreach (var node in nodes)
        {
            Assert.InRange(node.XMicrons, 0, 100000);
            Assert.InRange(node.YMicrons, 0, 100000);
        }
    }

    [Fact]
    public void GenerateGridNodes_ViewportNegativeStart_ClampsToZero()
    {
        var sheet = Sheet.FromFormat("A4");
        // Viewport starts at negative coordinates (beyond sheet edge)
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 10000, 1.0,
            -50000, -50000,
            200000, 200000
        );

        // Nodes should start from (0,0) not (-50000,-50000)
        Assert.NotEmpty(nodes);
        foreach (var node in nodes)
        {
            Assert.True(node.XMicrons >= 0);
            Assert.True(node.YMicrons >= 0);
        }
    }

    [Fact]
    public void GenerateGridNodes_LargeStepExceedsMaxNodes_ReturnsEmpty()
    {
        var sheet = Sheet.FromFormat("A0"); // 841x1189 мм
        // 1mm step on full A0 at zoom 5x → many nodes
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 1000, 5.0,
            0, 0,
            841000, 1189000
        );

        // At 5x zoom, 1mm = 5px → passes MinPixelSpacing
        // But cols*rows = 842*1190 = 1,001,980 > 250,000 → should be empty
        Assert.Empty(nodes);
    }

    // ===== ComputeDisplayStep — edge cases =====

    [Fact]
    public void ComputeDisplayStep_ZeroZoom_ReturnsCoarsestFallback()
    {
        // zoom=0 triggers the early return → NiceStepsMicrons[0] = 50000 (50mm)
        var step = GridHelper.ComputeDisplayStep(0, 250000, 210000, 297000, 210000, 297000);
        Assert.Equal(50000, step);
    }

    [Fact]
    public void ComputeDisplayStep_MaxNodesOne_ReturnsCoarsestStep()
    {
        // With maxNodes=1, even 50000 step on A4 gives 5*6=30 > 1 → still returns coarsest as best effort
        var step = GridHelper.ComputeDisplayStep(1.0, 1, 210000, 297000, 210000, 297000);
        Assert.Equal(50000, step);
    }

    [Fact]
    public void ComputeDisplayStep_VerySmallSheet_ReturnsFineStep()
    {
        // 10×10mm sheet at zoom 10× with preferred step = 1000 (1mm)
        // preferredPixelSpacing = 1*10 = 10px ≥ 0.5 → target = 1000
        // cols = 11, rows = 11 → 121 ≤ 250000 → returns 1000
        var step = GridHelper.ComputeDisplayStep(10.0, 250000, 10000, 10000, 10000, 10000, 1000);
        Assert.Equal(1000, step);
    }

    [Fact]
    public void ComputeDisplayStep_PreferredStepAlreadyGood_ReturnsPreferredStep()
    {
        // preferredStep=10000 (10mm) at zoom 1.0: pixel spacing = 10px ≥ 0.5 → target = 10000
        // nearest nice step = 10000. A4: 22×31 = 682 ≤ 250000 → returns 10000
        var step = GridHelper.ComputeDisplayStep(1.0, 250000, 210000, 297000, 210000, 297000, 10000);
        Assert.Equal(10000, step);
    }

    // ===== GenerateGridNodes — edge cases =====

    [Fact]
    public void GenerateGridNodes_ZeroWidthSheet_ReturnsEmpty()
    {
        // Sheet with zero width: viewport clipping results in viewportRight(0) < viewportLeft(5000)
        // cols = 0 → no X loop iterations → empty
        var sheet = new Sheet { WidthMicrons = 0, HeightMicrons = 100000, Format = "Custom" };
        var nodes = GridHelper.GenerateGridNodes(
            sheet, 5000, 1.0,
            5000, 0,
            200000, 100000);
        Assert.Empty(nodes);
    }
}
