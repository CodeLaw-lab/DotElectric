## Why

Middle-click drag panning не работает, когда лист помещается во вьюпорт (обычный случай при zoom=100% для A4). Курсор меняется на SizeAll, дельта считается корректно, PanOffsetX/Y обновляются, но CanvasOffsetX/Y возвращают 0 из-за IsCentered guard — TranslateTransform не двигается, лист стоит на месте.

## What Changes

- Убрать IsCentered guard из `ZoomPanManager.CanvasOffsetX` и `CanvasOffsetY` — эти свойства должны всегда возвращать `-PanOffsetX`/`-PanOffsetY`, а не 0 когда лист помещается во вьюпорт
- Добавить тесты: end-to-end через Router с установленным viewport (isCentered=true), проверяющие что ZoomPanManager.CanvasOffsetX меняется после PanCanvas
- Никаких изменений в scroll-логике: ScrollXRange/ScrollYRange продолжают использовать IsCentered для скрытия ScrollBar

## Capabilities

### New Capabilities
- `canvas-pan`: Панорамирование холста средней кнопкой мыши (все zoom-уровни, включая IsCentered=true)

### Modified Capabilities
(нет изменений существующих spec-level требований)

## Impact

- **ZoomPanManager.cs** — 2 строки: убрать `IsCentered ? 0 :` из CanvasOffsetX/Y
- **PanToolTests.cs** — 1 новый тест с isCentered=true, проверяющий CanvasOffsetX
- **CanvasInputRouter tests** — 1 новый тест полного pipeline (Router → PanCanvas → CanvasOffset)
