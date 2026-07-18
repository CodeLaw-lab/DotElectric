# План перехода от ScrollViewer к RenderTransform (бесконечный холст)

**Дата:** 09.04.2026
**Статус:** ✅ РЕАЛИЗОВАН (09.04.2026 — Sprint 17)
**Приоритет:** P1 (Should Have)
**Сложность:** Средняя (затрагивает 6 файлов кода + тесты)

---

## 1. Цель

Удалить `ScrollViewer` из `EditorCanvas` и заменить механизм панорамирования на `RenderTransform` (TranslateTransform). Это обеспечит:

- **Бесконечный холст** — нет полос прокрутки, навигация только через Pan (средняя кнопка мыши / Space+Drag) и зум
- **Более плавное панорамирование** — RenderTransform работает на уровне композиции GPU, не вызывает layout pass
- **Упрощение архитектуры** — не нужно协调 ScrollViewer offset с координатами модели
- **Профессиональный UX** — как в настоящих CAD-системах (AutoCAD, Compass, KiCad)

---

## 2. Текущая архитектура

### 2.1. Визуальное дерево (сейчас)

```
EditorCanvas (UserControl)
  └─ ScrollViewer (MainScrollViewer)
       ├─ HorizontalScrollBarVisibility="Auto"
       ├─ VerticalScrollBarVisibility="Auto"
       └─ Canvas (DrawingCanvas)
            ├─ Width = Sheet.WidthMm × Zoom
            ├─ Height = Sheet.HeightMm × Zoom
            ├─ ItemsControl (сетка)
            ├─ ItemsControl (объекты шаблона)
            ├─ Preview-элементы (линия, прямоугольник, текст)
            └─ ContentControl (маркеры выделения)
```

### 2.2. Поток панорамирования (сейчас)

```
PanTool.OnMouseMove()
  → EditorViewModel.PanCanvas(deltaXMm, deltaYMm)
    → event PanRequested(dx, dy)
      → EditorCanvasBehavior.HandlePan()
        → scrollViewer.ScrollToHorizontalOffset/VerticalOffset()
```

### 2.3. Конвертация координат мыши (сейчас)

```csharp
// EditorCanvasBehavior.ToModelPoint()
var mmX = wpfPoint.X / zoom;
var mmY = wpfPoint.Y / zoom;
var modelMmY = sheetHeightMm - mmY;
return PointMicrons.FromMm(mmX, modelMmY);
```

**Проблема:** Pan через ScrollViewer требует отдельного `HandlePan()` с конвертацией мм → пиксели, что создаёт рассинхронизацию.

---

## 3. Целевая архитектура

### 3.1. Визуальное дерево (после)

```
EditorCanvas (UserControl)
  └─ Border (Background="#F0F0F0", ClipToBounds="True")
       └─ Canvas (DrawingCanvas)
            ├─ RenderTransform: TranslateTransform(X=PanOffsetX, Y=PanOffsetY)
            ├─ ClipToBounds="True"
            ├─ Width = Sheet.WidthMm × Zoom
            ├─ Height = Sheet.HeightMm × Zoom
            ├─ ItemsControl (сетка — оптимизированная)
            ├─ ItemsControl (объекты шаблона)
            ├─ Preview-элементы
            └─ ContentControl (маркеры выделения)
```

### 3.2. Поток панорамирования (после)

```
PanTool.OnMouseMove()
  → EditorViewModel.PanCanvas(deltaXMm, deltaYMm)
    → PanOffsetX += deltaXPixels
    → PanOffsetY -= deltaYPixels
    → OnPropertyChanged("PanOffsetX/Y")
      → WPF обновляет TranslateTransform (GPU, без layout)
```

### 3.3. Конвертация координат мыши (после)

```csharp
// EditorCanvasBehavior.ToModelPoint()
var adjustedX = (wpfPoint.X - viewModel.PanOffsetX) / zoom;
var adjustedY = (wpfPoint.Y - viewModel.PanOffsetY) / zoom;
var modelMmY = sheetHeightMm - adjustedY;
return PointMicrons.FromMm(adjustedX, modelMmY);
```

---

