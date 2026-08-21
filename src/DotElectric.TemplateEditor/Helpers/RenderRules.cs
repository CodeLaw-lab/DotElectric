using System.Windows;
using System.Windows.Media;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Единые правила рендеринга «модель → примитивы» для всех конвейеров отображения:
/// канвас (DataTemplates + конвертеры), live-preview (PreviewLineChangedBehavior),
/// печать (PrintDocumentGenerator). Один источник истины для карт шрифт/dash/цвет,
/// Y-flip и anchor-политики на тип объекта (спека #88).
/// Возвращает готовые WPF-примитивы; guards (zoom, дефолты MultiBinding) остаются
/// в адаптерах-конвертерах.
/// </summary>
public static class RenderRules
{
    // Кэш FontFamily рядом с картой: биндинги канваса срабатывают на каждый
    // текстовый объект и каждое обновление. FontFamily не наследует Freezable —
    // экземпляры неизменяемы и читаемы из любого потока (печать) без заморозки.
    private static readonly IReadOnlyDictionary<string, FontFamily> FontFamilies =
        FontCatalog.All.ToDictionary(f => f.Name, f => CreateFontFamily(f.FamilyName));

    private static FontFamily CreateFontFamily(string familyName)
        => new($"pack://application:,,,/Resources/Fonts/#{familyName}");

    /// <summary>
    /// Карта шрифтов: доменное имя из каталога шрифтов → WPF FontFamily
    /// (pack-URI строится по внутреннему имени файла шрифта). Неизвестное имя
    /// или null — шрифт по умолчанию (спека #162): картинка и геометрия
    /// сходятся в одной точке решения. Экземпляры кэшированы.
    /// </summary>
    public static FontFamily FontFamilyFor(string? fontName)
        => FontFamilies[FontCatalog.Resolve(fontName)];

    private static readonly DoubleCollection _dashed = CreateFrozenDash(10, 5);
    private static readonly DoubleCollection _dashDot = CreateFrozenDash(10, 5, 2, 5);
    private static readonly DoubleCollection _dashDotDot = CreateFrozenDash(10, 5, 2, 5, 2, 5);

    // Frozen-коллекция читаема из любого потока: правила потребляют канвас (UI-поток),
    // preview и печать (произвольные потоки PrintDialog).
    private static DoubleCollection CreateFrozenDash(params double[] values)
    {
        var collection = new DoubleCollection(values);
        collection.Freeze();
        return collection;
    }

    /// <summary>
    /// Карта штрихов: тип линии → StrokeDashArray. Solid и неизвестные значения → null (сплошная).
    /// </summary>
    public static DoubleCollection? DashArrayFor(LineType lineType)
        => lineType switch
        {
            LineType.Solid => null,
            LineType.Dashed => _dashed,
            LineType.DashDot => _dashDot,
            LineType.DashDotDot => _dashDotDot,
            _ => null
        };

    /// <summary>
    /// Карта выравнивания текста: строковое значение модели → WPF TextAlignment.
    /// Неизвестное значение или null → Left.
    /// </summary>
    public static TextAlignment TextAlignmentFor(string? alignment)
        => alignment switch
        {
            "Center" => TextAlignment.Center,
            "Right" => TextAlignment.Right,
            _ => TextAlignment.Left
        };

    /// <summary>
    /// Парсинг hex-цвета в кисть. null/whitespace/invalid → чёрная,
    /// «Transparent» (без учёта регистра) → прозрачная; остальное — через BrushConverter.
    /// </summary>
    public static Brush BrushFromHex(string? hex)
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

    /// <summary>
    /// Y-flip: модельная координата Y (микроны, Y-up) → позиция верха в Y-down поверхности.
    /// Формула: (sheetHeightMm − yМм) × scale. Масштаб — параметр:
    /// канвас/preview/сетка передают zoom, печать — WPF-единиц на мм (96/25.4).
    /// </summary>
    public static double ModelYToTop(long yMicrons, double sheetHeightMm, double scale)
        => (sheetHeightMm - Coordinate.ToMm(yMicrons)) * scale;

    /// <summary>Верхний край линии: максимальный Y концов.</summary>
    public static long LineTopMicrons(long startY, long endY) => Math.Max(startY, endY);

    /// <summary>Левый край линии: минимальный X концов.</summary>
    public static long LineLeftMicrons(long startX, long endX) => Math.Min(startX, endX);

    /// <summary>Верхний край прямоугольного бокса (Rectangle, Text): Y + высота.</summary>
    public static long BoxTopMicrons(long yMicrons, long heightMicrons) => yMicrons + heightMicrons;

    /// <summary>
    /// Anchor-политика на тип: модельный Y верхней точки слота (позиция Canvas.Top / FixedPage.Top
    /// до Y-flip). Text: MicronsY + HeightMicrons — верх нетрансформированного бокса;
    /// смещение повёрнутого элемента применяет WPF при раскладке LayoutTransform, не правила.
    /// Неизвестный тип — явное исключение (без silent-default).
    /// </summary>
    public static long AnchorTopMicrons(TemplateObjectBase obj)
        => obj switch
        {
            Line line => LineTopMicrons(line.StartMicronsY, line.EndMicronsY),
            Rectangle rect => BoxTopMicrons(rect.MicronsY, rect.HeightMicrons),
            Text text => BoxTopMicrons(text.MicronsY, text.HeightMicrons),
            _ => throw new NotSupportedException($"Anchor-политика не определена для типа {obj.GetType().Name}.")
        };

    /// <summary>
    /// Anchor-политика на тип: модельный X левой точки слота (позиция Canvas.Left / FixedPage.Left).
    /// </summary>
    public static long AnchorLeftMicrons(TemplateObjectBase obj)
        => obj switch
        {
            Line line => LineLeftMicrons(line.StartMicronsX, line.EndMicronsX),
            Rectangle rect => rect.MicronsX,
            Text text => text.MicronsX,
            _ => throw new NotSupportedException($"Anchor-политика не определена для типа {obj.GetType().Name}.")
        };
}
