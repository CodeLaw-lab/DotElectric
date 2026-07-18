## 1. ZoomPanManager — хранение viewport в пикселях

- [x] 1.1 Заменить `_viewportWidthMm`/`_viewportHeightMm` (ObservableProperty) на приватные поля `_viewportWidthPx`/`_viewportHeightPx` (double)
- [x] 1.2 Добавить метод `SetViewportSize(double widthPx, double heightPx)` с нотификациями IsCentered, ScrollX/YRange, ScrollX/YValue, CanvasOffsetX/Y
- [x] 1.3 Изменить `ViewportWidthPixels`/`ViewportHeightPixels` на `_viewportWidthPx`/`_viewportHeightPx` (константы, не умножать на Zoom)
- [x] 1.4 Удалить `[ObservableProperty]` атрибуты с `_viewportWidthMm`/`_viewportHeightMm` (заменены на px-поля)

## 2. ZoomPanManager — исправление направления скроллбара

- [x] 2.1 Переписать `ScrollXValue`: `(C-V)/2 - PanOffsetX` → `PanOffsetX + (C-V)/2`
- [x] 2.2 Переписать `ScrollYValue`: `(C-H)/2 - PanOffsetY` → `PanOffsetY + (C-H)/2`
- [x] 2.3 Переписать `SetScrollX`: `PanOffsetX = (C-V)/2 - clamp(...)` → `PanOffsetX = clamp(...) - (C-V)/2`
- [x] 2.4 Переписать `SetScrollY`: `PanOffsetY = (C-H)/2 - clamp(...)` → `PanOffsetY = clamp(...) - (C-H)/2`

## 3. EditorCanvas.xaml.cs — OnSizeChanged

- [x] 3.1 Вычесть ширину и высоту скроллбаров (16px) из `e.NewSize` перед установкой viewport
- [x] 3.2 Вызвать `vm.ZoomPanManager.SetViewportSize(vpWidth, vpHeight)` вместо прямого присваивания
- [x] 3.3 Убрать `vm.CenterCanvas()` из `OnSizeChanged`

## 4. EditorViewModel — чистка

- [x] 4.1 Удалить публичные методы `SetScrollX`/`SetScrollY` (делегирование к ZoomPanManager) — скролл через code-behind напрямую к ZoomPanManager
- [x] 4.2 Убедиться, что `PanCanvas` остаётся (используется middle-mouse panning)

## 5. Тесты

- [x] 5.1 Обновить `ZoomPanManagerTests`: скорректировать ожидаемые `ScrollXValue`/`ScrollYValue` под новую формулу
- [x] 5.2 Обновить `EditorViewModelTests`: убрать вызовы `editor.SetScrollX`/`SetScrollY` (если есть)
- [x] 5.3 Добавить тест: `IsCentered` становится `false` при zoom, когда canvas > viewport
- [x] 5.4 Добавить тест: `SetViewportSize` обновляет `ViewportWidthPixels`/`ViewportHeightPixels`
- [x] 5.5 Добавить тест: `ScrollXValue=0` соответствует левому краю (PanOffset=-(C-V)/2)

## 6. Verification

- [x] 6.1 Build: `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [x] 6.2 Tests: `dotnet test src/DotElectric.TemplateEditor.Tests` — all pass
