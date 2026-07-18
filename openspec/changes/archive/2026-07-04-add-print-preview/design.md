## Context

В редакторе DotElectric печать реализована через `IPrintService.PrintWithVisual()`, который отправляет `DrawingCanvas` целиком (включая сетку, маркеры выделения, preview-призраки) на `PrintDialog`. Предпросмотр отсутствует — пункт меню «Предпросмотр печати» существует как заглушка без `Command`.

Нужно добавить preview, который показывает только лист + объекты, без UI-элементов редактора.

## Goals / Non-Goals

**Goals:**
- Показать пользователю, как шаблон будет выглядеть на бумаге, до отправки на принтер
- Отображать только лист и нарисованные объекты (Line, Rectangle, Text)
- Использовать `DocumentViewer` с встроенным zoom, пагинацией и кнопкой «Печать»
- Кнопка «Печать» открывает стандартный `PrintDialog`
- Сохранить существующий `IPrintService.PrintWithVisual()` без изменений

**Non-Goals:**
- Тайлинг (разбивка большого листа на несколько страниц A4) — отложен
- Кастомный UI вместо DocumentViewer — используем встроенный тулбар
- Изменение существующего Ctrl+P / `PrintCommand`
- Печать нескольких вкладок одновременно

## Decisions

### 1. FixedDocument вместо DrawingVisual/DrawingContext

**Решение:** Генерировать `FixedDocument` с WPF-элементами (Line, Rectangle, TextBlock) на `FixedPage`.

**Почему:**
- `FixedDocument` — стандартный WPF-контейнер для print preview, поддерживается `DocumentViewer` и `XpsDocumentWriter`
- WPF-элементы (Line, Rectangle, TextBlock) имеют те же визуальные характеристики, что и в EditorCanvas — одинаковый рендеринг текста, dash-стилей, цветов
- DocumentViewer предоставляет zoom, пагинацию и кнопку «Печать» бесплатно
- Text через `TextBlock` корректно обрабатывает `LayoutTransform` (поворот), `TextWrapping`, `TextAlignment`, шрифты ГОСТ — без необходимости пересчитывать геометрию вручную

**Альтернативы, которые рассматривали:**
- **DrawingContext** — быстрее, но требует ручного рендеринга текста через `FormattedText`, что даёт отличия в layout и не поддерживает `LayoutTransform` для поворота
- **RenderTargetBitmap (snapshot очищенного Canvas)** — растровое качество, fragile (состояние гонки при layout pass), не тестируемо

### 2. Печать через встроенную кнопку DocumentViewer

**Решение:** Использовать `DocumentViewer.PrintCommand`, который открывает стандартный `PrintDialog`.

**Почему:**
- Не требует написания своего диалога выбора принтера
- Стандартный Windows PrintDialog даёт выбор принтера, копий, ориентации
- Пользователь уже знаком с этим диалогом (он же используется в Ctrl+P)
- DocumentViewer автоматически масштабирует содержимое под страницу при печати

### 3. FitToPage для всех листов независимо от формата

**Решение:** DocumentViewer отображает `FixedDocument` в режиме FitToPage, ужимая любой лист в окно просмотра.

**Почему:**
- Если лист A0, а экран 1920×1080, без масштабирования пользователь видит только угол
- DocumentViewer позволяет в любой момент изменить zoom через ползунок
- При печати PrintDialog сам применяет FitToPage через настройки принтера

### 4. Новый сервис IPrintDocumentGenerator, а не расширение существующего IPrintService

**Решение:** Создать отдельный интерфейс и реализацию, не изменяя `IPrintService`.

**Почему:**
- Существующий `IPrintService` работает с `Visual` (WPF-зависимый), а новый сервис работает с `Template` (модель)
- Разные ответственности: `IPrintService` — отправить Visual на печать; `IPrintDocumentGenerator` — построить `FixedDocument` из модели
- Не нарушаем существующие тесты `PrintService` (19 тестов)

### 5. Transient vs Singleton для генератора

**Решение:** `Transient` регистрация — новый экземпляр на каждый вызов.

**Почему:**
- `PrintDocumentGenerator` — stateless: принимает `Template` и `Size`, возвращает `FixedDocument`
- Нет внутреннего состояния, которое нужно разделять между вызовами
- Transient проще и безопаснее (не нужно думать о thread safety)

## Data Flow

