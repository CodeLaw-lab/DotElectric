using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Курсор мыши для инструмента.
/// </summary>
public enum ToolCursor
{
    /// <summary>Обычная стрелка.</summary>
    Arrow,
    /// <summary>Перекрестие (рисование).</summary>
    Cross,
    /// <summary>Рука (перемещение).</summary>
    Hand,
    /// <summary>Текстовый курсор (I-beam).</summary>
    IBeam,
    /// <summary>Ресайз: ↖↘ (северо-запад / юго-восток).</summary>
    SizeNWSE,
    /// <summary>Ресайз: ↗↙ (северо-восток / юго-запад).</summary>
    SizeNESW,
    /// <summary>Ресайз: ↔ (горизонтально).</summary>
    SizeWE,
    /// <summary>Ресайз: ↕ (вертикально).</summary>
    SizeNS,
}

/// <summary>
/// Клавиши, обрабатываемые инструментами (без WPF-зависимостей).
/// </summary>
public enum ToolKey
{
    /// <summary>Escape — отмена / сброс инструмента.</summary>
    Escape,
    /// <summary>Enter — подтверждение (inline-редактирование).</summary>
    Enter,
    /// <summary>Delete — удаление выделенного.</summary>
    Delete,
}

/// <summary>
/// Интерфейс инструмента (State Pattern).
/// Каждый инструмент обрабатывает ввод мыши на EditorCanvas.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Отображаемое имя инструмента.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Обработка нажатия кнопки мыши.
    /// </summary>
    void OnMouseDown(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers);

    /// <summary>
    /// Обработка перемещения мыши.
    /// </summary>
    void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers);

    /// <summary>
    /// Обработка отпускания кнопки мыши.
    /// </summary>
    void OnMouseUp(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers);

    /// <summary>
    /// Обработка двойного клика.
    /// </summary>
    void OnDoubleClick(PointMicrons modelPoint);

    /// <summary>
    /// Обработка колеса мыши.
    /// </summary>
    /// <returns>true если событие обработано (зум не применяется).</returns>
    bool OnMouseWheel(int delta, PointMicrons modelPoint);

    /// <summary>
    /// Обработка нажатия клавиши.
    /// </summary>
    /// <param name="key">Клавиша.</param>
    /// <param name="modifiers">Модификаторы (Ctrl, Shift, Alt).</param>
    /// <returns>true если клавиша обработана.</returns>
    bool OnKeyDown(ToolKey key, ToolModifiers modifiers);

    /// <summary>
    /// Получить курсор мыши для данного инструмента.
    /// </summary>
    ToolCursor GetCursor();

    /// <summary>
    /// Сбросить внутреннее состояние инструмента.
    /// Вызывается при переключении на другой инструмент.
    /// </summary>
    void Reset();
}
