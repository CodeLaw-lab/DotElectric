using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Tests.Helpers;

/// <summary>
/// Unit-тесты для <see cref="GridNodeGenerator"/> (замена GridHelper после Phase 1).
/// Генератор stateless — shared экземпляр безопасен.
/// Все координаты — абсолютные микроны листа (без viewport).
/// </summary>
public class GridNodeGeneratorTests
{
    private readonly GridNodeGenerator _sut = new();

    // A3 landscape: 420x297 мм
    private const long A3Width = 420000;
    private const long A3Height = 297000;

    // A4 portrait: 210x297 мм
    private const long A4Width = 210000;
    private const long A4Height = 297000;

    // A0 landscape: 1189x841 мм
    private const long A0Width = 1189000;
    private const long A0Height = 841000;

    // A0x2 portrait: 841x2378 мм
    private const long A0X2Width = 841000;
    private const long A0X2Height = 2378000;

    private const int MaxNodes = EditorSettings.MaxGridNodes; // 250000

    // ===== ComputeDisplayStep — user step =====

    [Fact]
    public void ComputeDisplayStep_UserStepFits_ReturnsUserStep()
    {
        // A3, zoom=1.0, step=5000 (5мм): pixelSpacing = 5*1.0 = 5px >= 5.0 (граница MinPixelSpacing),
        // cols*rows = 85*60 = 5100 <= 250000 → вернуть 5000
        var step = _sut.ComputeDisplayStep(1.0, MaxNodes, A3Width, A3Height, 5000);

        Assert.Equal(5000, step);
    }

    [Fact]
    public void ComputeDisplayStep_UserStepTooDense_CoarsensToNice()
    {
        // A3, zoom=1.0, step=500 (0.5мм): pixelSpacing = 0.5px < 5.0 MinPixelSpacing → user step отклонён.
        // Fallback target = 5.0/1.0 = 5мм → 5000 мкм → nearest nice = 5000.
        // 5000: cols*rows = 85*60 = 5100 <= 250000 → вернуть 5000.
        // С MinPixelSpacing=5.0 шаг < 5мм при zoom 1.0 coarsen'ится до 5мм (лимит плотности, не бюджета).
        var step = _sut.ComputeDisplayStep(1.0, MaxNodes, A3Width, A3Height, 500);

        Assert.Equal(5000, step);
    }

    [Fact]
    public void ComputeDisplayStep_UserStepBelowMinPixelSpacing_UsesFallback()
    {
        // A3, zoom=1.0, step=100 (0.1мм): pixelSpacing = 0.1px < 5.0 → игнорируем user step,
        // fallback на NiceSteps от target = 5.0/1.0 = 5мм (5000 мкм).
        // 5000: 85*60 = 5100 <= 250000 → вернуть 5000
        var step = _sut.ComputeDisplayStep(1.0, MaxNodes, A3Width, A3Height, 100);

        Assert.Equal(5000, step);
    }

    [Theory]
    [InlineData(0.0, 250000, 420000, 297000)]   // zoom = 0
    [InlineData(-1.0, 250000, 420000, 297000)]  // zoom < 0
    [InlineData(1.0, 0, 420000, 297000)]        // maxNodes = 0
    [InlineData(1.0, -5, 420000, 297000)]       // maxNodes < 0
    [InlineData(1.0, 250000, 0, 297000)]        // sheetW = 0
    [InlineData(1.0, 250000, 420000, 0)]        // sheetH = 0
    public void ComputeDisplayStep_InvalidInput_ReturnsDefault(
        double zoom, int maxNodes, long sheetW, long sheetH)
    {
        var step = _sut.ComputeDisplayStep(zoom, maxNodes, sheetW, sheetH, 5000);

        Assert.Equal(50000, step); // NiceSteps[0] — coarsest fallback
    }

    [Fact]
    public void ComputeDisplayStep_NoUserStep_AutoNice()
    {
        // preferredStep=0: target вычисляется от MinPixelSpacing (5.0/1.0 = 5мм → 5000 мкм).
        // nearest nice = 5000: 85*60 = 5100 <= 250000 → вернуть 5000
        var step = _sut.ComputeDisplayStep(1.0, MaxNodes, A3Width, A3Height, preferredStepMicrons: 0);

        Assert.Equal(5000, step);
    }

    [Fact]
    public void ComputeDisplayStep_HighZoom_SmallStep()
    {
        // A3, zoom=5.0, step=1000: pixelSpacing = 1*5 = 5px >= 5.0,
        // cols*rows = 421*298 = 125458 <= 250000 → вернуть 1000
        var step = _sut.ComputeDisplayStep(5.0, MaxNodes, A3Width, A3Height, 1000);

        Assert.Equal(1000, step);
    }

