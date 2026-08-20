using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using ModelLine = DotElectric.Document.Line;
using ModelRect = DotElectric.Document.Rectangle;
using WpfRectangle = System.Windows.Shapes.Rectangle;

namespace DotElectric.TemplateEditor.Tests.Services;

public class PrintDocumentGeneratorTests
{
    private readonly PrintDocumentGenerator _generator = new();

    // ===== 7.2: Generate — empty template returns FixedDocument with 1 page =====

    [Fact]
    public void Generate_EmptyTemplate_ReturnsFixedDocumentWithOnePage()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var doc = _generator.Generate(template);
            Assert.Single(doc.Pages);
        });
    }

    [Fact]
    public void Generate_EmptyTemplate_ContainsOnlyBackground()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            Assert.Single(page.Children);
            var bg = Assert.IsType<WpfRectangle>(page.Children[0]);
            Assert.Equal(new SolidColorBrush(Colors.White).Color, ((SolidColorBrush)bg.Fill).Color);
        });
    }

    // ===== 7.3: Page size matches sheet dimensions =====

    [Fact]
    public void Generate_PageSize_MatchesSheetDimensions()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate("A4", SheetOrientation.Portrait);
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var expectedW = template.Sheet.WidthMm * 96.0 / 25.4;
            var expectedH = template.Sheet.HeightMm * 96.0 / 25.4;
            Assert.Equal(expectedW, page.Width, 2);
            Assert.Equal(expectedH, page.Height, 2);
        });
    }

    // ===== 7.4: White background rectangle =====

    [Fact]
    public void Generate_FirstChild_IsWhiteBackgroundRectangle()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var bg = (WpfRectangle)page.Children[0];
            Assert.Equal(new SolidColorBrush(Colors.White).Color, ((SolidColorBrush)bg.Fill).Color);
            Assert.NotNull(bg.Stroke);
        });
    }

    // ===== 7.5: Line object creates WPF Line =====

    [Fact]
    public void Generate_LineObject_CreatesWpfLine()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var line = new ModelLine(0, 0, 100_000, 100_000, LineType.Solid);
            template.Objects.Add(line);
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfLine = page.Children.OfType<System.Windows.Shapes.Line>().FirstOrDefault();
            Assert.NotNull(wpfLine);
            Assert.Equal(0, wpfLine.X1, 2);
        });
    }

    // ===== 7.6: Rectangle object creates WPF Rectangle =====

    [Fact]
    public void Generate_RectangleObject_CreatesWpfRectangle()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var rect = new ModelRect(10_000, 10_000, 50_000, 30_000);
            template.Objects.Add(rect);
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfRect = page.Children.OfType<WpfRectangle>().Skip(1).FirstOrDefault();
            Assert.NotNull(wpfRect);
            Assert.True(wpfRect.Width > 0);
        });
    }

    // ===== 7.7: Text object creates WPF TextBlock =====

    [Fact]
    public void Generate_TextObject_CreatesTextBlock()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var text = new Text(10_000, 10_000, "Hello", 5_000);
            template.Objects.Add(text);
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var tb = page.Children.OfType<System.Windows.Controls.TextBlock>().FirstOrDefault();
            Assert.NotNull(tb);
            Assert.Equal("Hello", tb.Text);
        });
    }

    // ===== 7.8: LineType mappings =====

    [Fact]
    public void Generate_LineDashed_HasDashArray()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            template.Objects.Add(new ModelLine(0, 0, 100_000, 0, LineType.Dashed));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfLine = page.Children.OfType<System.Windows.Shapes.Line>().First();
            Assert.NotNull(wpfLine.StrokeDashArray);
        });
    }

    [Fact]
    public void Generate_LineSolid_HasNoDashArray()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            template.Objects.Add(new ModelLine(0, 0, 100_000, 0, LineType.Solid));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfLine = page.Children.OfType<System.Windows.Shapes.Line>().First();
            Assert.Null(wpfLine.StrokeDashArray);
        });
    }

    [Fact]
    public void Generate_LineDashDot_HasDashArray()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            template.Objects.Add(new ModelLine(0, 0, 100_000, 0, LineType.DashDot));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfLine = page.Children.OfType<System.Windows.Shapes.Line>().First();
            Assert.NotNull(wpfLine.StrokeDashArray);
            Assert.Equal(4, wpfLine.StrokeDashArray.Count);
        });
    }

    [Fact]
    public void Generate_LineDashDotDot_HasDashArray()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            template.Objects.Add(new ModelLine(0, 0, 100_000, 0, LineType.DashDotDot));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfLine = page.Children.OfType<System.Windows.Shapes.Line>().First();
            Assert.NotNull(wpfLine.StrokeDashArray);
            Assert.Equal(6, wpfLine.StrokeDashArray.Count);
        });
    }

    // ===== 7.9: Colors =====

    [Fact]
    public void Generate_LineStrokeColor_AppliesBrush()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            template.Objects.Add(new ModelLine(0, 0, 100_000, 0, strokeColor: "#FF0000"));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfLine = page.Children.OfType<System.Windows.Shapes.Line>().First();
            var brush = (SolidColorBrush)wpfLine.Stroke;
            Assert.Equal(Colors.Red, brush.Color);
        });
    }

    [Fact]
    public void Generate_RectangleFillColor_AppliesBrush()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            template.Objects.Add(new ModelRect(0, 0, 50_000, 50_000, fillColor: "#00FF00"));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfRect = page.Children.OfType<WpfRectangle>().Skip(1).First();
            var brush = (SolidColorBrush)wpfRect.Fill;
            Assert.Equal(Colors.Lime, brush.Color);
        });
    }

    [Fact]
    public void Generate_TextForeground_AppliesBrush()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var text = new Text(0, 0, "Test", 5_000, foreground: "#0000FF");
            template.Objects.Add(text);
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var tb = page.Children.OfType<System.Windows.Controls.TextBlock>().First();
            var brush = (SolidColorBrush)tb.Foreground;
            Assert.Equal(Colors.Blue, brush.Color);
        });
    }

    [Fact]
    public void Generate_LineCoordinates_CorrectWpfTransform()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate("A4", SheetOrientation.Portrait);
            var sheetH = template.Sheet.HeightMm;
            template.Objects.Add(new ModelLine(0, 0, 100_000, 100_000));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfLine = page.Children.OfType<System.Windows.Shapes.Line>().First();
            var factor = 96.0 / 25.4;
            Assert.Equal(0, wpfLine.X1, 2);
            Assert.Equal(sheetH * factor, wpfLine.Y1, 2);
        });
    }

    // ===== 7.10: Text rotation =====

    [Fact]
    public void Generate_TextWithRotation_HasLayoutTransform()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var text = new Text(0, 0, "Rotated", 5_000, rotationAngle: 45);
            template.Objects.Add(text);
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var tb = page.Children.OfType<System.Windows.Controls.TextBlock>().First();
            Assert.NotNull(tb.LayoutTransform);
        });
    }

    [Fact]
    public void Generate_TextNoRotation_NoRotateTransform()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var text = new Text(0, 0, "Normal", 5_000, rotationAngle: 0);
            template.Objects.Add(text);
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var tb = page.Children.OfType<System.Windows.Controls.TextBlock>().First();
            Assert.IsNotType<RotateTransform>(tb.LayoutTransform);
        });
    }

    // ===== 7.11: Multiple objects =====

    [Fact]
    public void Generate_MultipleObjects_AllRendered()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            template.Objects.Add(new ModelLine(0, 0, 50_000, 50_000));
            template.Objects.Add(new ModelRect(0, 0, 30_000, 20_000));
            template.Objects.Add(new Text(0, 0, "Multi", 3_000));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            Assert.Equal(4, page.Children.Count); // bg + 3 objects
        });
    }

    // ===== 7.12: Null/empty template objects (handles gracefully) =====

    [Fact]
    public void Generate_NullObjectsCollection_ReturnsPageWithBackgroundOnly()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            Assert.Single(page.Children);
            Assert.IsType<WpfRectangle>(page.Children[0]);
        });
    }

    // ===== 7.13: Title parameter =====

    [Fact]
    public void Generate_WithTitle_ReturnsDocument()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            var doc = _generator.Generate(template, "Test Title");
            Assert.Single(doc.Pages);
        });
    }

    // ===== Anchor'ы печати (фиксация единых правил рендеринга, спека #88) =====

    [Fact]
    public void Generate_Text_TopPosition_AccountsTextHeight()
    {
        // Regression: печать обязана ставить верх текста там же, где канвас —
        // по MicronsY + HeightMicrons (а не по MicronsY без высоты).
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate("A4", SheetOrientation.Portrait);
            var sheetH = template.Sheet.HeightMm;
            var text = new Text(10_000, 20_000, "Anchor", 5_000, fontName: "ГОСТ А");
            template.Objects.Add(text);

            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var tb = page.Children.OfType<System.Windows.Controls.TextBlock>().First();

            var factor = 96.0 / 25.4;
            var expectedTop = (sheetH - Coordinate.ToMm(text.BottomMicronsY)) * factor;
            Assert.Equal(expectedTop, FixedPage.GetTop(tb), 4);
        });
    }

    [Fact]
    public void Generate_MultilineText_TopPosition_AccountsFullHeight()
    {
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate("A4", SheetOrientation.Portrait);
            var sheetH = template.Sheet.HeightMm;
            var text = new Text(0, 50_000, "A\nB\nC", 5_000, fontName: "ГОСТ Б");
            template.Objects.Add(text);

            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var tb = page.Children.OfType<System.Windows.Controls.TextBlock>().First();

            var factor = 96.0 / 25.4;
            var expectedTop = (sheetH - Coordinate.ToMm(text.BottomMicronsY)) * factor;
            Assert.Equal(expectedTop, FixedPage.GetTop(tb), 4);
        });
    }

    [Fact]
    public void Generate_RotatedText_SlotAnchor_SameAsUnrotated()
    {
        // Поворот не меняет слот-anchor (смещение LayoutTransform применяет WPF
        // при раскладке — та же семантика, что канвас).
        WpfContext.Execute(() =>
        {
            var templatePlain = CreateEmptyTemplate("A4", SheetOrientation.Portrait);
            var templateRotated = CreateEmptyTemplate("A4", SheetOrientation.Portrait);
            var plain = new Text(10_000, 20_000, "Rot", 5_000, rotationAngle: 0);
            var rotated = new Text(10_000, 20_000, "Rot", 5_000, rotationAngle: 45);
            templatePlain.Objects.Add(plain);
            templateRotated.Objects.Add(rotated);

            var pagePlain = (FixedPage)GetPageContent(_generator.Generate(templatePlain)).Child;
            var pageRotated = (FixedPage)GetPageContent(_generator.Generate(templateRotated)).Child;
            var tbPlain = pagePlain.Children.OfType<System.Windows.Controls.TextBlock>().First();
            var tbRotated = pageRotated.Children.OfType<System.Windows.Controls.TextBlock>().First();

            Assert.Equal(FixedPage.GetTop(tbPlain), FixedPage.GetTop(tbRotated), 4);
            Assert.Equal(FixedPage.GetLeft(tbPlain), FixedPage.GetLeft(tbRotated), 4);
            Assert.IsType<RotateTransform>(tbRotated.LayoutTransform);
        });
    }

    [Fact]
    public void Generate_Rectangle_TopPosition_MatchesModelBottom()
    {
        // Паритет с канвасом: верх прямоугольника — по MicronsY + HeightMicrons.
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate("A4", SheetOrientation.Portrait);
            var sheetH = template.Sheet.HeightMm;
            var rect = new ModelRect(10_000, 20_000, 50_000, 30_000);
            template.Objects.Add(rect);

            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfRect = page.Children.OfType<WpfRectangle>().Skip(1).First();

            var factor = 96.0 / 25.4;
            var expectedTop = (sheetH - Coordinate.ToMm(rect.MicronsY + rect.HeightMicrons)) * factor;
            Assert.Equal(expectedTop, FixedPage.GetTop(wpfRect), 4);
            Assert.Equal(Coordinate.ToMm(rect.MicronsX) * factor, FixedPage.GetLeft(wpfRect), 4);
        });
    }

    [Theory]
    [InlineData(LineType.Dashed, new double[] { 10, 5 })]
    [InlineData(LineType.DashDot, new double[] { 10, 5, 2, 5 })]
    [InlineData(LineType.DashDotDot, new double[] { 10, 5, 2, 5, 2, 5 })]
    public void Generate_LineDashValues_MatchRenderRules(LineType lineType, double[] expected)
    {
        // Печать берёт dash-значения из единых правил (ранее тесты фиксировали только Count).
        WpfContext.Execute(() =>
        {
            var template = CreateEmptyTemplate();
            template.Objects.Add(new ModelLine(0, 0, 100_000, 0, lineType));
            var doc = _generator.Generate(template);
            var page = (FixedPage)GetPageContent(doc).Child;
            var wpfLine = page.Children.OfType<System.Windows.Shapes.Line>().First();
            Assert.NotNull(wpfLine.StrokeDashArray);
            Assert.Equal(expected, wpfLine.StrokeDashArray);
        });
    }

    // ===== Helpers =====

    private static Template CreateEmptyTemplate(string format = "A4", SheetOrientation orientation = SheetOrientation.Portrait)
    {
        return new Template(new Metadata(), Sheet.FromFormat(format, orientation));
    }

    private static PageContent GetPageContent(FixedDocument doc)
        => (PageContent)doc.Pages[0];
}