## 4. Детальный план реализации

### 4.1. EditorViewModel — добавить свойства PanOffset

**Файл:** `src/DotElectric.TemplateEditor/ViewModels/EditorViewModel.cs`

**Добавить:**

```csharp
/// <summary>
/// Смещение панорамирования по X в DIP (WPF-пикселях).
/// </summary>
[ObservableProperty]
private double _panOffsetX;

/// <summary>
/// Смещение панорамирования по Y в DIP (WPF-пикселях).
/// </summary>
[ObservableProperty]
private double _panOffsetY;
```

**Изменить `PanCanvas()`:**

```csharp
public void PanCanvas(double deltaXMm, double deltaYMm)
{
    var deltaXPixels = Coordinate.ToMm(deltaXMm) * Zoom; // мм → DIP
    var deltaYPixels = Coordinate.ToMm(deltaYMm) * Zoom;

    PanOffsetX += deltaXPixels;
    PanOffsetY -= deltaYPixels; // Y инвертирован
}
```

**Удалить:**
- `event Action<double, double>? PanRequested;` — больше не нужно

### 4.2. EditorCanvas.xaml — убрать ScrollViewer, добавить RenderTransform

**Файл:** `src/DotElectric.TemplateEditor/Views/EditorCanvas.xaml`

**Было:**
```xml
<ScrollViewer x:Name="MainScrollViewer"
              HorizontalScrollBarVisibility="Auto"
              VerticalScrollBarVisibility="Auto"
              Background="#F0F0F0">
    <Canvas x:Name="DrawingCanvas" ...>
        ...
    </Canvas>
</ScrollViewer>
```

**Стало:**
```xml
<!-- Фон UserControl -->
<Border Background="#F0F0F0" ClipToBounds="True">
    <Canvas x:Name="DrawingCanvas"
            behaviors:EditorCanvasBehavior.Editor="{Binding}"
            Background="Transparent"
            ClipToBounds="True">
        <Canvas.RenderTransform>
            <TranslateTransform X="{Binding PanOffsetX}"
                                Y="{Binding PanOffsetY}"/>
        </Canvas.RenderTransform>

        <!-- Сетка -->
        <ItemsControl ItemsSource="{Binding GridLines}">
            ...
        </ItemsControl>

        <!-- Объекты шаблона -->
        <ItemsControl ItemsSource="{Binding Template.Objects}">
            ...
        </ItemsControl>

        <!-- Preview-элементы -->
        ...

        <!-- Маркеры выделения -->
        <ContentControl Content="{Binding SingleSelectedObject}" ...>
            ...
        </ContentControl>
    </Canvas>
</Border>
```

**Ключевые изменения:**
1. Убран `ScrollViewer` полностью
2. Добавлен `Border` с `ClipToBounds` для фона
3. Canvas имеет `RenderTransform → TranslateTransform` с binding к `PanOffsetX/Y`
4. Canvas `Background="Transparent"` (вместо White) — для получения событий мыши

### 4.3. EditorCanvasBehavior — учесть PanOffset в ToModelPoint

**Файл:** `src/DotElectric.TemplateEditor/Behaviors/EditorCanvasBehavior.cs`

**Изменить `ToModelPoint()`:**

```csharp
private static PointMicrons ToModelPoint(Canvas canvas, EditorCanvasState state, Point wpfPoint)
{
    var editor = state.Editor;
    var zoom = editor.Zoom;
    var sheetHeightMm = editor.Template.Sheet.HeightMm;

    // Учитываем смещение панорамирования
    var adjustedX = (wpfPoint.X - editor.PanOffsetX) / zoom;
    var adjustedY = (wpfPoint.Y - editor.PanOffsetY) / zoom;

    // Инверсия Y (WPF: Y↓, модель: Y↑)
    var modelMmY = sheetHeightMm - adjustedY;

    // Округляем до микрон
    var micronsX = Coordinate.ToMicrons(adjustedX);
    var micronsY = Coordinate.ToMicrons(modelMmY);

    return new PointMicrons(micronsX, micronsY);
}
```