    [Fact]
    public void ComputeDisplayStep_HugeSheet_Coarsens()
    {
        // A0x2 (841x2378мм), zoom=1.0, step=1000: pixelSpacing = 1px < 5.0 → user step отклонён.
        // Fallback target = 5.0/1.0 = 5мм → 5000 мкм → nearest nice = 5000.
        // 5000: 169*476 = 80444 <= 250000 → вернуть 5000
        var step = _sut.ComputeDisplayStep(1.0, MaxNodes, A0X2Width, A0X2Height, 1000);

        Assert.Equal(5000, step);
    }

    // ===== ComputeDisplayStep — migrated from GridHelperTests =====

    [Fact]
    public void ComputeDisplayStep_A0Zoom05_Returns10mm()
    {
        // A0: 841x1189мм (W x H = 841000 x 1189000), zoom 0.5
        // target = 5.0/0.5 = 10мм → nearest = 10000
        // 10000: 119*85 = 10115 <= 250K
        var step = _sut.ComputeDisplayStep(0.5, MaxNodes, A0Height, A0Width);

        Assert.Equal(10000, step);
    }

    [Fact]
    public void ComputeDisplayStep_A4Zoom1_Returns5mm()
    {
        // A4 portrait: 210x297мм, zoom 1.0
        // target = 5.0мм → nearest = 5000: 43*60 = 2580 <= 250K
        var step = _sut.ComputeDisplayStep(1.0, MaxNodes, A4Width, A4Height);

        Assert.Equal(5000, step);
    }

    [Fact]
    public void ComputeDisplayStep_A0Zoom1_Returns5mm()
    {
        // A0: 841x1189мм, zoom 1.0
        // target = 5000 → nearest = 5000: 238*169 = 40222 <= 250K
        var step = _sut.ComputeDisplayStep(1.0, MaxNodes, A0Height, A0Width);

        Assert.Equal(5000, step);
    }

    [Fact]
    public void ComputeDisplayStep_LowZoom_ReturnsCoarseStep()
    {
        // A0 at zoom 0.1: target = 5.0/0.1 = 50мм → nearest = 50000
        // 24*17 = 408 <= 250K
        var step = _sut.ComputeDisplayStep(0.1, MaxNodes, A0Height, A0Width);

        Assert.Equal(50000, step);
    }

    [Fact]
    public void ComputeDisplayStep_MaxNodesOne_ReturnsCoarsestStep()
    {
        // maxNodes=1: даже 50000 на A4 даёт 5*6=30 > 1 → best effort = NiceSteps[0]
        var step = _sut.ComputeDisplayStep(1.0, 1, A4Width, A4Height);

        Assert.Equal(50000, step);
    }

    [Fact]
    public void ComputeDisplayStep_VerySmallSheet_ReturnsFineStep()
    {
        // 10x10мм sheet, zoom 10, preferred 1000: pixelSpacing = 10px >= 5.0,
        // 11*11 = 121 <= 250K → вернуть 1000
        var step = _sut.ComputeDisplayStep(10.0, MaxNodes, 10000, 10000, 1000);

        Assert.Equal(1000, step);
    }

    [Fact]
    public void ComputeDisplayStep_PreferredStepAlreadyGood_ReturnsPreferredStep()
    {
        // A4, zoom 1.0, preferred 10000 (10мм): pixelSpacing = 10px >= 5.0,
        // 22*30 = 660 <= 250K → вернуть 10000
        var step = _sut.ComputeDisplayStep(1.0, MaxNodes, A4Width, A4Height, 10000);

        Assert.Equal(10000, step);
    }

    // ===== GenerateGridNodes — happy path =====

    [Fact]
    public void GenerateGridNodes_A3_5mm_CorrectCount()
    {
        // A3: 420000x297000, step=5000, zoom=1.0
        // cols = 420000/5000+1 = 85, rows = 297000/5000+1 = 60 → 5100 узлов
        var nodes = _sut.GenerateGridNodes(5000, 1.0, A3Width, A3Height, MaxNodes);

        Assert.Equal(85 * 60, nodes.Count);
        Assert.Equal(0, nodes[0].XMicrons);
        Assert.Equal(0, nodes[0].YMicrons);
        // Дальний угол: max кратные шагу координаты (297000 не кратно 5000 → max Y = 295000)
        Assert.Contains(nodes, n => n.XMicrons == 420000 && n.YMicrons == 295000);
    }

    [Fact]
    public void GenerateGridNodes_StartsAtOrigin()
    {
        var nodes = _sut.GenerateGridNodes(5000, 1.0, A4Width, A4Height, MaxNodes);

        Assert.NotEmpty(nodes);
        foreach (var node in nodes)
        {
            Assert.True(node.XMicrons >= 0, $"X negative: {node.XMicrons}");
            Assert.True(node.YMicrons >= 0, $"Y negative: {node.YMicrons}");
            Assert.Equal(0, node.XMicrons % 5000);
            Assert.Equal(0, node.YMicrons % 5000);
        }
    }

