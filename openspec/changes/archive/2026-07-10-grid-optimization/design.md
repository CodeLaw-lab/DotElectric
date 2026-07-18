## Context

Текущая реализация сетки (`GridManager.RefreshGridNodes`) генерирует узлы для всего листа при каждом изменении зума или панорамирования. При шаге 1мм на A0 (~1,000,000 узлов) срабатывает `MaxGridNodes` (100K) и сетка не отображается. Даже при меньших форматах каждый refresh аллоцирует `List<GridNodeVm>`, вызывая GC-паузы.

Три связанные проблемы: (1) full-sheet генерация вместо viewport, (2) жёсткий лимит 100K без fallback, (3) heap-аллокации на каждый refresh.

Существующий spec `zoom-display-grid` требует full-sheet coverage — это поведение меняется.

## Goals / Non-Goals

**Goals:**
- Сетка видна на любом формате и зуме, если шаг в пикселях ≥ MinPixelSpacing
- Нет фризов при зуме, панорамировании, ресайзе окна
- Refresh сетки за < 10ms на любой конфигурации
- Ноль heap-аллокаций при refresh (no-GC)

**Non-Goals:**
- Изменение визуального стиля сетки (остаются точки-пересечения, не линии)
- Изменение логики Snap (SnapHelper не меняется)
- Оптимизация рендеринга объектов (Line/Rectangle/Text) — только сетка

## Decisions

### Decision 1: Viewport Culling вместо full-sheet генерации
Текущий spec требует генерации для всего листа. Меняем: узлы генерируются для viewport + margin в 1 экран. При панорамировании/зуме происходит быстрый пересчёт.

- **Why**: Full-sheet генерация непрактична при 1мм на A0 — 1M узлов. При viewport-culling даже на A0 ×0.5 viewport = 841×1189mm = всё ещё 1M узлов, но adaptive step решает это.
- **Alternatives considered**: Full-sheet generation with LOD (coarse step for whole sheet + fine step for viewport) — сложнее, выигрыш сомнителен.

### Decision 2: Adaptive Step с «красивыми» множителями
Последовательность красивых шагов: baseStep × [1, 2, 5, 10, 20, 50, 100, 200, 500]. Для 1мм: 1, 2, 5, 10, 20, 50, 100, 200, 500мм. Для 0.5мм: 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500мм.

- **Why**: Кратные 1/2/5 дают интуитивно понятный шаг. В отличие от простого удвоения (×2, ×4, ×8, ×16...), пользователь видит «10мм» а не «16мм».
- **Selection logic**: Перебираем от самого мелкого к крупному. Первый подходящий по двум критериям: pixelSpacing ≥ MinPixelSpacing (5px) И количество узлов ≤ maxNodes (50K).
- **CalculateOptimalStep существует, но не используется** — заменяем на новый алгоритм.

### Decision 3: Pooling — long[] вместо List<GridNodeVm>
Храним узлы как чередующиеся X,Y-координаты в `long[]`. Размер массива = maxNodes × 2. Счётчик active nodes = `_nodeCount`.

- **Why**: List<GridNodeVm> аллоцирует 2 объекта на узел (сам список + GridNodeVm). При 50K узлов — 100K аллокаций. long[] аллоцируется один раз и переиспользуется. Экономия: ~48 bytes/узел → 8 bytes/узел (×6).
- **GridNodeVm.cs** — удаляется. `GridNodesLayer` читает long[] напрямую.
- **GridNode struct** (в GridHelper) — остаётся для генерации, но результат пишется напрямую в массив, а не через List<GridNode>.

### Decision 4: Debounce через DispatcherPriority.Render
RefreshGridNodes вычисляет узлы синхронно (быстро, < 1ms), но `InvalidateVisual` откладывается на 16ms.

- **Why**: При панорамировании 60 MouseMove/сек → 60 Refresh/сек. Без debounce — 60 перерисовок сетки/сек. С debounce — только 60 пересчётов (дешёвых) и 1 Render/кадр.
- **Implementation**: CancellationTokenSource отменяет предыдущий отложенный render, если за 16ms пришёл новый refresh.

### Decision 5: Viewport-микроны из ZoomPanManager
Добавить метод `GetViewportMicrons()` в ZoomPanManager, который возвращает видимую область в микронах модели.

- **Why**: Сейчас GridManager не знает о viewport (использует startX=0). Нужно, чтобы ZoomPanManager (который знает PanOffset, Zoom, viewport size) предоставлял эту информацию.
- **Alternative**: Вычислять в GridManager из публичных свойств ZoomPanManager — но это дублирует математику.

## Risks / Trade-offs

- **[Spec change] Существующий spec требует full-sheet grid** → Сознательно меняем поведение. Пользователь может заметить, что при быстром панорамировании сетка появляется на 1 кадр позже объектов. Mitigation: viewport margin в 1 экран скрывает эффект для медленного пана, debounce гарантирует render в том же кадре для обычного движения.
- **[Rejected: Simple doubling in step selection] Множители ×2 дают менее интуитивные шаги (16мм, 32мм)** → Используем 1/2/5-ряд.
- **[Pooling complexity] long[] менее читаем, чем List<GridNodeVm>** → Инкапсулируем в GridManager. GridNodesLayer получает ReadOnlySpan<long> — API остаётся чистым.

## Open Questions

- Нужен ли margin больше 1 экрана для viewport culling при очень быстром панорамировании?
- Какое значение maxNodes оптимально: 50K безопасно для всех форматов?
