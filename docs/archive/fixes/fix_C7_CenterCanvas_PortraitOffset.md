# C7: Смещение листа A0 книжного вправо при центрировании

**Дата:** 21.06.2026
**Статус:** Исправлено ✅
**Связанные файлы:**
- `ViewModels/Managers/ZoomPanManager.cs` (CenterCanvas)
- `Views/EditorCanvas.xaml` (InlineTextEditor binding)

---

## Проблема

### 1. Смещение листа вправо (CenterCanvas)

При создании A0 книжного (841×1189 мм) лист отображался со смещением к правой стороне рабочей области.

**Корневая причина:** `ZoomPanManager.CenterCanvas()` в else-ветке безусловно применял формулу `(canvasPx − viewportPx) / 2` для обеих осей, даже когда холст по ширине **меньше** вьюпорта.

Для A0 книжного в типичном вьюпорте 1200×800 px:
- `CanvasWidthPixels` (841) < `ViewportWidthPixels` (1200) → формула даёт **отрицательный** `PanOffsetX` = −179.5
- `CanvasOffsetX = -PanOffsetX = +179.5` → `TranslateTransform` сдвигает холст **вправо** на 179.5 px

**Результат:** лист прижимается к правому краю, слева — пустота.

### 2. InlineTextEditor Canvas.Top

`EditorCanvas.xaml:329` — привязка `Canvas.Top` для InlineTextEditor использовала `Template.Sheet.HeightMicrons` (`long`) вместо `Template.Sheet.HeightMm` (`double`). Конвертер `ModelYToCanvasTopConverter` ожидает `double` для высоты листа; на `long` срабатывает `is not double` → возвращается `0.0`.

---

## Решение

### CenterCanvas — `ZoomPanManager.cs:117-118`

```csharp
// Было
PanOffsetX = (CanvasWidthPixels - ViewportWidthPixels) / 2;
PanOffsetY = (CanvasHeightPixels - ViewportHeightPixels) / 2;

// Стало
PanOffsetX = Math.Max(0, (CanvasWidthPixels - ViewportWidthPixels) / 2);
PanOffsetY = Math.Max(0, (CanvasHeightPixels - ViewportHeightPixels) / 2);
```

`Math.Max(0, ...)` гарантирует, что оффсет применяется только для измерений, где canvas **реально превышает** viewport. Для измерений, где canvas помещается, оффсет = 0, и центрирование остаётся на WPF-выравнивании (`HorizontalAlignment="Center"`).

### InlineTextEditor — `EditorCanvas.xaml:329`

```xml
<!-- Было -->
<Binding Path="Template.Sheet.HeightMicrons"/>

<!-- Стало -->
<Binding Path="Template.Sheet.HeightMm"/>
```

---

## Тесты

Добавлены в `IntegrationTests.cs`:
- `CreateTemplate_A0Portrait_HasCorrectDimensions` — 841×1189 мм
- `CreateTemplate_A0Landscape_HasCorrectDimensions` — 1189×841 мм

**Результат:** 0 ошибок, 287/287 тестов пройдено (включая все ZoomPanManager + EditorViewModel тесты).
