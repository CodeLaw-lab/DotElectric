using System.Windows.Media;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using Xunit;

namespace DotElectric.TemplateEditor.Tests.Helpers;

/// <summary>
/// Тесты RenderRules — единого модуля правил рендеринга (спека #88, тикет #89).
/// Правила тестируются напрямую как чистые функции; поверхности (канвас/preview/печать)
/// фиксируют швы в своих тестовых файлах.
/// </summary>
public class RenderRulesTests
{
    // ===== Карта шрифтов =====
    // В MTA-окружении pack URI не резолвится, поэтому проверяем Source — строку URI
    // (паттерн FontNameToFamilyConverterTests).

    [Theory]
    [InlineData("ГОСТ А", "pack://application:,,,/Resources/Fonts/#GOST Type AU")]
    [InlineData("ГОСТ Б", "pack://application:,,,/Resources/Fonts/#GOST Type BU")]
    public void FontFamilyFor_KnownFont_ReturnsGostFamily(string fontName, string expectedSource)
    {
        var family = RenderRules.FontFamilyFor(fontName);
        Assert.Equal(expectedSource, family.Source);
    }

    [Theory]
    [InlineData("Arial")]
    [InlineData("")]
    [InlineData("   ")]
    public void FontFamilyFor_UnknownFont_ReturnsSegoeUi(string fontName)
    {
        var family = RenderRules.FontFamilyFor(fontName);
        Assert.Equal("Segoe UI", family.Source);
    }

    [Fact]
    public void FontFamilyFor_Null_ReturnsSegoeUi()
    {
        var family = RenderRules.FontFamilyFor(null);
        Assert.Equal("Segoe UI", family.Source);
    }

    // ===== Dash-карта (LineType → StrokeDashArray) =====
    // Значения — канон канваса (LineTypeToDashArrayConverter).

    [Fact]
    public void DashArrayFor_Solid_ReturnsNull()
    {
        Assert.Null(RenderRules.DashArrayFor(LineType.Solid));
    }

    [Fact]
    public void DashArrayFor_Dashed_ReturnsExactValues()
    {
        var dash = RenderRules.DashArrayFor(LineType.Dashed);
        Assert.NotNull(dash);
        Assert.Equal(new double[] { 10, 5 }, dash);
    }

    [Fact]
    public void DashArrayFor_DashDot_ReturnsExactValues()
    {
        var dash = RenderRules.DashArrayFor(LineType.DashDot);
        Assert.NotNull(dash);
        Assert.Equal(new double[] { 10, 5, 2, 5 }, dash);
    }

    [Fact]
    public void DashArrayFor_DashDotDot_ReturnsExactValues()
    {
        var dash = RenderRules.DashArrayFor(LineType.DashDotDot);
        Assert.NotNull(dash);
        Assert.Equal(new double[] { 10, 5, 2, 5, 2, 5 }, dash);
    }

    [Fact]
    public void DashArrayFor_UnknownValue_ReturnsNull()
    {
        Assert.Null(RenderRules.DashArrayFor((LineType)999));
    }

    [Fact]
    public void DashArrayFor_ReturnsFrozenSharedInstances()
    {
        // Frozen-коллекции читаемы из любого потока (канвас/preview/печать — разные потоки).
        var first = RenderRules.DashArrayFor(LineType.Dashed);
        var second = RenderRules.DashArrayFor(LineType.Dashed);
        Assert.NotNull(first);
        Assert.True(first.IsFrozen);
        Assert.Same(first, second);
    }

    // ===== Карта выравнивания текста =====

    [Theory]
    [InlineData("Center", System.Windows.TextAlignment.Center)]
    [InlineData("Right", System.Windows.TextAlignment.Right)]
    [InlineData("Left", System.Windows.TextAlignment.Left)]
    [InlineData("unknown", System.Windows.TextAlignment.Left)]
    public void TextAlignmentFor_KnownAndUnknownValues(string alignment, System.Windows.TextAlignment expected)
    {
        Assert.Equal(expected, RenderRules.TextAlignmentFor(alignment));
    }

    [Fact]
    public void TextAlignmentFor_Null_ReturnsLeft()
    {
        Assert.Equal(System.Windows.TextAlignment.Left, RenderRules.TextAlignmentFor(null));
    }

    // ===== hex → Brush =====
    // Семантика — канон канваса (HexToBrushConverter).

