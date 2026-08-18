using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using WpfLine = System.Windows.Shapes.Line;
using WpfRectangle = System.Windows.Shapes.Rectangle;

namespace DotElectric.TemplateEditor.Services;

public sealed class PrintDocumentGenerator : IPrintDocumentGenerator
{
    private const double WpfUnitsPerMm = 96.0 / 25.4;

    public FixedDocument Generate(Template template)
        => Generate(template, "DotElectric Template");

    public FixedDocument Generate(Template template, string title)
    {
        var document = new FixedDocument();
        var pageContent = new PageContent();
        var fixedPage = new FixedPage();

        var sheet = template.Sheet;
        var sheetWpfW = sheet.WidthMm * WpfUnitsPerMm;
        var sheetWpfH = sheet.HeightMm * WpfUnitsPerMm;

        fixedPage.Width = sheetWpfW;
        fixedPage.Height = sheetWpfH;

        var bg = new WpfRectangle
        {
            Width = sheetWpfW,
            Height = sheetWpfH,
            Fill = new SolidColorBrush(Colors.White),
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 1
        };
        fixedPage.Children.Add(bg);

        foreach (var obj in template.Objects)
        {
            var element = CreateElement(obj, sheet.HeightMm);
            if (element != null)
                fixedPage.Children.Add(element);
        }

        pageContent.Child = fixedPage;
        document.Pages.Add(pageContent);
        return document;
    }

    private static UIElement? CreateElement(TemplateObjectBase obj, double sheetHeightMm)
        => obj switch
        {
            Models.Objects.Line line => CreateLineElement(line, sheetHeightMm),
            Models.Objects.Rectangle rect => CreateRectangleElement(rect, sheetHeightMm),
            Models.Objects.Text text => CreateTextElement(text, sheetHeightMm),
            _ => null
        };

    private static UIElement CreateLineElement(Models.Objects.Line line, double sheetHeightMm)
    {
        var x1 = Coordinate.ToMm(line.StartMicronsX) * WpfUnitsPerMm;
        var y1 = RenderRules.ModelYToTop(line.StartMicronsY, sheetHeightMm, WpfUnitsPerMm);
        var x2 = Coordinate.ToMm(line.EndMicronsX) * WpfUnitsPerMm;
        var y2 = RenderRules.ModelYToTop(line.EndMicronsY, sheetHeightMm, WpfUnitsPerMm);
        var thickness = Coordinate.ToMm(line.StrokeThicknessMicrons) * WpfUnitsPerMm;

        return new WpfLine
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = RenderRules.BrushFromHex(line.StrokeColor),
            StrokeThickness = Math.Max(thickness, 0.5),
            StrokeDashArray = RenderRules.DashArrayFor(line.LineType)
        };
    }

    private static UIElement CreateRectangleElement(Models.Objects.Rectangle rect, double sheetHeightMm)
    {
        var mmX = Coordinate.ToMm(rect.MicronsX);
        var mmW = Coordinate.ToMm(rect.WidthMicrons);
        var mmH = Coordinate.ToMm(rect.HeightMicrons);
        var thickness = Coordinate.ToMm(rect.StrokeThicknessMicrons) * WpfUnitsPerMm;

        var wpfX = mmX * WpfUnitsPerMm;
        var wpfY = RenderRules.ModelYToTop(RenderRules.AnchorTopMicrons(rect), sheetHeightMm, WpfUnitsPerMm);
        var wpfW = mmW * WpfUnitsPerMm;
        var wpfH = mmH * WpfUnitsPerMm;

        var wpfRect = new WpfRectangle
        {
            Width = wpfW,
            Height = wpfH,
            Stroke = RenderRules.BrushFromHex(rect.StrokeColor),
            StrokeThickness = Math.Max(thickness, 0.5),
            StrokeDashArray = RenderRules.DashArrayFor(rect.LineType),
            Fill = RenderRules.BrushFromHex(rect.FillColor)
        };

        FixedPage.SetLeft(wpfRect, wpfX);
        FixedPage.SetTop(wpfRect, wpfY);
        return wpfRect;
    }

    private static UIElement CreateTextElement(Models.Objects.Text text, double sheetHeightMm)
    {
        var mmX = Coordinate.ToMm(text.MicronsX);
        var fontSizeMm = Coordinate.ToMm(text.FontSizeMicrons);

        var wpfX = mmX * WpfUnitsPerMm;
        // Anchor — канвас-семантика: верх нетрансформированного бокса (MicronsY + HeightMicrons).
        // Смещение повёрнутого элемента применяет WPF при раскладке LayoutTransform.
        var wpfY = RenderRules.ModelYToTop(RenderRules.AnchorTopMicrons(text), sheetHeightMm, WpfUnitsPerMm);
        var wpfFontSize = fontSizeMm * WpfUnitsPerMm;

        var textBlock = new TextBlock
        {
            Text = text.Content,
            FontFamily = RenderRules.FontFamilyFor(text.FontName),
            FontSize = Math.Max(wpfFontSize, 1.0),
            Foreground = RenderRules.BrushFromHex(text.Foreground),
            TextWrapping = text.TextWrapping ? System.Windows.TextWrapping.Wrap : System.Windows.TextWrapping.NoWrap,
            TextAlignment = TextAlignmentFromString(text.TextAlignment)
        };

        if (text.RotationAngle != 0)
        {
            textBlock.LayoutTransform = new RotateTransform(text.RotationAngle);
        }

        FixedPage.SetLeft(textBlock, wpfX);
        FixedPage.SetTop(textBlock, wpfY);
        return textBlock;
    }

    private static System.Windows.TextAlignment TextAlignmentFromString(string alignment)
        => alignment switch
        {
            "Center" => System.Windows.TextAlignment.Center,
            "Right" => System.Windows.TextAlignment.Right,
            _ => System.Windows.TextAlignment.Left
        };
}
