namespace DotElectric.Document;

/// <summary>
/// Контракт метрик шрифта — только чтение. Жизненный цикл (загрузка/сброс шрифтов) —
/// детали WPF-реализации в редакторе и не входят в контракт; документная библиотека
/// читает метрики через <see cref="FontMetricsProvider"/>.
/// </summary>
public interface IFontMetrics
{
    double GetHeightRatio(string fontName);

    double GetAdvWidthRatio(string fontName);
}
