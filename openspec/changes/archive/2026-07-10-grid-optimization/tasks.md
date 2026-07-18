## 1. ZoomPanManager — Viewport API

- [x] 1.1 Добавить метод `GetViewportMicrons()` — возвращает (left, bottom, width, height) видимой области в микронах модели, с учётом PanOffsetXY, Zoom, viewport size и Y-flip
- [x] 1.2 Добавить тесты ZoomPanManager на GetViewportMicrons: centered mode, panned mode, разные zoom

## 2. GridHelper — Adaptive Step + Viewport Generation

- [x] 2.1 Добавить функцию `SelectDisplayStep(long baseStepMicrons, double zoom, long viewportWidthMicrons, long viewportHeightMicrons, int maxNodes)` — выбирает красивый шаг (×1,×2,×5,×10,×20,×50,×100,×200,×500) по двум критериям: pixelSpacing ≥ 5px И cols×rows ≤ maxNodes
- [x] 2.2 Модифицировать `GenerateGridNodes()` — убедиться, что корректно использует переданные viewport-параметры (работает, но не вызывается с ними) — функция уже корректна, вызов из GridManager исправлен в 3.2
- [x] 2.3 Добавить тесты для SelectDisplayStep: A0/×0.5/1mm → 10mm, A4/×1/1mm → 5mm, A0/×0.5/0.5mm → 10mm
- [x] 2.4 Добавить тесты GenerateGridNodes на viewport-ограничение

## 3. GridManager — Refresh с Viewport + Adaptive Step + Pooling

- [x] 3.1 Заменить `List<GridNodeVm>` на `long[] _nodeData` (maxNodes × 2) + `int _nodeCount`
- [x] 3.2 Интегрировать `ZoomPanManager.GetViewportMicrons()` в RefreshGridNodes вместо startX=0
- [x] 3.3 Интегрировать `SelectDisplayStep()` — вызывать перед GenerateGridNodes, заменять step на displayStep
- [x] 3.4 Удалить GridNodeVm.cs (замена на long[])
- [x] 3.5 Обновить GridNodesLayer — OnRender читает long[] из GridManager, рисует dc.DrawEllipse по парам X/Y
- [x] 3.6 Добавить debounce: CancellationTokenSource + Dispatcher.InvokeAsync с DispatcherPriority.Render, отмена предыдущего вызова
- [x] 3.7 Добавить тесты GridManager: viewport refresh, step adaptation, pool reuse, debounce

## 4. Spec migration

- [x] 4.1 Обновить `openspec/specs/zoom-display-grid/spec.md` — изменить Requirement "Grid covers full sheet" на viewport-поведение

## 5. Verify

- [ ] 5.1 РУЧНАЯ: A0, шаг 1мм, zoom ×0.5 — сетка видна
- [ ] 5.2 РУЧНАЯ: A4, шаг 1мм, zoom ×1 — 1мм сетка
- [ ] 5.3 РУЧНАЯ: панорамирование с debounce — нет фризов
- [x] 5.4 `dotnet test src/DotElectric.TemplateEditor.Tests` — 1865 passed, 1 skip (pre-existing)
- [x] 5.5 `dotnet build` — 0 errors, 0 warnings
