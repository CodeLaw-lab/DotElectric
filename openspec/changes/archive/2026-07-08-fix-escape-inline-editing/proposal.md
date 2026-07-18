## Why

Escape не отменяет inline-редактирование текста. Когда пользователь дважды кликает текст, открывается TextBox для редактирования, но нажатие Escape очищает выделение вместо того чтобы закрыть редактор. Пользователь остаётся в режиме редактирования без возможности выйти по Escape — только Enter (фиксация) или клик мышью.

## What Changes

- `CanvasInputRouter.RoutePreviewKeyDown` получает focus guard: если фокус на дочернем элементе Canvas'а (TextBox), PreviewKeyDown не перехватывается — событие доходит до child control через bubbling KeyDown
- Поведение SelectTool.OnKeyDown(Escape) не меняется — очистка выделения по Escape продолжает работать, когда нет активного inline-редактирования
- Никаких изменений в IEditorContext, EditorViewModel, InlineEditManager, XAML

## Capabilities

### New Capabilities

- `inline-editing-escape`: Escape корректно отменяет inline-редактирование текста, когда TextBox в фокусе; очищает выделение в обычном режиме

### Modified Capabilities

*None*

## Impact

- **Файл:** `Behaviors/CanvasInputRouter.cs` — одна guard-строка в `RoutePreviewKeyDown`
- **Тесты:** опционально — тест в `Tests/Behaviors/EditorCanvasBehaviorTests.cs`
- **Регрессия:** отсутствует — поведение Escape без активного редактирования не меняется