```
MainWindow.xaml
  ┌─ MenuItem "Предпросмотр печати" ─── MainViewModel.PreviewCommand
  └─ KeyBinding Ctrl+Shift+P          (Ctrl+Shift+P)
          │
          ▼
  MainViewModel.Preview()
    │  var template = SelectedTab.Template
    │  var generator = _printDocGenerator
    │  var doc = generator.BuildDocument(template, pageSize)
    │  PrintPreviewWindow.Show(doc)
          │
          ▼
  PrintDocumentGenerator.BuildDocument(Template, Size)
    │  var fixedDoc = new FixedDocument()
    │  var page = new FixedPage() { Width = pageW, Height = pageH }
    │  // Sheet border
    │  page.Children.Add(new Rectangle { ... })
    │  // Objects
    │  foreach (obj in template.Objects):
    │    switch obj:
    │      Line      → add Line element                     (Y-flip, dash, color, thickness)
    │      Rectangle → add Rectangle element                 (Y-flip, dash, stroke, fill, thickness)
    │      Text      → add TextBlock element                 (Y-flip, fontSize, fontName, rotation, alignment, wrapping, color)
    │  fixedDoc.Pages.Add(new PageContent { Child = page })
    │  return fixedDoc
          │
          ▼
  PrintPreviewWindow (DocumentViewer)
    │  DocumentViewer.Document = fixedDoc
    │  DocumentViewer.InvalidateVisual()
    │
    ├── [🔍 zoom] → встроенный ползунок
    ├── [◀ ▶]     → встроенная пагинация
    └── [🖨]      → встроенная команда → стандартный PrintDialog
```

## Coordinate Conversion

Для каждого объекта применяется та же математика Y-flip, что и в конвертерах EditorCanvas:

```
wpfX = micronsX / 1000.0                        // микроны → мм
wpfY = (sheetHeightMm - micronsY / 1000.0)      // Y-flip (декартова → WPF)
```

Для Line: позиционирование через `Canvas.Left/Top` на минимальную X/Y + локальные координаты линий.

Для Rectangle: `Canvas.Left/Top` = (X, Y+H) (левый верхний угол в WPF), `Width/Height` = (W, H).

Для Text: `Canvas.Left/Top` = (X, Y+H) (верхняя базовая линия → верхний край TextBlock).

## Object Rendering Details

### Line
```
Line wpfLine = new()
{
    X1 = coord1 - minX,         // локальные координаты
    Y1 = maxY - coord1,
    X2 = coord2 - minX,
    Y2 = maxY - coord2,
    Stroke = HexToBrush(strokeColor),
    StrokeThickness = strokeThicknessMicrons / 1000.0,  // мм → WPF units
    StrokeDashArray = LineTypeToDashArray(lineType),
};
Canvas.SetLeft(wpfLine, minX_mm);
Canvas.SetTop(wpfLine, sheetH_mm - maxY_mm);
```

### Rectangle
```
Rectangle wpfRect = new()
{
    Width = widthMm,
    Height = heightMm,
    Stroke = HexToBrush(strokeColor),
    Fill = HexToBrush(fillColor),
    StrokeThickness = strokeThicknessMm,
    StrokeDashArray = LineTypeToDashArray(lineType),
};
Canvas.SetLeft(wpfRect, xMm);
Canvas.SetTop(wpfRect, sheetH_mm - (yMm + heightMm));
```

### Text
```
TextBlock wpfText = new()
{
    Text = content,
    FontFamily = FontNameToFamily(fontName),       // через существующий конвертер
    FontSize = fontSizeMm * 96.0 / 25.4,            // мм → WPF points (1pt = 1/72in ≈ 0.3528mm)
    Foreground = HexToBrush(foreground),
    TextWrapping = textWrapping ? TextWrapping.Wrap : TextWrapping.NoWrap,
    TextAlignment = ParseAlignment(textAlignment),
};
wpfText.LayoutTransform = new RotateTransform(rotationAngle);
Canvas.SetLeft(wpfText, xMm);
Canvas.SetTop(wpfText, sheetH_mm - (yMm + textHeightMm));
```

**Где взять textHeightMm:**
`FontSizeMicrons / 1000.0` — аппроксимация. WPF TextBlock сам рассчитает точную высоту при layout. Для Canvas.Top используется приближение, чтобы тексты не съезжали по вертикали. Приближение достаточно точное для ГОСТ-шрифтов (моноширинных).

## Risks / Trade-offs

- **[Text height approximation]** → `Canvas.Top` для текста использует `fontSizeMm` как высоту строки. Для точного совпадения с EditorCanvas нужно использовать `TopEdgeMicronsMultiConverter` логику. Отличие будет в пределах 1-2 пикселей для типичных размеров ГОСТ-шрифтов.
- **[WPF units vs mm]** → `FixedPage` использует WPF-единицы (1/96 дюйма = 0.2646 мм), а модель хранит мм. Коэффициент `96/25.4` конвертирует мм → WPF units. Погрешность < 1 микрон.
- **[No tiling]** → Если лист больше экрана/бумаги, DocumentViewer покажет его уменьшенным (FitToPage). При печати на A4 принтере лист A0 будет напечатан на одном листе в масштабе. Тайлинг — следующий PR.
