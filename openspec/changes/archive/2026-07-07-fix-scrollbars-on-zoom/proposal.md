## Why

При увеличении масштаба (zoom) вертикальный и горизонтальный скроллбары перестают появляться, хотя лист становится больше области просмотра. Пользователь не может панорамировать через скроллбары — только через middle-click drag. Это делает навигацию при высоком zoom неудобной и неинтуитивной.

## What Changes

- **ZoomPanManager**: переработать хранение viewport-размеров. Заменить `ViewportWidthMm`/`ViewportHeightMm` на фиксированные `_viewportWidthPx`/`_viewportHeightPx` (пиксели, не зависят от zoom). `ViewportWidthPixels`/`ViewportHeightPixels` возвращают константу.
- **OnSizeChanged**: вычитать ширину скроллбаров (16px) из `e.NewSize`, чтобы viewport соответствовал реальной canvas-области.
- **ScrollXValue/ScrollYValue**: исправить направление (инвертирован знак `PanOffset`).
- **SetScrollX/SetScrollY**: исправить формулу под новое направление.
- **CenterCanvas**: убрать безусловный вызов в `OnSizeChanged` — пан не должен сбрасываться при ресайзе окна.

## Capabilities

No new capabilities. No existing specs are modified — this is a pure implementation bugfix.

## Impact

- `ViewModels/Managers/ZoomPanManager.cs` — формулы `IsCentered`, `ScrollXValue`, `ScrollYValue`, `SetScrollX`, `SetScrollY`, `CenterCanvas`, поля `ViewportWidthMm`/`ViewportHeightMm`
- `Views/EditorCanvas.xaml.cs` — `OnSizeChanged` (вычитание 16px, убрать `CenterCanvas`)
- Тесты `ZoomPanManagerTests.cs`, `EditorViewModelTests.cs` — обновить ожидаемые значения после инверсии скролла
