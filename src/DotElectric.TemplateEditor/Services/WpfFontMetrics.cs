using System.Windows.Media;
using Serilog;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// WPF-реализация метрик шрифта: измеряет коэффициенты по встроенным TTF
/// (GlyphTypeface). Запасные значения при недоступности шрифта и метрики
/// неизвестного имени (ведёт себя как шрифт по умолчанию, спека #162) —
/// из каталога шрифтов документной библиотеки.
/// </summary>
public sealed class WpfFontMetrics : IFontMetrics
{
    public static readonly WpfFontMetrics Default = new();

    private static readonly IReadOnlyList<int> SampleChars = Enumerable.Range('A', 26)
        .Concat(Enumerable.Range('a', 26))
        .Concat(Enumerable.Range('А', 32))
        .Concat(Enumerable.Range('а', 32))
        .ToList();

    private readonly Dictionary<string, double> _heightRatios = new();
    private readonly Dictionary<string, double> _widthRatios = new();
    private bool _initialized;
    private readonly object _lock = new();

    public bool IsInitialized => _initialized;

    public void Initialize()
    {
        lock (_lock)
        {
            if (_initialized) return;

            foreach (var font in FontCatalog.All)
            {
                LoadFont(font.Name, font.FamilyName,
                    fallbackHeight: font.FallbackHeightRatio,
                    fallbackWidth: font.FallbackWidthRatio);
            }

            _initialized = true;
        }
    }

    private void LoadFont(string fontName, string familyName,
        double fallbackHeight, double fallbackWidth)
    {
        try
        {
            var family = new FontFamily($"pack://application:,,,/Resources/Fonts/#{familyName}");

            foreach (var typeface in family.GetTypefaces())
            {
                if (typeface.TryGetGlyphTypeface(out var glyphTypeface))
                {
                    _heightRatios[fontName] = glyphTypeface.Height;
                    _widthRatios[fontName] = ComputeAverageAdvanceWidth(glyphTypeface.CharacterToGlyphMap, glyphTypeface.AdvanceWidths, SampleChars, fallbackWidth);
                    return;
                }
            }

            ApplyFallback(fontName, fallbackHeight, fallbackWidth);
        }
        catch (Exception ex)
        {
            HandleFallbackWithLog(fontName, ex.Message, fallbackHeight, fallbackWidth);
        }
    }

    private void HandleFallbackWithLog(string fontName, string message,
        double fallbackHeight, double fallbackWidth)
    {
        Log.Warning("Failed to load font {FontName}: {Message}", fontName, message);
        ApplyFallback(fontName, fallbackHeight, fallbackWidth);
    }

    private void ApplyFallback(string fontName, double fallbackHeight, double fallbackWidth)
    {
        _heightRatios[fontName] = fallbackHeight;
        _widthRatios[fontName] = fallbackWidth;
    }

    internal static double ComputeAverageAdvanceWidth(
        IDictionary<int, ushort> charToGlyphMap,
        IDictionary<ushort, double> advanceWidths,
        IEnumerable<int> sampleChars,
        double fallbackWidth)
    {
        double totalWidth = 0;
        int count = 0;
        foreach (var c in sampleChars)
        {
            var codePoint = (ushort)c;
            if (charToGlyphMap.TryGetValue(codePoint, out var glyphIndex))
            {
                if (advanceWidths.TryGetValue(glyphIndex, out var advWidth))
                {
                    totalWidth += advWidth;
                    count++;
                }
            }
        }
        return count > 0 ? totalWidth / count : fallbackWidth;
    }

    public void Reset()
    {
        lock (_lock)
        {
            _heightRatios.Clear();
            _widthRatios.Clear();
            _initialized = false;
        }
    }

    public double GetHeightRatio(string fontName)
    {
        if (_heightRatios.TryGetValue(fontName, out var ratio))
            return ratio;

        var resolved = FontCatalog.Resolve(fontName);
        return _heightRatios.TryGetValue(resolved, out var measured)
            ? measured
            : FontCatalog.Get(resolved).FallbackHeightRatio;
    }

    public double GetAdvWidthRatio(string fontName)
    {
        if (_widthRatios.TryGetValue(fontName, out var ratio))
            return ratio;

        var resolved = FontCatalog.Resolve(fontName);
        return _widthRatios.TryGetValue(resolved, out var measured)
            ? measured
            : FontCatalog.Get(resolved).FallbackWidthRatio;
    }
}
