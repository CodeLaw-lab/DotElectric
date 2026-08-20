namespace DotElectric.Document;

/// <summary>
/// Константы документа — знание, которое читает сама модель документа.
/// Константы взаимодействия редактора (маркеры, рамка выделения, ресайз,
/// поля панели свойств, диалоги) живут в приложении.
/// </summary>
public static class DocumentConstants
{
    /// <summary>
    /// Допуск попадания в тело линии/прямоугольника (5 мм).
    /// </summary>
    public const long LineHitToleranceMicrons = 5000;
}
