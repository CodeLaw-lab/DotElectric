using System.Windows.Media;
using Serilog;

namespace DotElectric.TemplateEditor.Services;

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

            LoadFont("ГОСТ А", "GOST Type AU", fallbackHeight: 1.0, fallbackWidth: 0.5);
            LoadFont("ГОСТ Б", "GOST Type BU", fallbackHeight: 1.0, fallbackWidth: 0.65);

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
        return 1.0;
    }

    public double GetAdvWidthRatio(string fontName)
    {
        if (_widthRatios.TryGetValue(fontName, out var ratio))
            return ratio;
        return fontName switch
        {
            "ГОСТ А" => 0.5,
            "ГОСТ Б" => 0.65,
            _ => 0.6
        };
    }
}