**Удалить:**
- `HandlePan()` — Pan теперь через `PanOffsetX/Y` напрямую
- `FindScrollViewer()` — не нужен
- `EditorCanvasState.ScrollViewer` — удалить свойство
- `EditorCanvasState.IsPanning` — можно оставить для состояния PanTool, но `HandlePan` больше не вызывается

**Упростить обработчики мыши:**

`State_MouseDown` для Pan:
```csharp
if (e.ChangedButton == MouseButton.Middle)
{
    var modelPoint = ToModelPoint(canvas, state, e.GetPosition(canvas));
    var panTool = state.Editor.GetOrCreateTool<PanTool>();
    panTool.OnMouseDown(modelPoint, MouseButton.Middle, Keyboard.Modifiers);
    state.IsPanning = true;
    e.Handled = true;
    return;
}
```

`State_MouseMove` для Pan — убрать вызов `HandlePan`:
```csharp
if (state.IsPanning)
{
    var modelPoint = ToModelPoint(canvas, state, e.GetPosition(canvas));
    var panTool = state.Editor.GetOrCreateTool<PanTool>();
    panTool.OnMouseMove(modelPoint, MouseButton.Middle, Keyboard.Modifiers);
    return;
}
```

### 4.4. PanTool — убрать зависимость от PanRequested

**Файл:** `src/DotElectric.TemplateEditor/Tools/PanTool.cs`

**Изменить `OnMouseMove()`:**

```csharp
public void OnMouseMove(PointMicrons modelPoint, MouseButton button, ModifierKeys modifiers)
{
    if (!_isPanning)
        return;

    // PanTool больше не вызывает PanRequested напрямую.
    // PanCanvas() в ViewModel обновляет PanOffsetX/Y,
    // которые через binding сдвигают TranslateTransform.
    var currentPosition = new Point(modelPoint.MicronsX / 1000.0, modelPoint.MicronsY / 1000.0);
    var delta = currentPosition - _lastMousePosition;

    _editor.PanCanvas(delta.X, delta.Y);

    _lastMousePosition = currentPosition;
}
```

**Фактически:** PanTool остаётся почти без изменений! Он уже вызывает `_editor.PanCanvas()`, который теперь обновляет `PanOffsetX/Y` вместо вызова события.

### 4.5. Сетка — замена линий на узлы (Grid Dots)

**Файлы:** `src/DotElectric.TemplateEditor/ViewModels/EditorViewModel.cs`, `src/DotElectric.TemplateEditor/Helpers/GridHelper.cs`, `src/DotElectric.TemplateEditor/ViewModels/GridLineVm.cs`, `src/DotElectric.TemplateEditor/Views/EditorCanvas.xaml`

**Текущее состояние:** Сетка отображается как набор линий (`GridLineVm` + `ItemsControl` с `<Line>`).

**Цель:** Заменить линии на точки (кружки) в узлах пересечения сетки — как в AutoCAD, KiCad, Compass.

**Изменения:**

#### 4.5.1. GridHelper — новый метод генерации узлов

Добавить метод `GenerateGridNodes()` вместо `GenerateGridLines/GenerateVisibleGridLines`:

