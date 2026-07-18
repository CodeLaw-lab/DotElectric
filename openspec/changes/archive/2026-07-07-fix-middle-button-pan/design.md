## Context

**Current state:** Middle-click drag panning реализован через CanvasInputRouter → ZoomPanManager.PanCanvas → PanOffsetX/Y. Однако `CanvasOffsetX` и `CanvasOffsetY` (которые привязаны к TranslateTransform канваса) имеют guard `IsCentered ? 0 : -PanOffset`. Когда лист помещается во вьюпорт (A4 при zoom=100% в окне 1200×800), `IsCentered` = true, и CanvasOffsetX/Y всегда возвращают 0. PanOffsetX/Y корректно обновляются в PanCanvas, но translate transform не двигается.

**Pipeline с багом:**
```
MouseMove → RouteMouseMove → deltaPx → PanCanvas(deltaX, deltaY)
  → PanOffsetX += deltaX * Zoom   ✓
  → OnPropertyChanged(CanvasOffsetX)
    → CanvasOffsetX = IsCentered ? 0 : -PanOffsetX  ✗ возвращает 0
      → TranslateTransform не двигается
```

**Корень:** Guard был добавлен с мыслью «если контент помещается, не надо его двигать, центрирование через Border». Но это ломает панорамирование — пользователь явно хочет двигать лист.

## Goals / Non-Goals

**Goals:**
- Middle-click drag panning работает на всех zoom-уровнях, включая IsCentered=true
- CanvasOffsetX/Y всегда отражают PanOffsetX/Y (с отрицанием)
- Тесты покрывают end-to-end pipeline с установленным viewport

**Non-Goals:**
- ScrollBar поведение не меняется (ScrollXRange/ScrollYRange продолжают использовать IsCentered)
- Auto-pan (click-release mode) не реализуется
- Изменения в routing или MouseCapture

## Decisions

### 1. Убрать IsCentered guard из CanvasOffsetX/Y

**Решение:** `CanvasOffsetX => -PanOffsetX` вместо `CanvasOffsetX => IsCentered ? 0 : -PanOffsetX`

**Rationale:**
- `PanOffsetX` — это абсолютное смещение канваса в пикселях. Когда лист центрирован Border'ом, PanOffset = 0 → CanvasOffset = 0 → центрирование работает.
- Когда пользователь панорамирует, PanOffsetX ≠ 0 → CanvasOffsetX = -PanOffsetX → TranslateTransform сдвигает канвас.
- При CenterCanvas() (вызывается при изменении zoom/viewport) PanOffset сбрасывается → центрирование восстанавливается.
- ScrollBars используют ScrollXRange/YRange, которые всё ещё проверяют IsCentered → скрываются когда контент помещается.

### 2. Новый тест: end-to-end через Router с isCentered=true

**Решение:** Добавить тест в EditorCanvasBehaviorTests, который:
- Создаёт EditorViewModel с A4 шаблоном
- Устанавливает ViewportWidthMm/HeightMm (больше размеров листа) → IsCentered = true
- Симулирует middle-click-drag через CanvasInputRouter.RouteMouseDown/Move
- Проверяет что CanvasOffsetX/Y изменились

**Rationale:** Существующие PanTool тесты проверяют PanOffsetX/Y, но не проходят через Router и не проверяют CanvasOffset. Тест в PanToolTests тоже не покрывает isCentered=true сценарий.

## Risks / Trade-offs

**R1: Canvas съезжает от центра при панорамировании в IsCentered=true**
- Это именно желаемое поведение. При повторном CenterCanvas() (zoom change) центрирование восстанавливается.
- Риск: если пользователь панорамирует, потом меняет zoom, CenterCanvas() сбросит позицию. Это существующее поведение, не меняется.

**R2: ScrollBar может показывать неправильную позицию**
- ScrollXValue/YValue продолжают использовать IsCentered → при IsCentered=true возвращают 0.
- После панорамирования при IsCentered=true ScrollXValue = 0, хотя canvas смещён.
- Mitigation: когда пользователь панорамирует при IsCentered=true, ScrollBars скрыты (ScrollXRange=0). Когда пользователь приближает (canvas > viewport), CenterCanvas() вызывается, IsCentered становится false, PanOffset пересчитывается — ScrollBar позиция корректна.
- Trade-off принят: ScrollBars скрыты, панорамирование работает. Минимальный UX компромисс.

**R3: Нет тестов на CanvasOffset через Router**
- Существующие тесты PanTool проверяют только PanOffsetX/Y напрямую.
- Mitigation: новый end-to-end тест покрывает Router → ZoomPanManager pipeline.
