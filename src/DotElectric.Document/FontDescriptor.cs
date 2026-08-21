namespace DotElectric.Document;

/// <summary>
/// Дескриптор шрифта — единственная точка знания на шрифт: доменное имя
/// (строковая идентичность, сериализуется в .tdel), внутреннее имя файла
/// шрифта и запасные коэффициенты метрик. Новый шрифт = один дескриптор
/// в <see cref="FontCatalog"/>.
/// </summary>
/// <param name="Name">Доменное имя шрифта в модели и файле («ГОСТ А», «ГОСТ Б»).</param>
/// <param name="FamilyName">Внутреннее имя файла шрифта (pack-URI рендера и загрузка метрик).</param>
/// <param name="FallbackHeightRatio">Запасной коэффициент высоты (шрифт недоступен или метрики не измерены).</param>
/// <param name="FallbackWidthRatio">Запасной коэффициент средней ширины глифа.</param>
public sealed record FontDescriptor(
    string Name,
    string FamilyName,
    double FallbackHeightRatio,
    double FallbackWidthRatio);
