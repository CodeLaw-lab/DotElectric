## Context

При inline-редактировании текста (двойной клик → TextBox) нажатие Escape не закрывает редактор.

**Корневая причина:** `CanvasInputRouter.RoutePreviewKeyDown` висит на `DrawingCanvas.PreviewKeyDown` (туннелирование). DrawingCanvas — предок TextBox по визуальному дереву. `SelectTool.OnKeyDown(ToolKey.Escape)` возвращает `true`, `e.Handled` выставляется на фазе PreviewKeyDown, и парный `KeyDown` (bubbling) не стреляет. TextBox.InputBindings (Escape → CancelInlineEditingCommand) не срабатывают.

```
Escape PreviewKeyDown (▼ туннелирование)
  Window → ... → DrawingCanvas → CanvasInputRouter
    → SelectTool.OnKeyDown(Escape) → true
    → e.Handled = true
    → TextBox никогда не получает событие
    → KeyDown (▲ всплытие) не стреляет
    → CancelInlineEditingCommand не вызывается
```

## Goals / Non-Goals

**Goals:**
- Escape отменяет inline-редактирование когда TextBox в фокусе
- Escape очищает выделение когда TextBox не в фокусе (существующее поведение)
- Изменение только в одном файле (`CanvasInputRouter.cs`)
- Никаких изменений в `IEditorContext`, `EditorViewModel`, `SelectTool`, XAML

**Non-Goals:**
- Авто-фокус на TextBox при старте редактирования (отдельное улучшение)
- Рефакторинг клавиатурного роутинга
- Другие клавиши (Enter, Ctrl+Enter, Delete)

## Decisions

**Решение: focus guard в RoutePreviewKeyDown**

```csharp
public static void RoutePreviewKeyDown(Canvas canvas, KeyEventArgs e, EditorCanvasState state)
{
    // Если фокус на дочернем элементе Canvas'а — не перехватываем,
    // пусть child получит событие первым на фазе KeyDown (bubbling)
    if (FocusManager.GetFocusedElement(canvas) is UIElement focused && focused != canvas)
        return;

    var tool = GetCurrentTool(state.Editor);
    var toolKey = ToToolKey(e.Key);
    ...
}
```

**Почему guard, а не другие варианты:**

| Вариант | Минусы |
|---------|--------|
| Изменить SelectTool (добавить CancelInlineEditing) | Расширять IEditorContext, SelectTool узнаёт про редактирование, меняет контракт |
| Убрать Escape из ToToolKey | Логика ClearSelection переезжает в другое место, дублирование |
| Убрать PreviewKeyDown routing полностью | Меняет контракт для всех клавиш, избыточно |
| Обрабатывать в UserControl.InputBindings | ClearSelection остаётся в SelectTool, логика размазывается |

**Поведение по сценариям:**

| Фокус | Клавиша | Результат |
|-------|---------|-----------|
| TextBox (inline editing) | Escape | Guard → return → PreviewKeyDown доходит до TextBox → KeyDown → InputBindings → CancelInlineEditingCommand |
| Canvas (обычный режим) | Escape | Guard пропускает → SelectTool.OnKeyDown(Escape) → ClearSelection + Reset |
| Canvas (без выделения) | Escape | Guard пропускает → SelectTool.OnKeyDown(Escape) → Reset (no-op) |
| TextBox | Enter | Guard → return → существующий путь (Enter bubbles) |
| TextBox | Ctrl+Enter | Guard → return → TextBox.InputBindings |

## Risks / Trade-offs

- **TextBox без фокуса после старта редактирования** — Если пользователь дважды кликнул текст (TextBox появился), но НЕ кликнул в TextBox (фокус на Canvas), Escape пойдёт в SelectTool (очистит выделение, но TextBox останется). Это редкий сценарий — пользователь интуитивно кликает в TextBox перед набором.
- **Будущий child control** — Если другой дочерний элемент Canvas'а (не TextBox) получит фокус и ему нужен Escape, guard сработает автоматически. Масштабируется без изменений.
- **GetFocusedElement возвращает Window** — Редкий случай: если фокус нигде, `GetFocusedElement` может вернуть Window. Проверка `focused != canvas` отфильтрует этот случай — guard не сработает, что корректно.
