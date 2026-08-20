using DotElectric.Sheets;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Отображаемая метка ориентации листа («кн.»/«алб.»).
/// Единственная точка знания меток — все потребители делегируют сюда.
/// </summary>
public static class OrientationLabels
{
    /// <summary>
    /// Короткая метка ориентации для отображения в заголовках и статусной строке.
    /// </summary>
    public static string For(SheetOrientation orientation) =>
        orientation switch
        {
            SheetOrientation.Portrait => "кн.",
            SheetOrientation.Landscape => "алб.",
            _ => ""
        };
}
