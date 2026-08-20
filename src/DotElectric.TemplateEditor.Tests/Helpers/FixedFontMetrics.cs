using DotElectric.Document;

namespace DotElectric.TemplateEditor.Tests.Helpers;

/// <summary>
/// Тестовая заглушка метрик шрифта для слота <see cref="FontMetricsProvider"/>:
/// возвращает фиксированные коэффициенты для любого шрифта (заменяет умерший
/// тестовый шов FontMetrics.InitializeWithTestValues).
/// </summary>
public sealed class FixedFontMetrics : IFontMetrics
{
    private readonly double _heightRatio;
    private readonly double _advWidthRatio;

    public FixedFontMetrics(double heightRatio, double advWidthRatio)
    {
        _heightRatio = heightRatio;
        _advWidthRatio = advWidthRatio;
    }

    public double GetHeightRatio(string fontName) => _heightRatio;

    public double GetAdvWidthRatio(string fontName) => _advWidthRatio;
}
