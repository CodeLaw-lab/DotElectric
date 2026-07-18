## Context

Текущий `ZoomPanManager` хранит размер viewport в модельных миллиметрах (`ViewportWidthMm`/`ViewportHeightMm`), которые устанавливаются один раз в `OnSizeChanged` как `e.NewSize.Width / zoom`. При изменении zoom эти значения не пересчитываются — `ViewportWidthPixels = ViewportWidthMm * Zoom` продолжает расти пропорционально zoom. Поскольку `CanvasWidthPixels = sheetWidthMm * Zoom` растёт с той же скоростью, сравнение в `IsCentered` всегда даёт один и тот же результат (zoom сокращается). Скроллбары никогда не появляются.

Дополнительно:
- `OnSizeChanged` не вычитает 16px скроллбаров.
- `ScrollXValue = (C-V)/2 - PanOffsetX` — знак инвертирован (0 → правый край, max → левый).
- `CenterCanvas()` вызывается на каждый `SizeChanged`, сбрасывая pan.

## Goals / Non-Goals

**Goals:**
- Скроллбары появляются при zoom, достаточном чтобы лист превысил viewport.
- Drag скроллбара панорамирует лист в ожидаемом направлении.
- Viewport корректно отражает реальную canvas-область (минус 16px).
- Pan не сбрасывается при ресайзе окна.

**Non-Goals:**
- Изменение middle-click panning (работает, трогать не нужно).
- Изменение `PanTool` (работает через `PanCanvas`).
- Изменение XAML разметки скроллбаров (меняются только биндинги).
- Новые specs (нет изменения требований).

## Decisions

### D1: Store viewport in pixels, not mm

**Решение:** Заменить `_viewportWidthMm`/`_viewportHeightMm` (double, модель-пространство) на `_viewportWidthPx`/`_viewportHeightPx` (double, пиксели, константа).

**Текущий код:**
```csharp
[ObservableProperty] private double _viewportWidthMm;
public double ViewportWidthPixels => ViewportWidthMm * Zoom;
// OnSizeChanged: ViewportWidthMm = e.NewSize.Width / zoom;
```

**Новый код:**
```csharp
private double _viewportWidthPx;
public double ViewportWidthPixels => _viewportWidthPx;  // константа!
// OnSizeChanged: _viewportWidthPx = e.NewSize.Width;
```

**Почему:** `ViewportWidthPixels` должен отражать РЕАЛЬНЫЙ размер viewport в пикселях — константу, не зависящую от zoom. При изменении zoom меняется только `CanvasWidthPixels`, а `ViewportWidthPixels` остаётся прежним. Тогда `IsCentered = CanvasWidthPixels <= ViewportWidthPixels` корректно пересчитывается при каждом zoom-изменении.

**Альтернатива:** Обновлять `ViewportWidthMm` в `OnZoomChanged` как `_viewportWidthPx / Zoom`. Эквивалентно, но менее прозрачно. Хранение в px проще для понимания.

### D2: Invert ScrollXValue/ScrollYValue formula

**Текущий код:**
```csharp
// ScrollXValue = 0 → правый край (инвертировано)
public double ScrollXValue => IsCentered ? 0 : Math.Max(0, (C-V)/2 - PanOffsetX);
public void SetScrollX(double value) {
    PanOffsetX = (C-V)/2 - Math.Max(0, Math.Min(value, ScrollXRange));
}
```

**Новый код:**
```csharp
// ScrollXValue = 0 → левый край (корректно)
public double ScrollXValue => IsCentered ? 0 : Math.Max(0, PanOffsetX + (C-V)/2);
public void SetScrollX(double value) {
    PanOffsetX = Math.Max(0, Math.Min(value, ScrollXRange)) - (C-V)/2;
}
```

**Почему:** Текущая формула `(C-V)/2 - PanOffsetX` даёт ScrollXValue=0 при `PanOffsetX=(C-V)/2`, что соответствует крайнему ПРАВОМУ положению (canvasOffset = -(C-V)/2, canvas сдвинут влево). Пользователь ожидает, что ScrollXValue=0 показывает левый край. Новая формула `PanOffsetX + (C-V)/2` даёт:
- `PanOffsetX = -(C-V)/2` → `ScrollXValue = 0` → canvasOffset = +(C-V)/2 → canvas left = 0 **(левый край)**
- `PanOffsetX = +(C-V)/2` → `ScrollXValue = C-V = Range` → canvasOffset = -(C-V)/2 → canvas right = V **(правый край)**

### D3: Subtract scrollbar width/height in OnSizeChanged

```csharp
// Текущее: viewport включает 16px скроллбаров
vm.ZoomPanManager.SetViewportSize(e.NewSize.Width, e.NewSize.Height);

// Новое: вычитаем скроллбары
var vpWidth = e.NewSize.Width - SystemParameters.ScrollWidth;  // 16px
var vpHeight = e.NewSize.Height - SystemParameters.ScrollHeight; // 16px
vm.ZoomPanManager.SetViewportSize(vpWidth, vpHeight);
```

Добавить метод `SetViewportSize(double widthPx, double heightPx)` в ZoomPanManager.

### D4: Remove CenterCanvas from OnSizeChanged

Убрать `vm.CenterCanvas()` из `OnSizeChanged`. PanOffset не должен сбрасываться при ресайзе — пользователь может панорамировать и одновременно ресайзить окно.

`CenterCanvas` остаётся вызываться из `FitToScreen`.

### D5: New method SetViewportSize

Добавить в ZoomPanManager:
```csharp
public void SetViewportSize(double widthPx, double heightPx)
{
    _viewportWidthPx = widthPx;
    _viewportHeightPx = heightPx;
    OnPropertyChanged(nameof(ViewportWidthPixels));
    OnPropertyChanged(nameof(ViewportHeightPixels));
    OnPropertyChanged(nameof(IsCentered));
    OnPropertyChanged(nameof(ScrollXRange));
    OnPropertyChanged(nameof(ScrollYRange));
    OnPropertyChanged(nameof(ScrollXValue));
    OnPropertyChanged(nameof(ScrollYValue));
    OnPropertyChanged(nameof(CanvasOffsetX));
    OnPropertyChanged(nameof(CanvasOffsetY));
}
```

## Risks / Trade-offs

- **[Test breakage]** Тесты, проверяющие `ScrollXValue`/`ScrollYValue` после пана, потребуют обновления ожидаемых значений (инверсия знака). → Заранее подсчитать новые ожидаемые формулы.
- **[Middle-mouse pan consistency]** Middle-mouse pan через `PanCanvas` (+delta → +PanOffset) и скроллбар после инверсии будут двигать canvas в одном направлении. → Проверить, что оба механизма консистентны после фикса.
- **[Vertical scrollbar edge case]** Вертикальный скроллбар для квадратных/портретных форматов (A4 portrait: 210x297мм) — при zoom ~2.0 лист выше, чем шире, и только вертикальный скроллбар активен. → Проверить, что `SetViewportSize` и формулы `ScrollYValue`/`SetScrollY` корректны (симметричны горизонтальным).