```csharp
/// <summary>
/// Узел сетки — точка пересечения вертикальной и горизонтальной линий.
/// </summary>
public readonly struct GridNode
{
    /// <summary>Координата X узла в микронах.</summary>
    public long XMicrons { get; }

    /// <summary>Координата Y узла в микронах.</summary>
    public long YMicrons { get; }

    public GridNode(long xMicrons, long yMicrons)
    {
        XMicrons = xMicrons;
        YMicrons = yMicrons;
    }
}

/// <summary>
/// Сгенерировать узлы сетки для видимой области.
/// Вместо линий — только точки пересечения.
/// </summary>
public static List<GridNode> GenerateGridNodes(
    Sheet sheet,
    long stepMicrons,
    double zoom,
    long viewportLeftMicrons,
    long viewportBottomMicrons,
    long viewportWidthMicrons,
    long viewportHeightMicrons)
{
    var nodes = new List<GridNode>();

    if (stepMicrons <= 0 || zoom <= 0)
        return nodes;

    // Проверяем, не слишком ли мелкая сетка
    var pixelSpacing = Coordinate.ToMm(stepMicrons) * zoom;
    if (pixelSpacing < 15.0) // Минимум 15px между узлами для видимости
        return nodes;

    var viewportRight = Math.Min(viewportLeftMicrons + viewportWidthMicrons, sheet.WidthMicrons);
    var viewportTop = Math.Min(viewportBottomMicrons + viewportHeightMicrons, sheet.HeightMicrons);
    var viewportLeft = Math.Max(viewportLeftMicrons, 0);
    var viewportBottom = Math.Max(viewportBottomMicrons, 0);

    // Находим первый узел в видимой области
    var startX = ((viewportLeft + stepMicrons - 1) / stepMicrons) * stepMicrons;
    var startY = ((viewportBottom + stepMicrons - 1) / stepMicrons) * stepMicrons;

    for (long x = startX; x <= viewportRight; x += stepMicrons)
    {
        for (long y = startY; y <= viewportTop; y += stepMicrons)
        {
            nodes.Add(new GridNode(x, y));
        }
    }

    return nodes;
}
```

#### 4.5.2. GridNodeVm — ViewModel узла сетки

Заменить `GridLineVm` на `GridNodeVm`:

```csharp
public class GridNodeVm
{
    /// <summary>Координата X узла в микронах.</summary>
    public long XMicrons { get; init; }

    /// <summary>Координата Y узла в микронах.</summary>
    public long YMicrons { get; init; }

    /// <summary>Позиция X в пикселях (с учётом PanOffset и Zoom).</summary>
    public double XPixels { get; init; }

    /// <summary>Позиция Y в пикселях (с учётом PanOffset и Zoom).</summary>
    public double YPixels { get; init; }
}
```

#### 4.5.3. EditorViewModel.RefreshGridNodes()

Заменить `RefreshGridLines()` на `RefreshGridNodes()`:

```csharp
public ObservableCollection<GridNodeVm> GridNodes { get; } = new();

private void RefreshGridNodes()
{
    GridNodes.Clear();

    if (!GridSettings.Enabled || !GridSettings.Visible)
        return;

    var step = GridSettings.StepMicrons;

    // Размеры видимой области (весь лист, так как без ScrollViewer)
    var viewportLeftMm = PanOffsetX / Zoom;
    var viewportTopMm = PanOffsetY / Zoom;
    var viewportWidthMm = (CanvasWidthPixels) / Zoom;
    var viewportHeightMm = (CanvasHeightPixels) / Zoom;

    var viewportBottomMm = Template.Sheet.HeightMm - viewportTopMm - viewportHeightMm;

    var nodes = GridHelper.GenerateGridNodes(
        Template.Sheet,
        step,
        Zoom,
        Coordinate.ToMicrons(Math.Max(0, viewportLeftMm)),
        Coordinate.ToMicrons(Math.Max(0, viewportBottomMm)),
        Coordinate.ToMicrons(viewportWidthMm),
        Coordinate.ToMicrons(viewportHeightMm));

    foreach (var node in nodes)
    {
        // Позиция в пикселях = (мм * zoom) - PanOffset
        GridNodes.Add(new GridNodeVm
        {
            XMicrons = node.XMicrons,
            YMicrons = node.YMicrons,
            XPixels = Coordinate.ToMm(node.XMicrons) * Zoom - PanOffsetX,
            YPixels = Coordinate.ToMm(node.YMicrons) * Zoom - PanOffsetY,
        });
    }
}
```

#### 4.5.4. EditorCanvas.xaml — ItemsControl для узлов

Заменить ItemsControl сетки:

