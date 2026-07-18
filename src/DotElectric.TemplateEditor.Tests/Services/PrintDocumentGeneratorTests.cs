using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using ModelLine = DotElectric.TemplateEditor.Models.Objects.Line;
using ModelRect = DotElectric.TemplateEditor.Models.Objects.Rectangle;
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

    // ===== Helpers =====

    private static Template CreateEmptyTemplate(string format = "A4", SheetOrientation orientation = SheetOrientation.Portrait)
    {
        return new Template(new Metadata(), Sheet.FromFormat(format, orientation));
    }

    private static PageContent GetPageContent(FixedDocument doc)
        => (PageContent)doc.Pages[0];
}