    [Fact]
    public void GenerateGridNodes_NodesWithinSheet()
    {
        // step=3000 (3мм), zoom=2.0 → pixelSpacing = 6px >= 5.0 → узлы генерируются
        var nodes = _sut.GenerateGridNodes(3000, 2.0, A3Width, A3Height, MaxNodes);

        Assert.NotEmpty(nodes);
        foreach (var node in nodes)
        {
            Assert.InRange(node.XMicrons, 0, A3Width);
            Assert.InRange(node.YMicrons, 0, A3Height);
        }
    }

    // ===== GenerateGridNodes — guards (edge/error) =====

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void GenerateGridNodes_StepZeroOrNegative_ReturnsEmpty(long stepMicrons)
    {
        var nodes = _sut.GenerateGridNodes(stepMicrons, 1.0, A4Width, A4Height, MaxNodes);

        Assert.Empty(nodes);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void GenerateGridNodes_ZoomZeroOrNegative_ReturnsEmpty(double zoom)
    {
        var nodes = _sut.GenerateGridNodes(5000, zoom, A4Width, A4Height, MaxNodes);

        Assert.Empty(nodes);
    }

    [Fact]
    public void GenerateGridNodes_BelowMinPixelSpacing_ReturnsEmpty()
    {
        // step=100 (0.1мм), zoom=1.0 → pixelSpacing = 0.1px < 5.0 → слишком плотно, пусто
        var nodes = _sut.GenerateGridNodes(100, 1.0, A4Width, A4Height, MaxNodes);

        Assert.Empty(nodes);
    }

    [Fact]
    public void GenerateGridNodes_TooSmallZoom_ReturnsEmpty()
    {
        // 5мм * 0.001 зум = 0.005px < 5.0 (MinPixelSpacing) → узлы слишком плотные, пусто
        var nodes = _sut.GenerateGridNodes(5000, 0.001, A4Width, A4Height, MaxNodes);

        Assert.Empty(nodes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void GenerateGridNodes_MaxNodesZeroOrNegative_ReturnsEmpty(int maxNodes)
    {
        // Guard от бесконечного цикла coarsen: maxNodes <= 0 → пусто
        var nodes = _sut.GenerateGridNodes(5000, 1.0, A4Width, A4Height, maxNodes);

        Assert.Empty(nodes);
    }

    [Theory]
    [InlineData(-1, 297000)]
    [InlineData(210000, -1)]
    public void GenerateGridNodes_NegativeSheetDimensions_ReturnsEmpty(long sheetW, long sheetH)
    {
        var nodes = _sut.GenerateGridNodes(5000, 1.0, sheetW, sheetH, MaxNodes);

        Assert.Empty(nodes);
    }

    // ===== GenerateGridNodes — defense-in-depth budget coarsen =====

    [Fact]
    public void GenerateGridNodes_ExceedsBudget_CoarsensAndReturnsNodes()
    {
        // КЛЮЧЕВОЙ тест: сетка НИКОГДА не исчезает из-за бюджета.
        // step=1000, zoom=5.0 (pixelSpacing = 5px >= 5.0), A0x2 (841x2378мм): 842*2379 = 2003118 > 250000
        // → defense coarsen x2: 2000 → 421*1190 = 500990 > 250000
        // → x2: 4000 → 211*595 = 125545 <= 250000 → генерируем с шагом 4000
        var nodes = _sut.GenerateGridNodes(1000, 5.0, A0X2Width, A0X2Height, MaxNodes);

        Assert.NotEmpty(nodes);
        Assert.Equal(211 * 595, nodes.Count);
        foreach (var node in nodes)
        {
            Assert.Equal(0, node.XMicrons % 4000);
            Assert.Equal(0, node.YMicrons % 4000);
        }
    }

    // ===== GenerateGridNodes — migrated from GridHelperTests =====

    [Fact]
    public void GenerateGridNodes_A4Sheet_5mmStep_CreatesNodes()
    {
        // A4 portrait: 210x297мм, step 5000, zoom 1.0
        // cols = 43, rows = 60 → 2580
        var nodes = _sut.GenerateGridNodes(5000, 1.0, A4Width, A4Height, MaxNodes);

        Assert.Equal(43 * 60, nodes.Count);
    }

    [Fact]
    public void GenerateGridNodes_1mmStep_AtZoom1_ReturnsEmpty()
    {
        // 1мм × 1.0 зум = 1px < 5px MinPixelSpacing → скрыта (Sprint 47)
        var nodes = _sut.GenerateGridNodes(1000, 1.0, A4Width, A4Height, MaxNodes);

        Assert.Empty(nodes);
    }

    [Fact]
    public void GenerateGridNodes_1mmStep_AtZoom5_GeneratesNodes()
    {
        // 1мм × 5.0 зум = 5px >= 5.0 MinPixelSpacing → точки различимы
        var nodes = _sut.GenerateGridNodes(1000, 5.0, A4Width, A4Height, MaxNodes);

        Assert.Equal(211 * 298, nodes.Count);
    }
}