    [Theory]
    [InlineData("#FF0000", 0xFFFF0000)]
    [InlineData("#00FF00", 0xFF00FF00)]
    [InlineData("#0000FF", 0xFF0000FF)]
    public void BrushFromHex_ValidHexColor_ReturnsSolidColorBrush(string hex, uint expectedArgb)
    {
        var brush = Assert.IsType<SolidColorBrush>(RenderRules.BrushFromHex(hex));
        Assert.Equal(Color.FromArgb(
            (byte)(expectedArgb >> 24),
            (byte)(expectedArgb >> 16),
            (byte)(expectedArgb >> 8),
            (byte)expectedArgb), brush.Color);
    }

    [Theory]
    [InlineData("Transparent")]
    [InlineData("transparent")]
    [InlineData("TRANSPARENT")]
    public void BrushFromHex_TransparentAnyCase_ReturnsTransparentBrush(string hex)
    {
        var brush = Assert.IsType<SolidColorBrush>(RenderRules.BrushFromHex(hex));
        Assert.Equal(Colors.Transparent, brush.Color);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-color")]
    [InlineData("#GGHHII")]
    public void BrushFromHex_NullOrInvalid_ReturnsBlack(string? hex)
    {
        var brush = Assert.IsType<SolidColorBrush>(RenderRules.BrushFromHex(hex));
        Assert.Equal(Colors.Black, brush.Color);
    }

    [Fact]
    public void BrushFromHex_NamedColor_ParsesViaBrushConverter()
    {
        var brush = Assert.IsType<SolidColorBrush>(RenderRules.BrushFromHex("Red"));
        Assert.Equal(Colors.Red, brush.Color);
    }

    // ===== Y-flip: модель Y-up → поверхность Y-down =====
    // Ожидаемые значения — независимый ручной расчёт (мм, масштаб).

    [Theory]
    [InlineData(50_000L, 297.0, 1.0, 247.0)]   // (297 - 50) * 1
    [InlineData(0L, 297.0, 1.0, 297.0)]         // низ листа
    [InlineData(297_000L, 297.0, 1.0, 0.0)]     // верх листа
    [InlineData(0L, 297.0, 2.0, 594.0)]         // zoom 2
    [InlineData(-10_000L, 100.0, 1.0, 110.0)]   // ниже листа
    public void ModelYToTop_ZoomScale_FlipsModelY(long yMicrons, double sheetHeightMm, double scale, double expected)
    {
        Assert.Equal(expected, RenderRules.ModelYToTop(yMicrons, sheetHeightMm, scale), 4);
    }

    [Fact]
    public void ModelYToTop_PrintScale_UsesWpfUnitsPerMm()
    {
        // (297 - 25) * 96/25.4
        var expected = 272.0 * 96.0 / 25.4;
        Assert.Equal(expected, RenderRules.ModelYToTop(25_000, 297.0, 96.0 / 25.4), 4);
    }

    // ===== Anchor'ы: формулы на сырых значениях =====

    [Theory]
    [InlineData(3_000L, 1_000L, 3_000L)]
    [InlineData(1_000L, 3_000L, 3_000L)]
    [InlineData(2_000L, 2_000L, 2_000L)]
    public void LineTopMicrons_ReturnsMaxY(long startY, long endY, long expected)
    {
        Assert.Equal(expected, RenderRules.LineTopMicrons(startY, endY));
    }

    [Theory]
    [InlineData(3_000L, 1_000L, 1_000L)]
    [InlineData(1_000L, 3_000L, 1_000L)]
    public void LineLeftMicrons_ReturnsMinX(long startX, long endX, long expected)
    {
        Assert.Equal(expected, RenderRules.LineLeftMicrons(startX, endX));
    }

    [Fact]
    public void BoxTopMicrons_ReturnsYPlusHeight()
    {
        Assert.Equal(7_000L, RenderRules.BoxTopMicrons(5_000, 2_000));
    }

    // ===== Anchor-политика на тип объекта (единый dispatch) =====

    [Fact]
    public void AnchorTopMicrons_Line_ReturnsMaxYOfEndpoints()
    {
        var line = new Line(startMicronsX: 0, startMicronsY: 1_000, endMicronsX: 10_000, endMicronsY: 3_000);
        Assert.Equal(3_000L, RenderRules.AnchorTopMicrons(line));
    }

    [Fact]
    public void AnchorTopMicrons_Rectangle_ReturnsYPlusHeight()
    {
        var rect = new Rectangle(micronsX: 0, micronsY: 5_000, widthMicrons: 10_000, heightMicrons: 2_000);
        Assert.Equal(7_000L, RenderRules.AnchorTopMicrons(rect));
    }

    [Fact]
    public void AnchorTopMicrons_Text_ReturnsBottomMicronsY()
    {
        var text = new Text(micronsX: 1_000, micronsY: 2_000, content: "ABC", fontSizeMicrons: 5_000, fontName: "ГОСТ А");
        // Anchor — верх нетрансформированного бокса; heightRatio берёт на себя модель (HeightMicrons).
        Assert.Equal(text.BottomMicronsY, RenderRules.AnchorTopMicrons(text));
        Assert.True(RenderRules.AnchorTopMicrons(text) > text.MicronsY + text.FontSizeMicrons / 2);
    }

    [Fact]
    public void AnchorTopMicrons_MultilineText_AccountsLineCount()
    {
        var single = new Text(micronsX: 0, micronsY: 0, content: "A", fontSizeMicrons: 5_000, fontName: "ГОСТ А");
        var multi = new Text(micronsX: 0, micronsY: 0, content: "A\nB\nC", fontSizeMicrons: 5_000, fontName: "ГОСТ А");
        Assert.Equal(single.BottomMicronsY, RenderRules.AnchorTopMicrons(single));
        Assert.Equal(multi.BottomMicronsY, RenderRules.AnchorTopMicrons(multi));
        Assert.True(RenderRules.AnchorTopMicrons(multi) > RenderRules.AnchorTopMicrons(single));
    }

    [Theory]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(135)]
    [InlineData(270)]
    public void AnchorTopMicrons_RotatedText_SlotAnchorInvariant(int angle)
    {
        // Слот-anchor (позиция Canvas.Left/Top) не зависит от поворота:
        // смещение LayoutTransform применяет WPF при раскладке, не правила.
        var text = new Text(micronsX: 1_000, micronsY: 2_000, content: "ABC", fontSizeMicrons: 5_000, fontName: "ГОСТ А");
        var before = RenderRules.AnchorTopMicrons(text);
        text.RotationAngle = angle;
        Assert.Equal(before, RenderRules.AnchorTopMicrons(text));
    }

