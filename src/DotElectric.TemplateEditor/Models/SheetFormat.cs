namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Стандартный формат листа — запись каталога.
/// </summary>
/// <param name="Name">Каноническое имя формата (например, "A4", "A4×2").</param>
/// <param name="LongSideMicrons">Длинная сторона листа в микронах.</param>
/// <param name="ShortSideMicrons">Короткая сторона листа в микронах.</param>
/// <param name="DefaultOrientation">Ориентация по умолчанию.</param>
public sealed record SheetFormat(
    string Name,
    long LongSideMicrons,
    long ShortSideMicrons,
    SheetOrientation DefaultOrientation)
{
    /// <summary>
    /// Полуформат — удвоенная длинная сторона (A4×2…A0×2).
    /// </summary>
    public bool IsHalfFormat => Name.EndsWith("×2", StringComparison.Ordinal);
}