```xml
<!-- Узлы сетки (точки вместо линий) -->
<ItemsControl ItemsSource="{Binding GridNodes}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <Canvas IsHitTestVisible="False"/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Ellipse Fill="#C0C0C0" Width="2" Height="2"
                     Canvas.Left="{Binding XPixels}"
                     Canvas.Top="{Binding YPixels}"
                     IsHitTestVisible="False"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

#### 4.5.5. GridHelper — удалить старые методы

Удалить `GenerateGridLines()` и `GenerateVisibleGridLines()` — больше не нужны.

#### 4.5.6. Почему узлы вместо линий?

| Подход | Плюсы | Минусы |
|--------|-------|--------|
| Линии | Видна сетка при любом зуме | Тысячи линий, тормоза при большом листе |
| Узлы (точки) | Быстрее, чище UI, как в CAD | При мелком зуме узлы могут слиться |

**Порог видимости:** Узлы рисуются только если расстояние между ними ≥ 15px. При меньшем зуме — сетка скрывается полностью (не рисуется).

---

### 4.6. Сетка — оптимизация видимой области (ОБЪЕДИНЕНО с 4.5)

> Этот раздел объединён с 4.5. Логика GenerateVisibleGridLines заменена на GenerateGridNodes.

---

## 5. FitToScreen — сброс PanOffset

**Файл:** `src/DotElectric.TemplateEditor/ViewModels/EditorViewModel.cs`

**Изменить `FitToScreen()`:**

```csharp
public void FitToScreen(double canvasWidth, double canvasHeight)
{
    // Рассчитываем зум
    var sheetWidthMm = Coordinate.ToMm(Template.Sheet.WidthMicrons);
    var sheetHeightMm = Coordinate.ToMm(Template.Sheet.HeightMicrons);
    var padding = 0.9;
    var zoomW = (canvasWidth * padding) / sheetWidthMm;
    var zoomH = (canvasHeight * padding) / sheetHeightMm;
    Zoom = Math.Min(zoomW, zoomH);

    // Сбрасываем панорамирование — центрируем лист
    PanOffsetX = 0;
    PanOffsetY = 0;
}
```

### 4.7. Удалить неиспользуемый код

| Файл | Удалить |
|------|---------|
| `EditorCanvasBehavior.cs` | `HandlePan()`, `FindScrollViewer()`, `EditorCanvasState.ScrollViewer` |
| `EditorViewModel.cs` | `event PanRequested` |
| `EditorCanvas.xaml.cs` | Проверить, нет ли ссылок на `MainScrollViewer` |

---

## 5. Тесты

### 5.1. Изменить существующие тесты

| Файл | Тесты | Изменение |
|------|-------|-----------|
| `PanToolTests.cs` | Все 11 тестов | `PanRequested` → проверка `PanOffsetX/Y` |
| `EditorViewModelTests.cs` | `FitToScreen`, `PanCanvas` | Проверка `PanOffsetX/Y` вместо подписки на событие |
| `GridHelperTests.cs` | Тесты на GenerateGridLines | Заменить на GenerateGridNodes |

### 5.2. Добавить новые тесты

| Файл | Тест | Что проверяет |
|------|------|---------------|
| `EditorViewModelTests.cs` | `PanCanvas_UpdatesPanOffset` | PanCanvas обновляет PanOffsetX/Y |
| `EditorViewModelTests.cs` | `PanCanvas_MultipleCalls_Accumulates` | Многократный Pan накапливается |
| `EditorViewModelTests.cs` | `FitToScreen_ResetsPanOffset` | FitToScreen сбрасывает PanOffset |
| `EditorViewModelTests.cs` | `PanOffset_DefaultValue_IsZero` | По умолчанию PanOffset = 0 |
| `EditorCanvasBehaviorTests.cs` | `ToModelPoint_AccountsForPanOffset` | ToModelPoint учитывает PanOffset |
| `GridHelperTests.cs` | `GenerateGridNodes_ReturnsIntersections` | Узлы на пересечениях сетки |
| `GridHelperTests.cs` | `GenerateGridNodes_HidesWhenTooSmall` | Узлы скрыты при мелком зуме |
| `GridHelperTests.cs` | `GenerateGridNodes_OnlyVisibleArea` | Только узлы видимой области |

### 5.3. Ожидаемые метрики

| Метрика | Сейчас | После |
|---------|--------|-------|
| Всего тестов | 1221 | ~1229 (+8 новых) |
| Сбоев | 0 | 0 |
| Покрытие | 66.2% | ≥ 66.2% |

---

## 6. Риски и митигация

| Риск | Вероятность | Влияние | Митигация |
|------|-------------|---------|-----------|
| Рассинхронизация PanOffset с реальной позицией | Средняя | Высокое | Тщательное тестирование ToModelPoint с PanOffset |
| Сетка рисуется за пределами видимости | Средняя | Среднее | Использовать GenerateVisibleGridLines |
| RenderTransform вызывает проблемы с hit-testing | Низкая | Высокое | ClipToBounds=True на Canvas |
| Потеря данных при большом Pan | Низкая | Низкое | PanOffset — double, диапазон огромный |
| Обратная совместимость с .tdel | Нет | Нет | PanOffset НЕ сериализуется (состояние сессии) |

---

## 7. Порядок выполнения

| Шаг | Задача | Файлы | Оценка |
|-----|--------|-------|--------|
| 1 | Добавить `PanOffsetX/Y` в EditorViewModel | `EditorViewModel.cs` | 15 мин |
| 2 | Изменить `PanCanvas()` — обновлять PanOffset вместо события | `EditorViewModel.cs` | 10 мин |
| 3 | Убрать ScrollViewer из XAML, добавить RenderTransform | `EditorCanvas.xaml` | 20 мин |
| 4 | Обновить `ToModelPoint()` — учитывать PanOffset | `EditorCanvasBehavior.cs` | 15 мин |
| 5 | Удалить `HandlePan`, `FindScrollViewer`, `ScrollViewer` state | `EditorCanvasBehavior.cs` | 15 мин |
| 6 | Заменить GenerateGridLines → GenerateGridNodes | `GridHelper.cs` | 30 мин |
| 7 | Заменить GridLineVm → GridNodeVm | `GridLineVm.cs` | 15 мин |
| 8 | Обновить сетку в XAML — Ellipse вместо Line | `EditorCanvas.xaml` | 15 мин |
| 9 | Обновить `RefreshGridLines()` → `RefreshGridNodes()` | `EditorViewModel.cs` | 20 мин |
| 10 | Обновить `FitToScreen()` — сброс PanOffset + обновление узлов | `EditorViewModel.cs` | 5 мин |
| 11 | Обновить тесты PanTool | `PanToolTests.cs` | 20 мин |
| 12 | Обновить тесты GridHelper | `GridHelperTests.cs` | 20 мин |
| 13 | Добавить новые тесты (8 штук) | `EditorViewModelTests.cs`, `EditorCanvasBehaviorTests.cs`, `GridHelperTests.cs` | 40 мин |
| 14 | Запустить все тесты, исправить сбои | — | 20 мин |

**Общее время:** ~4.5 часа

---

## 8. Критерии приёмки

- [ ] ScrollViewer удалён из EditorCanvas.xaml
- [ ] Pan работает через TranslateTransform
- [ ] При Pan объекты и узлы сетки двигаются синхронно
- [ ] Привязка к сетке работает корректно при любом PanOffset
- [ ] Сетка отображается как точки (узлы), а не линии
- [ ] Узлы скрыты при слишком мелком зуме (< 15px между узлами)
- [ ] FitToScreen сбрасывает PanOffset и центрирует лист
- [ ] Все тесты проходят (1229+, 0 сбоев)
- [ ] Нет утечек памяти (PanOffset НЕ накапливает GC-мусор)
- [ ] Маркеры выделения остаются на своих местах при Pan

---

## 9. Известные ограничения

1. **PanOffset не сохраняется** — при перезагрузке файла лист возвращается в начальную позицию. Это осознанное решение (состояние сессии, не документа).

2. **Нет ограничений на Pan** — пользователь может "уехать" далеко за пределы листа. Это соответствует парадигме бесконечного холста. При желании можно добавить ограничения позже.

3. **Узлы сетки при очень большом Pan** — если уехать далеко за пределы листа, узлы перестанут рисоваться (GenerateGridNodes ограничена рамками листа).

4. **Узлы вместо линий** — при среднем зуме сетка видна только как точки. Если пользователю нужны линии — можно добавить настройку "Тип сетки: точки/линии" позже.

---

**Документ готов к реализации. Ожидает команды от заказчика.**
