using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
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
        var y1 = (sheetHeightMm - Coordinate.ToMm(line.StartMicronsY)) * WpfUnitsPerMm;
        var x2 = Coordinate.ToMm(line.EndMicronsX) * WpfUnitsPerMm;
        var y2 = (sheetHeightMm - Coordinate.ToMm(line.EndMicronsY)) * WpfUnitsPerMm;
        var thickness = Coordinate.ToMm(line.StrokeThicknessMicrons) * WpfUnitsPerMm;

        return new WpfLine
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = HexToBrush(line.StrokeColor),
            StrokeThickness = Math.Max(thickness, 0.5),
            StrokeDashArray = LineTypeToDashArray(line.LineType)
        };
    }

    private static UIElement CreateRectangleElement(Models.Objects.Rectangle rect, double sheetHeightMm)
    {
        var mmX = Coordinate.ToMm(rect.MicronsX);
        var mmY = Coordinate.ToMm(rect.MicronsY);
        var mmW = Coordinate.ToMm(rect.WidthMicrons);
        var mmH = Coordinate.ToMm(rect.HeightMicrons);
        var thickness = Coordinate.ToMm(rect.StrokeThicknessMicrons) * WpfUnitsPerMm;

        var wpfX = mmX * WpfUnitsPerMm;
        var wpfY = (sheetHeightMm - mmY - mmH) * WpfUnitsPerMm;
        var wpfW = mmW * WpfUnitsPerMm;
        var wpfH = mmH * WpfUnitsPerMm;

        var wpfRect = new WpfRectangle
        {
            Width = wpfW,
            Height = wpfH,
            Stroke = HexToBrush(rect.StrokeColor),
            StrokeThickness = Math.Max(thickness, 0.5),
            StrokeDashArray = LineTypeToDashArray(rect.LineType),
            Fill = HexToBrush(rect.FillColor)
        };

        FixedPage.SetLeft(wpfRect, wpfX);
        FixedPage.SetTop(wpfRect, wpfY);
        return wpfRect;
    }

    private static UIElement CreateTextElement(Models.Objects.Text text, double sheetHeightMm)
    {
        var mmX = Coordinate.ToMm(text.MicronsX);
        var mmY = Coordinate.ToMm(text.MicronsY);
        var fontSizeMm = Coordinate.ToMm(text.FontSizeMicrons);

        var wpfX = mmX * WpfUnitsPerMm;
        var wpfY = (sheetHeightMm - mmY) * WpfUnitsPerMm;
        var wpfFontSize = fontSizeMm * WpfUnitsPerMm;

        var textBlock = new TextBlock
        {
            Text = text.Content,
            FontFamily = FontNameToFamily(text.FontName),
            FontSize = Math.Max(wpfFontSize, 1.0),
            Foreground = HexToBrush(text.Foreground),
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

    private static Brush HexToBrush(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return new SolidColorBrush(Colors.Black);
        if (hex.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Colors.Transparent);
        try
        {
            return new BrushConverter().ConvertFromString(hex) as Brush
                   ?? new SolidColorBrush(Colors.Black);
        }
        catch
        {
            return new SolidColorBrush(Colors.Black);
        }
    }

    private static DoubleCollection? LineTypeToDashArray(LineType lineType)
        => lineType switch
        {
            LineType.Solid => null,
            LineType.Dashed => new DoubleCollection { 10, 5 },
            LineType.DashDot => new DoubleCollection { 10, 5, 2, 5 },
            LineType.DashDotDot => new DoubleCollection { 10, 5, 2, 5, 2, 5 },
            _ => null
        };

    private static FontFamily FontNameToFamily(string? fontName)
        => fontName switch
        {
            "ГОСТ А" => new FontFamily("pack://application:,,,/Resources/Fonts/#GOST Type AU"),
            "ГОСТ Б" => new FontFamily("pack://application:,,,/Resources/Fonts/#GOST Type BU"),
            _ => new FontFamily("Segoe UI")
        };
}
