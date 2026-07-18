## Why

При шаге сетки 1мм или 0.5мм на больших форматах (A0, A1) сетка полностью пропадает из-за лимита MaxGridNodes (100K). При любом изменении зума или панорамировании происходит перестроение всех узлов с аллокациями, вызывая микро-фризы. На A0 с шагом 1mm — ~1,000,000 потенциальных узлов, что в 10× превышает лимит и даёт пустой лист без сетки.

## What Changes

- **Viewport Culling**: Генерация узлов сетки только в видимой области экрана (+ запас 1 экран), а не для всего листа
- **Adaptive Step**: Автоматический выбор шага отображения (1→2→5→10→20→50→100мм), кратного базовому, чтобы количество узлов не превышало лимит (50K)
- **Node Pooling**: Замена `List<GridNodeVm>` на пред-аллоцированный `long[]` (координаты X,Y чередуются) — ноль heap-аллокаций на refresh
- **Grid Debounce**: Отложенный InvalidateVisual при панорамировании — не чаще 1 раза в 16ms
- **Spec change**: Меняется поведение: узлы генерируются для viewport, а не для всего листа. При панорамировании сетка перестраивается (быстро, ~1-8ms)

## Capabilities

### New Capabilities
- `grid-performance`: Оптимизации рендеринга сетки — viewport culling, adaptive step, pooling, debounce

### Modified Capabilities
- `zoom-display-grid`: Requirement "Grid covers full sheet after zoom-in with scrollbars" меняется — узлы генерируются для viewport, а не для всего листа. При панорамировании происходит быстрый пересчёт (~1-8ms), а не пред-генерация для всего листа.

## Impact

- `ViewModels/Managers/GridManager.cs` — логика RefreshGridNodes (viewport, step selection, pooling)
- `Helpers/GridHelper.cs` — GenerateGridNodes (viewports params уже есть, но не используются)
- `Views/GridNodesLayer.cs` — OnRender с long[] вместо List<GridNodeVm>
- `ViewModels/GridNodeVm.cs` — возможно удаление (замена на long[])
- `ViewModels/Managers/ZoomPanManager.cs` — нужен доступ к viewport-координатам в микронах
