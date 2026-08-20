namespace DotElectric.Document.Tests;

/// <summary>
/// Коллекция, отключающая параллельное выполнение тестов, которые мутируют
/// общий статический слот <see cref="FontMetricsProvider"/>.
/// </summary>
[CollectionDefinition("FontMetrics", DisableParallelization = true)]
public class FontMetricsTestCollection
{
}