    [Fact]
    public void AnchorLeftMicrons_Line_ReturnsMinXOfEndpoints()
    {
        var line = new Line(startMicronsX: 5_000, startMicronsY: 0, endMicronsX: 2_000, endMicronsY: 0);
        Assert.Equal(2_000L, RenderRules.AnchorLeftMicrons(line));
    }

    [Fact]
    public void AnchorLeftMicrons_RectangleAndText_ReturnMicronsX()
    {
        var rect = new Rectangle(micronsX: 3_000, micronsY: 0, widthMicrons: 10_000, heightMicrons: 2_000);
        var text = new Text(micronsX: 4_000, micronsY: 0, content: "X", fontSizeMicrons: 5_000);
        Assert.Equal(3_000L, RenderRules.AnchorLeftMicrons(rect));
        Assert.Equal(4_000L, RenderRules.AnchorLeftMicrons(text));
    }

    [Fact]
    public void AnchorTopMicrons_UnknownType_Throws()
    {
        // Без silent-default: будущий тип объекта обязан получить явную anchor-политику.
        Assert.Throws<NotSupportedException>(() => RenderRules.AnchorTopMicrons(new UnknownObject()));
    }

    [Fact]
    public void AnchorLeftMicrons_UnknownType_Throws()
    {
        Assert.Throws<NotSupportedException>(() => RenderRules.AnchorLeftMicrons(new UnknownObject()));
    }

    private sealed class UnknownObject : TemplateObjectBase
    {
        public override long MicronsX { get; set; }
        public override long MicronsY { get; set; }
        public override TemplateObjectBase Clone() => new UnknownObject();
        public override bool ContainsPoint(PointMicrons point) => false;
        public override RectMicrons GetBoundingBox() => new(0, 0, 0, 0);
        public override ResizeState CaptureResizeState() => new(0, 0, 0, 0);
        public override void ApplyResize(ResizeState state) { }
    }
}
